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
    private RunMode _runMode;
    private VariableResolver _variableResolver;
    private NestedFuncStack _nestedFuncStack;
    private NestedJumpStack _nestedJumpStack;

    // The register allocated for the result of sub-expressions. It must be set before visiting
    // sub-expressions.
    private uint _registerForSubExpr;
    // The next boolean operator. A true condition check instruction is emitted if the next boolean
    // operator is "And", otherwise a false condition instruction check is emitted.
    private BooleanOperator _nextBooleanOp;

    // The chunk on the top of the function stack.
    private Chunk _chunk;
    // The constant cache on the top of the function stack.
    private ConstantCache _constantCache;

    internal Function Compile(AstNode node, GlobalEnvironment env, RunMode runMode) {
      _runMode = runMode;
      _variableResolver = new VariableResolver(env);
      _nestedFuncStack = new NestedFuncStack();
      _nestedJumpStack = new NestedJumpStack();
      // Starts to parse the main function in the global scope.
      _nestedFuncStack.PushFunc("main");
      CacheTopFunction();
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0, 0, 0, null);
      return _nestedFuncStack.PopFunc();
    }

    protected override void Visit(BinaryExpression binary) {
      _variableResolver.BeginExpressionScope();
      uint register = _registerForSubExpr;
      uint left = VisitExpressionForRKId(binary.Left);
      uint right = VisitExpressionForRKId(binary.Right);
      _chunk.Emit(OpcodeOfBinaryOperator(binary.Op), register, left, right, binary.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(UnaryExpression unary) {
      _variableResolver.BeginExpressionScope();
      uint register = _registerForSubExpr;
      uint expr = VisitExpressionForRKId(unary.Expr);
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
              PatchJumps(_nestedJumpStack.TrueJumps);
              break;
            case BooleanOperator.Or:
              PatchJumps(_nestedJumpStack.FalseJumps);
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
        throw new DiagnosticException(SystemReporters.SeedInterpreter, Severity.Fatal, "",
                                      identifier.Range, Message.RuntimeErrorVariableNotDefined);
      }
    }

    protected override void Visit(NoneConstantExpression noneConstant) {
      _chunk.Emit(Opcode.LOADNONE, _registerForSubExpr, 1, 0, noneConstant.Range);
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      _chunk.Emit(Opcode.LOADBOOL, _registerForSubExpr, booleanConstant.Value ? 1u : 0, 0,
                  booleanConstant.Range);
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      uint id = _constantCache.IdOfConstant(numberConstant.Value);
      _chunk.Emit(Opcode.LOADK, _registerForSubExpr, id, numberConstant.Range);
    }

    protected override void Visit(StringConstantExpression stringConstant) {
      uint id = _constantCache.IdOfConstant(stringConstant.Value);
      _chunk.Emit(Opcode.LOADK, _registerForSubExpr, id, stringConstant.Range);
    }

    protected override void Visit(ListExpression list) {
      CreateTupleOrList(Opcode.NEWLIST, list.Exprs, list.Range);
    }

    protected override void Visit(TupleExpression tuple) {
      CreateTupleOrList(Opcode.NEWTUPLE, tuple.Exprs, tuple.Range);
    }

    protected override void Visit(SubscriptExpression subscript) {
      _variableResolver.BeginExpressionScope();
      uint targetId = _registerForSubExpr;
      uint listId = VisitExpressionForRegisterId(subscript.Expr);
      uint indexId = VisitExpressionForRKId(subscript.Index);
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
          uint funcRegister = needRegister ? _variableResolver.AllocateRegister() : resultRegister;
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
            _registerForSubExpr = _variableResolver.AllocateRegister();
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
      if (assignment.Exprs.Length == 1) {
        Unpack(assignment.Targets, assignment.Exprs[0], assignment.Range);
      } else {
        Pack(assignment.Targets, assignment.Exprs, assignment.Range);
      }
    }

    protected override void Visit(BlockStatement block) {
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void Visit(ExpressionStatement expr) {
      _variableResolver.BeginExpressionScope();
      _registerForSubExpr = _variableResolver.AllocateRegister();
      switch (_runMode) {
        case RunMode.Interactive:
          Expression eval = Expression.Identifier(NativeFunctions.PrintVal, expr.Range);
          Visit(Expression.Call(eval, new Expression[] { expr.Expr }, expr.Range));
          break;
        case RunMode.Script:
          Visit(expr.Expr);
          break;
      }
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(ForInStatement forIn) {
      VariableResolver.VariableInfo loopVar = DefineVariableIfNeeded(forIn.Id.Name);

      _variableResolver.BeginBlockScope();
      if (!(GetRegisterId(forIn.Expr) is uint sequence)) {
        sequence = _variableResolver.AllocateRegister();
        _registerForSubExpr = sequence;
        Visit(forIn.Expr);
      }
      uint index = _variableResolver.AllocateRegister();
      _chunk.Emit(Opcode.LOADK, index, _constantCache.IdOfConstant(0), forIn.Range);
      uint limit = _variableResolver.AllocateRegister();
      _chunk.Emit(Opcode.LEN, limit, sequence, 0, forIn.Range);
      uint step = _variableResolver.AllocateRegister();
      _chunk.Emit(Opcode.LOADK, step, _constantCache.IdOfConstant(1), forIn.Range);
      _chunk.Emit(Opcode.FORPREP, index, 0, forIn.Range);
      int bodyStart = _chunk.Bytecode.Count;
      switch (loopVar.Type) {
        case VariableResolver.VariableType.Global:
          _variableResolver.BeginExpressionScope();
          uint targetId = _variableResolver.AllocateRegister();
          _chunk.Emit(Opcode.GETELEM, targetId, sequence, index, forIn.Range);
          _chunk.Emit(Opcode.SETGLOB, targetId, loopVar.Id, forIn.Range);
          _variableResolver.EndExpressionScope();
          break;
        case VariableResolver.VariableType.Local:
          _chunk.Emit(Opcode.GETELEM, loopVar.Id, sequence, index, forIn.Range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
      Visit(forIn.Body);
      _chunk.PatchSBXAt(bodyStart - 1, _chunk.Bytecode.Count - bodyStart);
      _chunk.Emit(Opcode.FORLOOP, index, bodyStart - (_chunk.Bytecode.Count + 1), forIn.Range);
      _variableResolver.EndBlockScope();
    }

    protected override void Visit(FuncDefStatement funcDef) {
      VariableResolver.VariableInfo info = DefineVariableIfNeeded(funcDef.Name);
      PushFunc(funcDef.Name);
      foreach (string parameterName in funcDef.Parameters) {
        _variableResolver.DefineVariable(parameterName);
      }
      Visit(funcDef.Body);
      // Emits a default return opcode.
      _chunk.Emit(Opcode.RETURN, 0, 0, 0, null);
      Function func = PopFunc();
      uint funcId = _constantCache.IdOfConstant(func);
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          _variableResolver.BeginExpressionScope();
          uint registerId = _variableResolver.AllocateRegister();
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
      _nestedJumpStack.PushFrame();
      VisitTest(@if.Test);
      PatchJumps(_nestedJumpStack.TrueJumps);
      Visit(@if.ThenBody);
      if (!(@if.ElseBody is null)) {
        _chunk.Emit(Opcode.JMP, 0, 0, @if.Range);
        int jumpEnd = GetCurrentCodePos();
        PatchJumps(_nestedJumpStack.FalseJumps);
        Visit(@if.ElseBody);
        PatchJump(jumpEnd);
      } else {
        PatchJumps(_nestedJumpStack.FalseJumps);
      }
      _nestedJumpStack.PopFrame();
    }

    protected override void Visit(PassStatement pass) {
    }

    protected override void Visit(ReturnStatement @return) {
      if (@return.Exprs.Length == 0) {
        _chunk.Emit(Opcode.RETURN, 0, 0, 0, @return.Range);
      } else if (@return.Exprs.Length == 1) {
        if (!(GetRegisterId(@return.Exprs[0]) is uint result)) {
          _variableResolver.BeginExpressionScope();
          result = _variableResolver.AllocateRegister();
          _registerForSubExpr = result;
          Visit(@return.Exprs[0]);
          _variableResolver.EndExpressionScope();
        }
        _chunk.Emit(Opcode.RETURN, result, 1, 0, @return.Range);
      } else {
        _variableResolver.BeginExpressionScope();
        uint listRegister = _variableResolver.AllocateRegister();
        _registerForSubExpr = listRegister;
        Visit(Expression.Tuple(@return.Exprs, @return.Range));
        _chunk.Emit(Opcode.RETURN, listRegister, 1, 0, @return.Range);
        _variableResolver.EndExpressionScope();
      }
    }

    protected override void Visit(WhileStatement @while) {
      _nestedJumpStack.PushFrame();
      int start = _chunk.Bytecode.Count;
      VisitTest(@while.Test);
      Visit(@while.Body);
      _chunk.Emit(Opcode.JMP, 0, start - (_chunk.Bytecode.Count + 1), @while.Range);
      PatchJumps(_nestedJumpStack.FalseJumps);
      _nestedJumpStack.PopFrame();
    }

    private void VisitTest(Expression test) {
      if (test is ComparisonExpression || test is BooleanExpression) {
        Visit(test);
      } else {
        if (GetRegisterId(test) is uint registerId) {
          _chunk.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
        } else {
          _variableResolver.BeginExpressionScope();
          registerId = _variableResolver.AllocateRegister();
          _registerForSubExpr = registerId;
          Visit(test);
          _chunk.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
          _variableResolver.EndExpressionScope();
        }
        _chunk.Emit(Opcode.JMP, 0, 0, test.Range);
        int jump = GetCurrentCodePos();
        _nestedJumpStack.FalseJumps.Add(jump);
      }
    }

    private void VisitSingleComparison(Expression left, ComparisonOperator op, Expression right,
                                       Range range) {
      uint leftRegister = VisitExpressionForRKId(left);
      uint rightRegister = VisitExpressionForRKId(right);
      (Opcode opcode, bool checkFlag) = OpcodeAndCheckFlagOfComparisonOperator(op);
      if (_nextBooleanOp == BooleanOperator.Or) {
        checkFlag = !checkFlag;
      }
      _chunk.Emit(opcode, checkFlag ? 1u : 0u, leftRegister, rightRegister, range);
      _chunk.Emit(Opcode.JMP, 0, 0, range);
      int jump = GetCurrentCodePos();
      switch (_nextBooleanOp) {
        case BooleanOperator.And:
          _nestedJumpStack.FalseJumps.Add(jump);
          break;
        case BooleanOperator.Or:
          _nestedJumpStack.TrueJumps.Add(jump);
          break;
      }
    }

    private void Pack(Expression[] targets, Expression[] exprs, Range range) {
      if (targets.Length == 1) {
        Assign(targets[0], Expression.Tuple(exprs, range), range);
      } else if (targets.Length == exprs.Length) {
        AssignMultipleTargets(targets, exprs, range);
      } else {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", range,
                                      Message.RuntimeErrorIncorrectUnpackCount);
      }
    }

    private void Unpack(Expression[] targets, Expression expr, Range range) {
      if (targets.Length == 1) {
        Assign(targets[0], expr, range);
      } else {
        // If the length of targets is less than the one of the unpacked value, SeedPython will
        // unpack part of the value. And if the length of the targets is greater than the one of the
        // unpacked value, an index out of range exception will be thrown.
        // The behavior is different from the original Python. Python will throw an incorrect unpack
        // count exception for both situations.
        // TODO: Add a build-in function to check the length of the unpacked values.
        foreach (Expression target in targets) {
          if (target is IdentifierExpression id) {
            DefineVariableIfNeeded(id.Name);
          }
        }
        _variableResolver.BeginExpressionScope();
        uint listId = VisitExpressionForRegisterId(expr);
        uint valueId = _variableResolver.AllocateRegister();
        for (int i = 0; i < targets.Length; i++) {
          _variableResolver.BeginExpressionScope();
          uint constId = _constantCache.IdOfConstant(i);
          uint indexId = _variableResolver.AllocateRegister();
          _chunk.Emit(Opcode.LOADK, indexId, constId, range);
          _chunk.Emit(Opcode.GETELEM, valueId, listId, indexId, range);
          Assign(targets[i], valueId, range);
          _variableResolver.EndExpressionScope();
        }
        _variableResolver.EndExpressionScope();
      }
    }

    private void Assign(Expression target, Expression expr, Range range) {
      if (target is IdentifierExpression id) {
        DefineVariableIfNeeded(id.Name);
      }
      _variableResolver.BeginExpressionScope();
      uint valueId = VisitExpressionForRKId(expr);
      Assign(target, valueId, range);
      _variableResolver.EndExpressionScope();
    }

    private void AssignMultipleTargets(Expression[] targets, Expression[] exprs, Range range) {
      for (int i = 0; i < targets.Length; i++) {
        if (targets[i] is IdentifierExpression id) {
          DefineVariableIfNeeded(id.Name);
        }
      }
      _variableResolver.BeginExpressionScope();
      var exprIds = new uint[targets.Length];
      for (int i = 0; i < targets.Length; i++) {
        exprIds[i] = VisitExpressionForRKId(exprs[i]);
      }
      for (int i = 0; i < targets.Length; i++) {
        Assign(targets[i], exprIds[i], range);
      }
      _variableResolver.EndExpressionScope();
    }

    private void Assign(Expression expr, uint valueId, Range range) {
      switch (expr) {
        case IdentifierExpression id:
          Assign(id, valueId, range);
          break;
        case SubscriptExpression subscript:
          uint listId = VisitExpressionForRegisterId(subscript.Expr);
          uint indexId = VisitExpressionForRKId(subscript.Index);
          _chunk.Emit(Opcode.SETELEM, listId, indexId, valueId, range);
          break;
      }
    }

    private void Assign(IdentifierExpression id, uint valueId, Range range) {
      VariableResolver.VariableInfo info = _variableResolver.FindVariable(id.Name).Value;
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          uint tempRegister = valueId;
          if (Chunk.IsConstId(valueId)) {
            tempRegister = _variableResolver.AllocateRegister();
            _chunk.Emit(Opcode.LOADK, tempRegister, valueId, range);
          }
          _chunk.Emit(Opcode.SETGLOB, tempRegister, info.Id, range);
          break;
        case VariableResolver.VariableType.Local:
          if (Chunk.IsConstId(valueId)) {
            _chunk.Emit(Opcode.LOADK, info.Id, valueId, range);
          } else {
            _chunk.Emit(Opcode.MOVE, info.Id, valueId, 0, range);
          }
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    private void CreateTupleOrList(Opcode opcode, IReadOnlyList<Expression> exprs, Range range) {
      _variableResolver.BeginExpressionScope();
      uint target = _registerForSubExpr;
      uint? first = null;
      foreach (var expr in exprs) {
        _registerForSubExpr = _variableResolver.AllocateRegister();
        if (!first.HasValue) {
          first = _registerForSubExpr;
        }
        Visit(expr);
      }
      _chunk.Emit(opcode, target, first ?? 0, (uint)exprs.Count, range);
      _variableResolver.EndExpressionScope();
    }

    private VariableResolver.VariableInfo DefineVariableIfNeeded(string name) {
      if (_variableResolver.FindVariable(name) is VariableResolver.VariableInfo info) {
        return info;
      }
      return _variableResolver.DefineVariable(name);
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
      _chunk.PatchSBXAt(jump, _chunk.Bytecode.Count - jump - 1);
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

    private uint VisitExpressionForRegisterId(Expression expr) {
      if (!(GetRegisterId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateRegister();
        _registerForSubExpr = exprId;
        Visit(expr);
      }
      return exprId;
    }

    private uint VisitExpressionForRKId(Expression expr) {
      if (!(GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateRegister();
        _registerForSubExpr = exprId;
        Visit(expr);
      }
      return exprId;
    }

    private uint? GetRegisterOrConstantId(Expression expr) {
      if (GetRegisterId(expr) is uint registerId) {
        return registerId;
      } else if (GetConstantId(expr) is uint constantId) {
        return constantId;
      }
      return null;
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
        case BinaryOperator.FloorDivide:
          return Opcode.FLOORDIV;
        case BinaryOperator.Power:
          return Opcode.POW;
        case BinaryOperator.Modulo:
          return Opcode.MOD;
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
