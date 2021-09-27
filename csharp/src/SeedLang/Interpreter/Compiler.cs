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
      // A boolean flag to indicate if the sub-expression need emit a LOADK instruction if it's a
      // constant expression. The ResultConstId will be used to access the constant if no LOADK
      // instruction is emitted.
      public bool NeedLoadConstant;
      // The register allocated for the result of the sub-expression.
      public uint ResultRegister;
      // The constant id of the sub-expression if it's a constant expression.
      public uint ResultConstId;
    }

    private Chunk _chunk;
    private ConstantCache _constantCache;
    private readonly RegisterAllocator _registerAllocator = new RegisterAllocator();
    private ExpressionInfo _expressionInfo;

    internal Chunk Compile(Statement node) {
      _chunk = new Chunk();
      _constantCache = new ConstantCache(_chunk);
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0);
      _chunk.RegisterCount = _registerAllocator.MaxRegisterCount;
      return _chunk;
    }

    protected override void Visit(BinaryExpression binary) {
      uint result = _expressionInfo.ResultRegister;
      bool needLeftRegister = NeedAllocateRegister(binary.Left);
      bool needRightRegister = NeedAllocateRegister(binary.Right);
      uint left = needLeftRegister ? _registerAllocator.AllocateTempVariable() : 0;
      uint right = needRightRegister ? _registerAllocator.AllocateTempVariable() : 0;
      _expressionInfo.NeedLoadConstant = false;
      _expressionInfo.ResultRegister = left;
      Visit(binary.Left);
      if (!needLeftRegister) {
        left = _expressionInfo.ResultConstId;
      }
      _expressionInfo.NeedLoadConstant = false;
      _expressionInfo.ResultRegister = right;
      Visit(binary.Right);
      if (!needRightRegister) {
        right = _expressionInfo.ResultConstId;
      }
      _chunk.Emit(OpcodeOfBinaryOperator(binary.Op), result, left, right);
    }

    protected override void Visit(IdentifierExpression expression) {
      throw new NotImplementedException();
    }

    protected override void Visit(NumberConstantExpression number) {
      _expressionInfo.ResultConstId = _constantCache.IdOfConstant(number.Value);
      if (_expressionInfo.NeedLoadConstant) {
        _chunk.Emit(Opcode.LOADK, _expressionInfo.ResultRegister, _expressionInfo.ResultConstId);
      }
    }

    protected override void Visit(StringConstantExpression str) {
      throw new NotImplementedException();
    }

    protected override void Visit(UnaryExpression unary) {
      throw new NotImplementedException();
    }

    protected override void Visit(AssignmentStatement assignment) {
      throw new NotImplementedException();
    }

    protected override void Visit(EvalStatement eval) {
      uint register = _registerAllocator.AllocateTempVariable();
      _expressionInfo.NeedLoadConstant = true;
      _expressionInfo.ResultRegister = register;
      Visit(eval.Expr);
      _chunk.Emit(Opcode.EVAL, register);
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
