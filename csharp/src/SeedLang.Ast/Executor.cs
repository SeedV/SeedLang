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
using System.Diagnostics;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // An executor class to execute a program represented by an AST tree.
  public sealed class Executor : AstWalker {
    // The visualizer center to observe AST execution events and dispatch them to the registered
    // visualizers.
    private readonly VisualizerCenter _visualizerCenter;

    // The dictionary to store variable names and current values of global variables.
    // TODO: define a class to handle global symbol table lookup.
    private readonly Dictionary<string, BaseValue> _globals = new Dictionary<string, BaseValue>();

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
          Debug.Fail($"Unsupported binary operator: {binary.Op}");
          break;
      }
      _visualizerCenter.BinaryPublisher.Notify(
          new BinaryEvent(left, binary.Op, right, _expressionResult));
    }

    protected override void Visit(IdentifierExpression identifier) {
      if (_globals.TryGetValue(identifier.Name, out var value)) {
        _expressionResult = value;
      } else {
        // TODO: should the result be a null value or default number value if the variable is not
        // assigned before using? Another option is to report a runtime error.
        _expressionResult = new NumberValue();
      }
    }

    protected override void Visit(NumberConstantExpression number) {
      _expressionResult = new NumberValue(number.Value);
    }

    protected override void Visit(StringConstantExpression str) {
      _expressionResult = new StringValue(str.Value);
    }

    protected override void Visit(AssignmentStatement assignment) {
      Visit(assignment.Expr);
      _globals[assignment.Identifier.Name] = _expressionResult;
      var e = new AssignmentEvent(assignment.Identifier.Name, _expressionResult);
      _visualizerCenter.AssignmentPublisher.Notify(e);
    }

    protected override void Visit(EvalStatement eval) {
      Visit(eval.Expr);
      _visualizerCenter.EvalPublisher.Notify(new EvalEvent(_expressionResult));
    }
  }
}
