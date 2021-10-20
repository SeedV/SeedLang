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

using System.Diagnostics;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // An executor class to execute a program represented by an AST tree.
  internal sealed class Executor : AstWalker {
    // The visualizer center to observe AST execution events and dispatch them to the registered
    // visualizers.
    private readonly VisualizerCenter _visualizerCenter;

    // The global environment to store names and values of global variables.
    private readonly GlobalEnvironment<Value> _globals = new GlobalEnvironment<Value>();

    // The result of current executed expression.
    private Value _expressionResult;

    internal Executor(VisualizerCenter visualizerCenter = null) {
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
    }

    // Executes the given AST tree.
    internal void Run(AstNode node) {
      Visit(node);
    }

    protected override void Visit(BinaryExpression binary) {
      Visit(binary.Left);
      Value left = _expressionResult;
      Visit(binary.Right);
      Value right = _expressionResult;
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
          CheckDivideByZero(right.ToNumber(), binary.Range);
          _expressionResult = left / right;
          break;
        default:
          Debug.Fail($"Unsupported binary operator: {binary.Op}");
          break;
      }
      CheckOverflow(_expressionResult.ToNumber(), binary.Range);
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(left, binary.Op, right, _expressionResult, binary.Range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    protected override void Visit(IdentifierExpression identifier) {
      if (_globals.TryGetVariable(identifier.Name, out var value)) {
        _expressionResult = value;
        CheckOverflow(_expressionResult.ToNumber(), identifier.Range);
      } else {
        // TODO: should the result be a null value or default number value if the variable is not
        // assigned before using? Another option is to report a runtime error.
        _expressionResult = new NumberValue();
      }
    }

    protected override void Visit(NumberConstantExpression number) {
      CheckOverflow(number.Value, number.Range);
      _expressionResult = new NumberValue(number.Value);
    }

    protected override void Visit(StringConstantExpression str) {
      _expressionResult = new StringValue(str.Value);
    }

    protected override void Visit(UnaryExpression unary) {
      Visit(unary.Expr);
      // TODO: handle other unary operators, and add an unary event for visualization if needed.
      _expressionResult = new NumberValue(_expressionResult.ToNumber() * -1);
    }

    protected override void Visit(AssignmentStatement assignment) {
      Visit(assignment.Expr);
      _globals.SetVariable(assignment.Identifier.Name, _expressionResult);
      var ae = new AssignmentEvent(assignment.Identifier.Name, _expressionResult, assignment.Range);
      _visualizerCenter.AssignmentPublisher.Notify(ae);
    }

    protected override void Visit(EvalStatement eval) {
      Visit(eval.Expr);
      if (!_visualizerCenter.EvalPublisher.IsEmpty()) {
        var ee = new EvalEvent(_expressionResult, eval.Range);
        _visualizerCenter.EvalPublisher.Notify(ee);
      }
    }

    private static void CheckDivideByZero(double divisor, Range range) {
      if (divisor == 0) {
        // TODO: how to get the module name?
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Error, "", range,
                                      Message.RuntimeErrorDivideByZero);
      }
    }

    private static void CheckOverflow(double value, Range range) {
      // TODO: do we need separate NaN as another runtime error?
      if (double.IsInfinity(value) || double.IsNaN(value)) {
        // TODO: how to get the module name?
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Error, "", range,
                                      Message.RuntimeOverflow);
      }
    }
  }
}
