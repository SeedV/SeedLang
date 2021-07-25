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
using System.Collections.Generic;

namespace SeedLang.Ast {
  // A executor class to execute an program represented by an AST tree.
  //
  // The executor traverses through the AST tree by implementing the interfaces of AstVisitor. The
  // visit method of an expression will return the result of the expression. The return value of a
  // statement is not used.
  public sealed class Executor : AstVisitor<BaseValue> {
    // The global environment of the executor.
    private readonly Dictionary<string, BaseValue> _globals = new Dictionary<string, BaseValue>();

    public Executor() {
    }

    // Executes the given AST tree.
    public BaseValue Run(AstNode node) {
      return Visit(node);
    }

    // Registers a native function with a name in the global environment.
    public void RegisterNativeFunc(string name, Func<BaseValue, BaseValue> func) {
      // TODO: handle the ArgumentException when the name already exists.
      _globals.Add(name, new NativeFuncValue(func));
    }

    protected internal override BaseValue VisitBinaryExpression(BinaryExpression binary) {
      BaseValue left = Visit(binary.Left);
      BaseValue right = Visit(binary.Right);
      // TODO: handle other operators.
      switch (binary.Op) {
        case BinaryOperator.Add:
          return left + right;
        case BinaryOperator.Subtract:
          return left - right;
        case BinaryOperator.Multiply:
          return left * right;
        case BinaryOperator.Divide:
          return left / right;
        default:
          throw new ArgumentException("Unsupported binary operator.");
      }
    }

    protected internal override BaseValue VisitNumberConstant(NumberConstantExpression number) {
      return new NumberValue(number.Value);
    }

    protected internal override BaseValue VisitStringConstant(StringConstantExpression str) {
      return new StringValue(str.Value);
    }

    protected internal override BaseValue VisitEvalStatement(EvalStatement eval) {
      if (_globals["print"] is NativeFuncValue func) {
        func.Call(Visit(eval.Expr));
      } else {
        // TODO: throw an exception of the global native function doesn't exist. Might need a comman
        // exception class in SeedLang.Ast?
      }
      return null;
    }
  }
}
