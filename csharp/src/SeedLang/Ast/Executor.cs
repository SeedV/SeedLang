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
        new GlobalEnvironment<IValue>(new NoneValue());

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
            throw new NotImplementedException($"Unsupported binary operator: {binary.Op}");
        }
      } catch (DiagnosticException ex) {
        // Throws a new diagnostic exception with more information.
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, binary.Range,
                                      ex.Diagnostic.MessageId);
      }
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(left, binary.Op, right, _expressionResult, binary.Range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    protected override void Visit(BooleanExpression boolean) {
      var values = new IValue[boolean.Exprs.Length];
      bool shortCircuitCondition = boolean.Op == BooleanOperator.Or;
      for (int i = 0; i < boolean.Exprs.Length; ++i) {
        Visit(boolean.Exprs[i]);
        values[i] = _expressionResult;
        if (_expressionResult.Boolean == shortCircuitCondition) {
          break;
        }
      }
      if (!_visualizerCenter.BooleanPublisher.IsEmpty()) {
        var be = new BooleanEvent(boolean.Op, values, _expressionResult, boolean.Range);
        _visualizerCenter.BooleanPublisher.Notify(be);
      }
    }

    protected override void Visit(ComparisonExpression comparison) {
      Visit(comparison.First);
      IValue first = _expressionResult;
      var values = new IValue[comparison.Exprs.Length];
      bool currentResult = true;
      for (int i = 0; i < comparison.Ops.Length; ++i) {
        Visit(comparison.Exprs[i]);
        values[i] = _expressionResult;
        IValue left = i > 0 ? values[i - 1] : first;
        switch (comparison.Ops[i]) {
          case ComparisonOperator.Less:
            currentResult = ValueHelper.Less(left, values[i]);
            break;
          case ComparisonOperator.Greater:
            currentResult = ValueHelper.Great(left, values[i]);
            break;
          case ComparisonOperator.LessEqual:
            currentResult = ValueHelper.LessEqual(left, values[i]);
            break;
          case ComparisonOperator.GreaterEqual:
            currentResult = ValueHelper.GreatEqual(left, values[i]);
            break;
          case ComparisonOperator.EqEqual:
            currentResult = left.Equals(values[i]);
            break;
          case ComparisonOperator.NotEqual:
            currentResult = !left.Equals(values[i]);
            break;
          default:
            throw new NotImplementedException(
                $"Unsupported comparison operator: {comparison.Ops[i]}");
        }
        if (!currentResult) {
          break;
        }
      }
      _expressionResult = new BooleanValue(currentResult);
      if (!_visualizerCenter.ComparisonPublisher.IsEmpty()) {
        var ce = new ComparisonEvent(first, comparison.Ops, values, _expressionResult,
                                     comparison.Range);
        _visualizerCenter.ComparisonPublisher.Notify(ce);
      }
    }

    protected override void Visit(UnaryExpression unary) {
      Visit(unary.Expr);
      IValue result = _expressionResult;
      if (unary.Op == UnaryOperator.Negative) {
        _expressionResult = new NumberValue(result.Number * -1);
      } else if (unary.Op == UnaryOperator.Not) {
        _expressionResult = new BooleanValue(!result.Boolean);
      }
      if (!_visualizerCenter.UnaryPublisher.IsEmpty()) {
        var ue = new UnaryEvent(unary.Op, result, _expressionResult, unary.Range);
        _visualizerCenter.UnaryPublisher.Notify(ue);
      }
    }

    protected override void Visit(IdentifierExpression identifier) {
      _expressionResult = _globals.GetVariable(identifier.Name);
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      _expressionResult = new BooleanValue(booleanConstant.Value);
    }

    protected override void Visit(NoneConstantExpression noneConstant) {
      _expressionResult = new NoneValue();
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      try {
        _expressionResult = new NumberValue(numberConstant.Value);
      } catch (DiagnosticException ex) {
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, numberConstant.Range,
                                      ex.Diagnostic.MessageId);
      }
    }

    protected override void Visit(StringConstantExpression stringConstant) {
      _expressionResult = new StringValue(stringConstant.Value);
    }

    protected override void Visit(AssignmentStatement assignment) {
      Visit(assignment.Expr);
      _globals.SetVariable(assignment.Identifier.Name, _expressionResult);
      var ae = new AssignmentEvent(assignment.Identifier.Name, _expressionResult, assignment.Range);
      _visualizerCenter.AssignmentPublisher.Notify(ae);
    }

    protected override void Visit(BlockStatement block) {
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void Visit(ExpressionStatement expr) {
      Visit(expr.Expr);
      if (!_visualizerCenter.EvalPublisher.IsEmpty()) {
        var ee = new EvalEvent(_expressionResult, expr.Range);
        _visualizerCenter.EvalPublisher.Notify(ee);
      }
    }

    protected override void Visit(IfStatement @if) {
      Visit(@if.Test);
      if (_expressionResult.Boolean) {
        Visit(@if.ThenBody);
      } else {
        Visit(@if.ElseBody);
      }
    }

    protected override void Visit(WhileStatement @while) {
      while (true) {
        Visit(@while.Test);
        if (_expressionResult.Boolean) {
          Visit(@while.Body);
        } else {
          break;
        }
      }
    }
  }
}
