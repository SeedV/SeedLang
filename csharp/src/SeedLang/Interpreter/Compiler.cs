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

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class Compiler : AstWalker {
    private Chunk _chunk;

    internal Chunk Compile(Statement node) {
      _chunk = new Chunk();
      Visit(node);
      _chunk.Emit(Opcode.RETURN, 0);
      return _chunk;
    }

    protected override void Visit(BinaryExpression binary) {
      throw new NotImplementedException();
    }

    protected override void Visit(IdentifierExpression expression) {
      throw new NotImplementedException();
    }

    protected override void Visit(NumberConstantExpression number) {
      // TODO: implement registers allocation, and load the constants into an allocated register.
      var index = _chunk.AddConstant(number.Value);
      _chunk.Emit(Opcode.LOADK, 0, index);
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
      // TODO: implement registers allocation, and evaluate the result register.
      Visit(eval.Expr);
      _chunk.Emit(Opcode.EVAL, 0);
    }
  }
}
