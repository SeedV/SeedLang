// Copyright 2021-2022 The SeedV Lab.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Diagnostics;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class Compiler : AstWalker {
    private NestedFuncStack _nestedFuncStack;
    private VariableResolver _variableResolver;

    // The register allocated for the result of sub-expressions.
    private uint _registerForSubExpr;
    // The next boolean operator. A true condition check instruction is emitted if the next boolean
    // operator is "And", otherwise a false condition instruction check is emitted.
    private BooleanOperator _nextBooleanOp;
    // A list to collect all true short circuit jumps.
    private readonly List<int> _trueShortCircuitJumps = new List<int>();
    // A list to collect all false short circuit jumps.
    private readonly List<int> _falseShortCircuitJumps = new List<int>();

    // The chunk on the top of the function stack.
    private Chunk _chunk;
    // The constant cache on the top of the function stack.
    private ConstantCache _constantCache;

    internal Function Compile(AstNode node, GlobalEnvironment env) {
      _nestedFuncStack = new NestedFuncStack();
      _variableResolver = new VariableResolver(env);
      // Starts to parse the main function in the global scope.
      _nestedFuncStack.PushFunc("main");
      CacheTopFunction();
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0u, null);
      return _nestedFuncStack.PopFunc();
    }

    protected override void Visit(BinaryExpression binary) {
      _variableResolver.BeginExpressionScope();
      uint register = _registerForSubExpr;
      uint left = VisitExpression(binary.Left);
      uint right = VisitExpression(binary.Right);
      _chunk.Emit(OpcodeOfBinaryOperator(binary.Op), register, left, right, binary.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(UnaryExpression unary) {
      _variableResolver.BeginExpressionScope();
      uint register = _registerForSubExpr;
      uint expr = VisitExpression(unary.Expr);
      _chunk.Emit(Opcode.UNM, register, expr, 0, unary.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(BooleanExpression boolean) {
      BooleanOperator nextBooleanOp = _nextBooleanOp;
      for (int i = 0; i < boolean.Exprs.Length; i++) {
        _nextBooleanOp = i < boolean.Exprs.Length - 1 ? boolean.Op : nextBooleanOp;
        Visit(boolean.Exprs[i]);
        if (i < boolean.Exprs.Length - 1) {
          switch (boolean.Op) {
            case BooleanOperator.And:
              PatchJumps(_trueShortCircuitJumps);
              break;
            case BooleanOperator.Or:
              PatchJumps(_falseShortCircuitJumps);
              break;
          }
        }
      }
    }

    protected override void Visit(ComparisonExpression comparison) {
      Debug.Assert(comparison.Ops.Length > 0 && comparison.Exprs.Length > 0);
      _variableResolver.BeginExpressionScope();
      BooleanOperator nextBooleanOp = _nextBooleanOp;
      Expression left = comparison.First;
      for (int i = 0; i < comparison.Exprs.Length; i++) {
        _nextBooleanOp = i < comparison.Exprs.Length - 1 ? BooleanOperator.And : nextBooleanOp;
        VisitSingleComparison(left, comparison.Ops[i], comparison.Exprs[i], comparison.Range);
        left = comparison.Exprs[i];
      }
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(IdentifierExpression identifier) {
      if (_variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info) {
        switch (info.Type) {
          case VariableResolver.VariableType.Global:
            _chunk.Emit(Opcode.GETGLOB, _registerForSubExpr, info.Id, identifier.Range);
            break;
          case VariableResolver.VariableType.Local:
            _chunk.Emit(Opcode.MOVE, _registerForSubExpr, info.Id, 0, identifier.Range);
            break;
          case VariableResolver.VariableType.Upvalue:
            // TODO: handle upvalues.
            break;
        }
      } else {
        // TODO: throw a variable not defined runtime error.
      }
    }

    protected override void Visit(NoneConstantExpression noneConstant) {
      throw new System.NotImplementedException();
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      throw new System.NotImplementedException();
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      uint id = _constantCache.IdOfConstant(numberConstant.Value);
      _chunk.Emit(Opcode.LOADK, _registerForSubExpr, id, numberConstant.Range);
    }

    protected override void Visit(StringConstantExpression stringConstant) {
      throw new System.NotImplementedException();
    }

    protected override void Visit(ListExpression list) {
      var call = Expression.Call(Expression.Identifier("list", list.Range), list.Exprs, list.Range);
      Visit(call);
    }

    protected override void Visit(SubscriptExpression subscript) {
      _variableResolver.BeginExpressionScope();
      uint targetId = _registerForSubExpr;
      uint listId;
      if (GetRegisterId(subscript.Expr) is uint list) {
        listId = list;
      } else {
        listId = _variableResolver.AllocateVariable();
        _registerForSubExpr = listId;
        Visit(subscript.Expr);
      }
      uint indexId = VisitExpression(subscript.Index);
      _chunk.Emit(Opcode.GETELEM, targetId, listId, indexId, subscript.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(CallExpression call) {
      _variableResolver.BeginExpressionScope();
      // TODO: should call.Func always be IdentifierExpression?
      if (call.Func is IdentifierExpression identifier) {
        if (_variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info) {
          uint resultRegister = _registerForSubExpr;
          bool needRegister = resultRegister != _variableResolver.LastRegister;
          uint funcRegister = needRegister ? _variableResolver.AllocateVariable() : resultRegister;
          switch (info.Type) {
            case VariableResolver.VariableType.Global:
              _chunk.Emit(Opcode.GETGLOB, funcRegister, info.Id, identifier.Range);
              break;
            case VariableResolver.VariableType.Local:
              _chunk.Emit(Opcode.MOVE, funcRegister, info.Id, 0, identifier.Range);
              break;
            case VariableResolver.VariableType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          foreach (Expression expr in call.Arguments) {
            _registerForSubExpr = _variableResolver.AllocateVariable();
            Visit(expr);
          }
          _chunk.Emit(Opcode.CALL, funcRegister, (uint)call.Arguments.Length, 0, call.Range);
          if (needRegister) {
            _chunk.Emit(Opcode.MOVE, resultRegister, funcRegister, 0, call.Range);
          }
        } else {
          // TODO: throw a variable not defined runtime error.
        }
      }
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(AssignmentStatement assignment) {
      switch (assignment.Target) {
        case IdentifierExpression identifier:
          string name = identifier.Name;
          if (_variableResolver.FindVariable(name) is null) {
            _variableResolver.DefineVariable(name);
          }
          VariableResolver.VariableInfo info = _variableResolver.FindVariable(name).Value;
          switch (info.Type) {
            case VariableResolver.VariableType.Global:
              _variableResolver.BeginExpressionScope();
              uint resultRegister = _variableResolver.AllocateVariable();
              _registerForSubExpr = resultRegister;
              Visit(assignment.Expr);
              _chunk.Emit(Opcode.SETGLOB, resultRegister, info.Id, assignment.Range);
              _variableResolver.EndExpressionScope();
              break;
            case VariableResolver.VariableType.Local:
              if (GetRegisterId(assignment.Expr) is uint registerId) {
                _chunk.Emit(Opcode.MOVE, info.Id, registerId, 0, assignment.Range);
              } else if (GetConstantId(assignment.Expr) is uint constantId) {
                _chunk.Emit(Opcode.LOADK, info.Id, constantId, assignment.Range);
              } else {
                _registerForSubExpr = info.Id;
                Visit(assignment.Expr);
              }
              break;
            case VariableResolver.VariableType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          break;
        case SubscriptExpression subscript:
          _variableResolver.BeginExpressionScope();
          uint listId;
          if (GetRegisterId(subscript.Expr) is uint list) {
            listId = list;
          } else {
            listId = _variableResolver.AllocateVariable();
            _registerForSubExpr = listId;
            Visit(subscript.Expr);
          }
          uint indexId = VisitExpression(subscript.Index);
          uint exprId = VisitExpression(assignment.Expr);
          _chunk.Emit(Opcode.SETELEM, listId, indexId, exprId, subscript.Range);
          _variableResolver.EndExpressionScope();
          break;
      }
    }

    protected override void Visit(BlockStatement block) {
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void Visit(ExpressionStatement expr) {
      if (GetRegisterId(expr.Expr) is uint id) {
        _chunk.Emit(Opcode.EVAL, id, expr.Range);
      } else {
        _variableResolver.BeginExpressionScope();
        id = _variableResolver.AllocateVariable();
        _registerForSubExpr = id;
        Visit(expr.Expr);
        _variableResolver.EndExpressionScope();
        _chunk.Emit(Opcode.EVAL, id, expr.Range);
      }
    }

    protected override void Visit(FuncDefStatement funcDef) {
      VariableResolver.VariableInfo info = _variableResolver.DefineVariable(funcDef.Name);
      PushFunc(funcDef.Name);
      foreach (string parameterName in funcDef.Parameters) {
        _variableResolver.DefineVariable(parameterName);
      }
      Visit(funcDef.Body);
      Function func = PopFunc();
      uint funcId = _constantCache.IdOfConstant(func);
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          _variableResolver.BeginExpressionScope();
          uint registerId = _variableResolver.AllocateVariable();
          _chunk.Emit(Opcode.LOADK, registerId, funcId, funcDef.Range);
          _chunk.Emit(Opcode.SETGLOB, registerId, info.Id, funcDef.Range);
          _variableResolver.EndExpressionScope();
          break;
        case VariableResolver.VariableType.Local:
          _chunk.Emit(Opcode.LOADK, info.Id, funcId, funcDef.Range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    protected override void Visit(IfStatement @if) {
      Visit(@if.Test);
      PatchJumps(_trueShortCircuitJumps);
      Visit(@if.ThenBody);
      if (!(@if.ElseBody is null)) {
        _chunk.Emit(Opcode.JMP, 0, @if.Range);
        int jumpEnd = GetCurrentCodePos();
        PatchJumps(_falseShortCircuitJumps);
        Visit(@if.ElseBody);
        PatchJump(jumpEnd);
      } else {
        PatchJumps(_falseShortCircuitJumps);
      }
    }

    protected override void Visit(ReturnStatement @return) {
      if (!(GetRegisterId(@return.Result) is uint result)) {
        _variableResolver.BeginExpressionScope();
        result = _variableResolver.AllocateVariable();
        _registerForSubExpr = result;
        Visit(@return.Result);
        _variableResolver.EndExpressionScope();
      }
      _chunk.Emit(Opcode.RETURN, result, @return.Range);
    }

    protected override void Visit(WhileStatement @while) {
      int start = _chunk.Bytecode.Count;
      Visit(@while.Test);
      Visit(@while.Body);
      _chunk.Emit(Opcode.JMP, start - (_chunk.Bytecode.Count + 1), @while.Range);
      PatchJumps(_falseShortCircuitJumps);
    }

    private void VisitSingleComparison(Expression left, ComparisonOperator op, Expression right,
                                       Range range) {
      uint leftRegister = VisitExpression(left);
      uint rightRegister = VisitExpression(right);
      (Opcode opcode, bool checkFlag) = OpcodeAndCheckFlagOfComparisonOperator(op);
      if (_nextBooleanOp == BooleanOperator.Or) {
        checkFlag = !checkFlag;
      }
      _chunk.Emit(opcode, checkFlag ? 1u : 0u, leftRegister, rightRegister, range);
      _chunk.Emit(Opcode.JMP, 0, range);
      int jump = GetCurrentCodePos();
      switch (_nextBooleanOp) {
        case BooleanOperator.And:
          _falseShortCircuitJumps.Add(jump);
          break;
        case BooleanOperator.Or:
          _trueShortCircuitJumps.Add(jump);
          break;
      }
    }

    private int GetCurrentCodePos() {
      return _chunk.Bytecode.Count - 1;
    }

    private void PatchJumps(List<int> jumps) {
      foreach (int jump in jumps) {
        PatchJump(jump);
      }
      jumps.Clear();
    }

    private void PatchJump(int jump) {
      _chunk.PatchJumpAt(jump, _chunk.Bytecode.Count - jump - 1);
    }

    private void PushFunc(string name) {
      _nestedFuncStack.PushFunc(name);
      CacheTopFunction();
      _variableResolver.BeginFunctionScope();
    }

    private Function PopFunc() {
      _variableResolver.EndFunctionScope();
      Function func = _nestedFuncStack.PopFunc();
      CacheTopFunction();
      return func;
    }

    private void CacheTopFunction() {
      _chunk = _nestedFuncStack.CurrentChunk();
      _constantCache = _nestedFuncStack.CurrentConstantCache();
    }

    private uint VisitExpression(Expression expr) {
      if (!(GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateVariable();
        _registerForSubExpr = exprId;
        Visit(expr);
      }
      return exprId;
    }

    private uint? GetRegisterId(Expression expr) {
      if (expr is IdentifierExpression identifier &&
          _variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info &&
          info.Type == VariableResolver.VariableType.Local) {
        return info.Id;
      }
      return null;
    }

    private uint? GetConstantId(Expression expr) {
      switch (expr) {
        case NumberConstantExpression number:
          return _constantCache.IdOfConstant(number.Value);
        case StringConstantExpression str:
          return _constantCache.IdOfConstant(str.Value);
        default:
          return null;
      }
    }

    private uint? GetRegisterOrConstantId(Expression expr) {
      if (GetRegisterId(expr) is uint registerId) {
        return registerId;
      } else if (GetConstantId(expr) is uint constantId) {
        return constantId;
      }
      return null;
    }

    private static Opcode OpcodeOfBinaryOperator(BinaryOperator op) {
      switch (op) {
        case BinaryOperator.Add:
          return Opcode.ADD;
        case BinaryOperator.Subtract:
          return Opcode.SUB;
        case BinaryOperator.Multiply:
          return Opcode.MUL;
        case BinaryOperator.Divide:
          return Opcode.DIV;
        default:
          throw new System.NotImplementedException($"Operator {op} not implemented.");
      }
    }

    private static (Opcode, bool) OpcodeAndCheckFlagOfComparisonOperator(ComparisonOperator op) {
      switch (op) {
        case ComparisonOperator.Less:
          return (Opcode.LT, true);
        case ComparisonOperator.Greater:
          return (Opcode.LE, false);
        case ComparisonOperator.LessEqual:
          return (Opcode.LE, true);
        case ComparisonOperator.GreaterEqual:
          return (Opcode.LT, false);
        case ComparisonOperator.EqEqual:
          return (Opcode.EQ, true);
        case ComparisonOperator.NotEqual:
          return (Opcode.EQ, false);
        default:
          throw new System.NotImplementedException($"Operator {op} not implemented.");
      }
    }
  }
}
