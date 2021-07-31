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
  //
  // The executor traverses through the AST tree by implementing the IVisitor interface.
  public sealed class Executor : IVisitor {
    // The visualizer object to visualize the execution result.
    private readonly IVisualizer _visualizer;
    // The result of current executed expression.
    private BaseValue _expressionResult;

    public Executor() {
      _visualizer = new NullVisualizer();
    }

    public Executor(IVisualizer visualizer) {
      _visualizer = visualizer;
    }

    // Executes the given AST tree.
    public void Run(AstNode node) {
      Visit(node);
    }

    public void VisitBinaryExpression(BinaryExpression binary) {
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
      _visualizer.OnBinaryExpression(left, right, _expressionResult);
    }

    public void VisitNumberConstant(NumberConstantExpression number) {
      _expressionResult = new NumberValue(number.Value);
    }

    public void VisitStringConstant(StringConstantExpression str) {
      _expressionResult = new StringValue(str.Value);
    }

    public void VisitEvalStatement(EvalStatement eval) {
      Visit(eval.Expr);
      _visualizer.OnEvalStatement(_expressionResult);
    }

    private void Visit(AstNode node) {
      node.Accept(this);
    }
  }
}
