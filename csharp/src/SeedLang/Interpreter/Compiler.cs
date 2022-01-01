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

using System;
using SeedLang.Ast;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class Compiler : AstWalker {
    private Chunk _chunk;
    private ConstantCache _constantCache;
    private RegisterAllocator _registerAllocator;
    // The register allocated for the result of sub-expressions.
    private uint _registerForSubExpr;

    internal Chunk Compile(AstNode node) {
      _chunk = new Chunk();
      _constantCache = new ConstantCache();
      _registerAllocator = new RegisterAllocator();
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0u, null);
      _chunk.RegisterCount = _registerAllocator.MaxRegisterCount;
      _chunk.SetConstants(_constantCache.Constants.ToArray());
      return _chunk;
    }

    protected override void Visit(BinaryExpression binary) {
      uint register = _registerForSubExpr;
      bool needLeftRegister = !TryGetRegisterOrConstantId(binary.Left, out uint leftId);
      uint left = needLeftRegister ? _registerAllocator.AllocateTempVariable() : leftId;
      if (needLeftRegister) {
        _registerForSubExpr = left;
        Visit(binary.Left);
      }
      bool needRightRegister = !TryGetRegisterOrConstantId(binary.Right, out uint rightId);
      uint right = needRightRegister ? _registerAllocator.AllocateTempVariable() : rightId;
      if (needRightRegister) {
        _registerForSubExpr = right;
        Visit(binary.Right);
      }
      _chunk.Emit(OpcodeOfBinaryOperator(binary.Op), register, left, right, binary.Range);
      if (needLeftRegister) {
        _registerAllocator.DeallocateVariable();
      }
      if (needRightRegister) {
        _registerAllocator.DeallocateVariable();
      }
    }

    protected override void Visit(UnaryExpression unary) {
      uint register = _registerForSubExpr;
      bool needRegister = !TryGetRegisterOrConstantId(unary.Expr, out uint exprId);
      uint expr = needRegister ? _registerAllocator.AllocateTempVariable() : exprId;
      if (needRegister) {
        _registerForSubExpr = expr;
        Visit(unary.Expr);
      }
      _chunk.Emit(Opcode.UNM, register, expr, 0, unary.Range);
      if (needRegister) {
        _registerAllocator.DeallocateVariable();
      }
    }

    protected override void Visit(BooleanExpression boolean) {
      throw new NotImplementedException();
    }

    protected override void Visit(ComparisonExpression comparison) {
      // TODO: support multiple operators. Current implementation only supports two operators.
      bool needFirstRegister = !TryGetRegisterOrConstantId(comparison.First, out uint firstId);
      uint first = needFirstRegister ? _registerAllocator.AllocateTempVariable() : firstId;
      if (needFirstRegister) {
        _registerForSubExpr = first;
        Visit(comparison.First);
      }
      bool needSecondRegister = !TryGetRegisterOrConstantId(comparison.Exprs[0], out uint secondId);
      uint second = needSecondRegister ? _registerAllocator.AllocateTempVariable() : secondId;
      if (needSecondRegister) {
        _registerForSubExpr = second;
        Visit(comparison.Exprs[0]);
      }
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
      _chunk.Emit(op, expectedResult ? 1u : 0u, first, second, comparison.Range);
      if (needFirstRegister) {
        _registerAllocator.DeallocateVariable();
      }
      if (needSecondRegister) {
        _registerAllocator.DeallocateVariable();
      }
    }

    protected override void Visit(IdentifierExpression identifier) {
      if (_registerAllocator.IsInGlobalScope) {
        uint variableNameId = _constantCache.IdOfConstant(identifier.Name);
        _chunk.Emit(Opcode.GETGLOB, _registerForSubExpr, variableNameId, identifier.Range);
      } else {
        uint register = _registerAllocator.RegisterOfVariable(identifier.Name);
        _chunk.Emit(Opcode.MOVE, _registerForSubExpr, register, 0, identifier.Range);
      }
    }

    protected override void Visit(NoneConstantExpression noneConstant) {
      throw new NotImplementedException();
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      throw new NotImplementedException();
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      uint id = _constantCache.IdOfConstant(numberConstant.Value);
      _chunk.Emit(Opcode.LOADK, _registerForSubExpr, id, numberConstant.Range);
    }

    protected override void Visit(StringConstantExpression stringConstant) {
      throw new NotImplementedException();
    }

    protected override void Visit(ListExpression list) {
      throw new NotImplementedException();
    }

    protected override void Visit(SubscriptExpression subscript) {
      throw new NotImplementedException();
    }

    protected override void Visit(CallExpression call) {
      throw new NotImplementedException();
    }

    protected override void Visit(AssignmentStatement assignment) {
      switch (assignment.Target) {
        case IdentifierExpression identifier:
          if (_registerAllocator.IsInGlobalScope) {
            uint variableNameId = _constantCache.IdOfConstant(identifier.Name);
            if (TryGetRegisterId(assignment.Expr, out uint exprId)) {
              _chunk.Emit(Opcode.SETGLOB, exprId, variableNameId, assignment.Range);
            } else {
              uint resultRegister = _registerAllocator.AllocateTempVariable();
              _registerForSubExpr = resultRegister;
              Visit(assignment.Expr);
              _chunk.Emit(Opcode.SETGLOB, resultRegister, variableNameId, assignment.Range);
              _registerAllocator.DeallocateVariable();
            }
          } else {
            _registerForSubExpr = _registerAllocator.RegisterOfVariable(identifier.Name);
            if (TryGetRegisterOrConstantId(assignment.Expr, out uint exprId)) {
              _chunk.Emit(Opcode.MOVE, _registerForSubExpr, exprId, 0, assignment.Range);
            } else {
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
      if (!TryGetRegisterId(expr.Expr, out uint register)) {
        register = _registerAllocator.AllocateTempVariable();
        _registerForSubExpr = register;
        Visit(expr.Expr);
        _registerAllocator.DeallocateVariable();
      }
      _chunk.Emit(Opcode.EVAL, register, expr.Range);
    }

    protected override void Visit(FunctionStatement func) {
      throw new NotImplementedException();
    }

    protected override void Visit(IfStatement @if) {
      throw new NotImplementedException();
    }

    protected override void Visit(ReturnStatement @return) {
      throw new NotImplementedException();
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

    private bool TryGetRegisterId(Expression expr, out uint id) {
      if (expr is IdentifierExpression identifier && !_registerAllocator.IsInGlobalScope) {
        id = _registerAllocator.RegisterOfVariable(identifier.Name);
        return true;
      }
      id = 0;
      return false;
    }

    private bool TryGetRegisterOrConstantId(Expression expr, out uint id) {
      switch (expr) {
        case NumberConstantExpression number:
          id = _constantCache.IdOfConstant(number.Value);
          return true;
        case StringConstantExpression str:
          id = _constantCache.IdOfConstant(str.Value);
          return true;
        case IdentifierExpression identifier:
          if (!_registerAllocator.IsInGlobalScope) {
            id = _registerAllocator.RegisterOfVariable(identifier.Name);
            return true;
          }
          break;
      }
      id = 0;
      return false;
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
          throw new NotImplementedException($"Operator {op} not implemented.");
      }
    }
  }
}
