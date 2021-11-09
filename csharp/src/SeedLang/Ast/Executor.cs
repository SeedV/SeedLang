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

using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // An executor class to execute a program represented by an AST tree.
  internal sealed class Executor : AstWalker {
    // The visualizer center to observe AST execution events and dispatch them to the registered
    // visualizers.
    private readonly VisualizerCenter _visualizerCenter;

    // The global environment to store names and values of global variables.
    private readonly GlobalEnvironment<IValue> _globals =
        new GlobalEnvironment<IValue>(new NullValue());

    // The result of current executed expression.
    private IValue _expressionResult;

    internal Executor(VisualizerCenter visualizerCenter = null) {
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
    }

    // Executes the given AST tree.
    internal void Run(AstNode node) {
      Visit(node);
    }

    protected override void Visit(BinaryExpression binary) {
      Visit(binary.Left);
      IValue left = _expressionResult;
      Visit(binary.Right);
      IValue right = _expressionResult;
      // TODO: handle other operators.
      try {
        switch (binary.Op) {
          case BinaryOperator.Add:
            _expressionResult = new NumberValue(ValueHelper.Add(left, right));
            break;
          case BinaryOperator.Subtract:
            _expressionResult = new NumberValue(ValueHelper.Subtract(left, right));
            break;
          case BinaryOperator.Multiply:
            _expressionResult = new NumberValue(ValueHelper.Multiply(left, right));
            break;
          case BinaryOperator.Divide:
            _expressionResult = new NumberValue(ValueHelper.Divide(left, right));
            break;
          default:
            throw new System.NotImplementedException($"Unsupported binary operator: {binary.Op}");
        }
      } catch (DiagnosticException ex) {
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, binary.Range,
                                      ex.Diagnostic.MessageId);
      }
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(left, binary.Op, right, _expressionResult, binary.Range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    protected override void Visit(IdentifierExpression identifier) {
      _expressionResult = _globals.GetVariable(identifier.Name);
    }

    protected override void Visit(NumberConstantExpression number) {
      try {
        _expressionResult = new NumberValue(number.Value);
      } catch (DiagnosticException ex) {
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, number.Range,
                                      ex.Diagnostic.MessageId);
      }
    }

    protected override void Visit(StringConstantExpression str) {
      _expressionResult = new StringValue(str.Value);
    }

    protected override void Visit(UnaryExpression unary) {
      Visit(unary.Expr);
      // TODO: handle other unary operators, and add an unary event for visualization if needed.
      _expressionResult = new NumberValue(_expressionResult.Number * -1);
    }

    protected override void Visit(AssignmentStatement assignment) {
      Visit(assignment.Expr);
      _globals.SetVariable(assignment.Identifier.Name, _expressionResult);
      var ae = new AssignmentEvent(assignment.Identifier.Name, _expressionResult, assignment.Range);
      _visualizerCenter.AssignmentPublisher.Notify(ae);
    }

    protected override void Visit(ExpressionStatement expr) {
      Visit(expr.Expr);
      if (!_visualizerCenter.EvalPublisher.IsEmpty()) {
        var ee = new EvalEvent(_expressionResult, expr.Range);
        _visualizerCenter.EvalPublisher.Notify(ee);
      }
    }
  }
}
