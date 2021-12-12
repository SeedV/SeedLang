// Copyright 2021 The Aha001 Team.
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
    // A structure to exchange information between an expresion and its sub-expression.
    private struct ExpressionInfo {
      // A boolean flag to indicate if current expression can handle constant sub-expressions. If it
      // is false, the constant sub-expression needs to emit a LOADK instruction to load its value
      // into ResultRegister. Otherwise the constant sub-expression needs to set its id to
      // ResultConstId for current expression to use.
      public bool CanHandleConstSubExpr;
      // The register allocated for the result of the sub-expression.
      public uint ResultRegister;
      // The constant id of the sub-expression if it's a constant expression.
      public uint ResultConstId;
    }

    private Chunk _chunk;
    private ConstantCache _constantCache;
    private RegisterAllocator _registerAllocator;
    private ExpressionInfo _expressionInfo;

    internal Chunk Compile(AstNode node) {
      _chunk = new Chunk();
      _constantCache = new ConstantCache();
      _registerAllocator = new RegisterAllocator();
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0);
      _chunk.RegisterCount = _registerAllocator.MaxRegisterCount;
      _chunk.SetConstants(_constantCache.Constants.ToArray());
      return _chunk;
    }

    protected override void Visit(BinaryExpression binary) {
      uint resultRegister = _expressionInfo.ResultRegister;
      bool needLeftRegister = NeedAllocateRegister(binary.Left);
      bool needRightRegister = NeedAllocateRegister(binary.Right);
      uint left = needLeftRegister ? _registerAllocator.AllocateTempVariable() : 0;
      uint right = needRightRegister ? _registerAllocator.AllocateTempVariable() : 0;
      _expressionInfo.CanHandleConstSubExpr = true;
      _expressionInfo.ResultRegister = left;
      Visit(binary.Left);
      if (!needLeftRegister) {
        left = _expressionInfo.ResultConstId;
      }
      _expressionInfo.CanHandleConstSubExpr = true;
      _expressionInfo.ResultRegister = right;
      Visit(binary.Right);
      if (!needRightRegister) {
        right = _expressionInfo.ResultConstId;
      }
      _chunk.Emit(OpcodeOfBinaryOperator(binary.Op), resultRegister, left, right, binary.Range);
      if (needLeftRegister) {
        _registerAllocator.DeallocateVariable();
      }
      if (needRightRegister) {
        _registerAllocator.DeallocateVariable();
      }
    }

    protected override void Visit(UnaryExpression unary) {
      uint resultRegister = _expressionInfo.ResultRegister;
      bool needAllocateRegister = NeedAllocateRegister(unary.Expr);
      uint exprId = needAllocateRegister ? _registerAllocator.AllocateTempVariable() : 0;
      _expressionInfo.CanHandleConstSubExpr = true;
      _expressionInfo.ResultRegister = exprId;
      Visit(unary.Expr);
      if (!needAllocateRegister) {
        exprId = _expressionInfo.ResultConstId;
      }
      _chunk.Emit(Opcode.UNM, resultRegister, exprId, 0, unary.Range);
      if (needAllocateRegister) {
        _registerAllocator.DeallocateVariable();
      }
    }

    protected override void Visit(BooleanExpression boolean) {
      throw new NotImplementedException();
    }

    protected override void Visit(ComparisonExpression comparison) {
      throw new NotImplementedException();
    }

    protected override void Visit(IdentifierExpression identifier) {
      uint variableNameId = _constantCache.IdOfConstant(identifier.Name);
      _chunk.Emit(Opcode.GETGLOB, _expressionInfo.ResultRegister, variableNameId, identifier.Range);
    }

    protected override void Visit(NoneConstantExpression noneConstant) {
      throw new NotImplementedException();
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      throw new NotImplementedException();
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      _expressionInfo.ResultConstId = _constantCache.IdOfConstant(numberConstant.Value);
      if (!_expressionInfo.CanHandleConstSubExpr) {
        _chunk.Emit(Opcode.LOADK, _expressionInfo.ResultRegister, _expressionInfo.ResultConstId,
                    numberConstant.Range);
      }
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

    protected override void Visit(AssignmentStatement assignment) {
      uint resultRegister = _registerAllocator.AllocateTempVariable();
      _expressionInfo.CanHandleConstSubExpr = false;
      _expressionInfo.ResultRegister = resultRegister;
      Visit(assignment.Expr);
      uint variableNameId = _constantCache.IdOfConstant(assignment.Identifier.Name);
      _chunk.Emit(Opcode.SETGLOB, resultRegister, variableNameId, assignment.Range);
      _registerAllocator.DeallocateVariable();
    }

    protected override void Visit(BlockStatement block) {
      throw new NotImplementedException();
    }

    protected override void Visit(ExpressionStatement expr) {
      uint register = _registerAllocator.AllocateTempVariable();
      _expressionInfo.CanHandleConstSubExpr = false;
      _expressionInfo.ResultRegister = register;
      Visit(expr.Expr);
      _chunk.Emit(Opcode.EVAL, register, expr.Range);
      _registerAllocator.DeallocateVariable();
    }

    protected override void Visit(IfStatement @if) {
      throw new NotImplementedException();
    }

    protected override void Visit(WhileStatement @while) {
      throw new NotImplementedException();
    }

    private static bool NeedAllocateRegister(Expression expression) {
      return !(expression is NumberConstantExpression || expression is StringConstantExpression);
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
