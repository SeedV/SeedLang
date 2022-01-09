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

using System.Diagnostics;
using SeedLang.Ast;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class Compiler : AstWalker {
    private FunctionStack _functionStack;
    // The register allocated for the result of sub-expressions.
    private VariableResolver _variableResolver;
    private uint _registerForSubExpr;
    private Chunk _chunk;
    private ConstantCache _constantCache;

    internal Compiler() {
    }

    internal Function Compile(AstNode node, Environment env) {
      _functionStack = new FunctionStack();
      _variableResolver = new VariableResolver(env);
      // main function in global scope.
      _functionStack.PushFunc("main");
      CacheTopFunction();
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0u, null);
      return _functionStack.PopFunc();
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
      throw new System.NotImplementedException();
    }

    protected override void Visit(ComparisonExpression comparison) {
      // TODO: support comparison expressions with multiple operands (e.g. a < b < c). Current
      // implementation only supports two operands (e.g. a < b).
      _variableResolver.BeginExpressionScope();
      uint first = VisitExpression(comparison.First);
      uint second = VisitExpression(comparison.Exprs[0]);
      Opcode op = Opcode.EQ;
      bool expectedResult = false;
      switch (comparison.Ops[0]) {
        case ComparisonOperator.Less:
          op = Opcode.LT;
          expectedResult = true;
          break;
        case ComparisonOperator.Greater:
          op = Opcode.LE;
          expectedResult = false;
          break;
        case ComparisonOperator.LessEqual:
          op = Opcode.LE;
          expectedResult = true;
          break;
        case ComparisonOperator.GreaterEqual:
          op = Opcode.LT;
          expectedResult = false;
          break;
        case ComparisonOperator.EqEqual:
          op = Opcode.EQ;
          expectedResult = true;
          break;
        case ComparisonOperator.NotEqual:
          op = Opcode.EQ;
          expectedResult = false;
          break;
      }
      _functionStack.CurrentChunk().Emit(op, expectedResult ? 1u : 0u, first, second,
                                         comparison.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(IdentifierExpression identifier) {
      if (_variableResolver.FindVariable(identifier.Name) is uint id) {
        if (_variableResolver.IsInGlobalScope) {
          _chunk.Emit(Opcode.GETGLOB, _registerForSubExpr, id, identifier.Range);
        } else {
          _chunk.Emit(Opcode.MOVE, _registerForSubExpr, id, 0, identifier.Range);
        }
      } else {
        // TODO: throw variable is not defined runtime error.
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
      throw new System.NotImplementedException();
    }

    protected override void Visit(SubscriptExpression subscript) {
      throw new System.NotImplementedException();
    }

    protected override void Visit(CallExpression call) {
      _variableResolver.BeginExpressionScope();
      // TODO:
      if (call.Func is IdentifierExpression identifier) {
        if (_variableResolver.FindVariable(identifier.Name) is uint funcId) {
          uint resultRegister = _registerForSubExpr;
          bool needRegister = resultRegister != _variableResolver.LastRegister;
          uint funcRegister = needRegister ? _variableResolver.AllocateVariable() : resultRegister;
          _chunk.Emit(Opcode.GETGLOB, funcRegister, funcId, identifier.Range);
          foreach (Expression expr in call.Arguments) {
            _registerForSubExpr = _variableResolver.AllocateVariable();
            Visit(expr);
          }
          _chunk.Emit(Opcode.CALL, funcRegister, (uint)call.Arguments.Length, 0, call.Range);
          if (needRegister) {
            _chunk.Emit(Opcode.MOVE, resultRegister, funcRegister, 0, call.Range);
          }
        } else {
          // TODO:
        }
      }
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(AssignmentStatement assignment) {
      switch (assignment.Target) {
        case IdentifierExpression identifier:
          if (_variableResolver.FindVariable(identifier.Name) is null) {
            _variableResolver.DefineVariable(identifier.Name);
          }
          uint variableId = _variableResolver.FindVariable(identifier.Name).Value;
          if (_variableResolver.IsInGlobalScope) {
            _variableResolver.BeginExpressionScope();
            uint resultRegister = _variableResolver.AllocateVariable();
            _registerForSubExpr = resultRegister;
            Visit(assignment.Expr);
            _chunk.Emit(Opcode.SETGLOB, resultRegister, variableId, assignment.Range);
            _variableResolver.EndExpressionScope();
          } else {
            if (GetRegisterId(assignment.Expr) is uint registerId) {
              _chunk.Emit(Opcode.MOVE, variableId, registerId, 0, assignment.Range);
            } else if (GetConstantId(assignment.Expr) is uint constantId) {
              _chunk.Emit(Opcode.LOADK, variableId, constantId, 0, assignment.Range);
            } else {
              _registerForSubExpr = variableId;
              Visit(assignment.Expr);
            }
          }
          break;
        case SubscriptExpression _:
          // TODO: handle subscript assignment.
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

    protected override void Visit(FuncDeclStatement funcDecl) {
      PushFunc(funcDecl.Name);
      foreach (string parameterName in funcDecl.Parameters) {
        _variableResolver.DefineVariable(parameterName);
      }
      Visit(funcDecl.Body);
      Function func = PopFunc();
      uint funcId = _constantCache.IdOfConstant(func);
      uint variableId = _variableResolver.DefineVariable(funcDecl.Name);
      _variableResolver.BeginExpressionScope();
      uint registerId = _variableResolver.AllocateVariable();
      _chunk.Emit(Opcode.LOADK, registerId, funcId, funcDecl.Range);
      _chunk.Emit(Opcode.SETGLOB, registerId, variableId, funcDecl.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void Visit(IfStatement @if) {
      throw new System.NotImplementedException();
    }

    protected override void Visit(ReturnStatement @return) {
      throw new System.NotImplementedException();
    }

    protected override void Visit(WhileStatement @while) {
      int start = _chunk.Bytecode.Count;
      Visit(@while.Test);
      int jump = _chunk.Bytecode.Count;
      _chunk.Emit(Opcode.JMP, 0, @while.Range);
      Visit(@while.Body);
      _chunk.Emit(Opcode.JMP, start - (_chunk.Bytecode.Count + 1), @while.Range);
      _chunk.PatchJumpAt(jump, _chunk.Bytecode.Count - jump - 1);
    }

    private void PushFunc(string name) {
      _functionStack.PushFunc(name);
      CacheTopFunction();
      _variableResolver.BeginFunctionScope();
    }

    private Function PopFunc() {
      _variableResolver.EndFunctionScope();
      Function func = _functionStack.PopFunc();
      CacheTopFunction();
      return func;
    }

    private void CacheTopFunction() {
      _chunk = _functionStack.CurrentChunk();
      _constantCache = _functionStack.CurrentConstantCache();
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
      if (expr is IdentifierExpression identifier && !_variableResolver.IsInGlobalScope) {
        return _variableResolver.FindVariable(identifier.Name);
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
  }
}
