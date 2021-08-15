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
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // An executor class to execute a program represented by an AST tree.
  public sealed class Executor : AstWalker {
    // The visualizer object to visualize the execution result.
    private readonly VisualizerCenter _visualizerCenter;
    // The result of current executed expression.
    private BaseValue _expressionResult;

    public Executor(VisualizerCenter visualizerCenter) {
      _visualizerCenter = visualizerCenter;
    }

    // Executes the given AST tree.
    public void Run(AstNode node) {
      Visit(node);
    }

    protected override void Visit(BinaryExpression binary) {
      Visit(binary.Left);
      BaseValue left = _expressionResult;
      Visit(binary.Right);
      BaseValue right = _expressionResult;
      // TODO: handle other operators.
      switch (binary.Op) {
        case BinaryOperator.Add:
          _expressionResult = left + right;
          break;
        case BinaryOperator.Subtract:
          _expressionResult = left - right;
          break;
        case BinaryOperator.Multiply:
          _expressionResult = left * right;
          break;
        case BinaryOperator.Divide:
          _expressionResult = left / right;
          break;
        default:
          throw new ArgumentException("Unsupported binary operator.");
      }
      _visualizerCenter.BinaryEvent.Notify(new BinaryEvent(left, right, _expressionResult));
    }

    protected override void Visit(NumberConstantExpression number) {
      _expressionResult = new NumberValue(number.Value);
    }

    protected override void Visit(StringConstantExpression str) {
      _expressionResult = new StringValue(str.Value);
    }

    protected override void Visit(EvalStatement eval) {
      Visit(eval.Expr);
      _visualizerCenter.EvalEvent.Notify(new EvalEvent(_expressionResult));
    }
  }
}
