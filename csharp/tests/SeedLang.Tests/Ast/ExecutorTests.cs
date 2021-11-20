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
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExecutorTests {
    private class MockupVisualizer : IVisualizer<AssignmentEvent>,
                                     IVisualizer<BinaryEvent>,
                                     IVisualizer<ComparisonEvent>,
                                     IVisualizer<EvalEvent> {
      public AssignmentEvent AssignmentEvent { get; private set; }
      public BinaryEvent BinaryEvent { get; private set; }
      public ComparisonEvent ComparisonEvent { get; private set; }
      public EvalEvent EvalEvent { get; private set; }

      public void On(AssignmentEvent ae) {
        AssignmentEvent = ae;
      }

      public void On(BinaryEvent be) {
        BinaryEvent = be;
      }

      public void On(ComparisonEvent ce) {
        ComparisonEvent = ce;
      }

      public void On(EvalEvent ee) {
        EvalEvent = ee;
      }
    }

    [Fact]
    public void TestExecuteBinaryExpression() {
      var left = Expression.NumberConstant(1, NewTextRange());
      var right = Expression.NumberConstant(2, NewTextRange());
      var binary = Expression.Binary(left, BinaryOperator.Add, right, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(binary);
      Assert.Equal(1, visualizer.BinaryEvent.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.BinaryEvent.Op);
      Assert.Equal(2, visualizer.BinaryEvent.Right.Number);
      Assert.Equal(3, visualizer.BinaryEvent.Result.Number);
      Assert.Equal(NewTextRange(), visualizer.BinaryEvent.Range);
    }

    [Fact]
    public void TestExecuteComparisonExpression() {
      var first = Expression.NumberConstant(1, NewTextRange());
      var second = Expression.NumberConstant(2, NewTextRange());
      var ops1 = new ComparisonOperator[] { ComparisonOperator.Less };
      var ops2 = new ComparisonOperator[] { ComparisonOperator.Greater };
      var exprs = new Expression[] { second };
      var comparison1 = Expression.Comparison(first, ops1, exprs, NewTextRange());
      var comparison2 = Expression.Comparison(first, ops2, exprs, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();

      executor.Run(comparison1);
      Assert.Equal(new NumberValue(1), visualizer.ComparisonEvent.First);
      Assert.Equal(ops1, visualizer.ComparisonEvent.Ops);
      var expectedExprs = new IValue[] { new NumberValue(2) };
      Assert.Equal(expectedExprs, visualizer.ComparisonEvent.Exprs);
      Assert.Equal(new BooleanValue(true), visualizer.ComparisonEvent.Result);
      Assert.Equal(NewTextRange(), visualizer.ComparisonEvent.Range);

      executor.Run(comparison2);
      Assert.Equal(new NumberValue(1), visualizer.ComparisonEvent.First);
      Assert.Equal(ops2, visualizer.ComparisonEvent.Ops);
      Assert.Equal(expectedExprs, visualizer.ComparisonEvent.Exprs);
      Assert.Equal(new BooleanValue(false), visualizer.ComparisonEvent.Result);
      Assert.Equal(NewTextRange(), visualizer.ComparisonEvent.Range);
    }

    [Fact]
    public void TestExecuteMultipleComparisonExpression() {
      var first = Expression.NumberConstant(1, NewTextRange());
      var second = Expression.NumberConstant(2, NewTextRange());
      var third = Expression.NumberConstant(3, NewTextRange());
      var ops1 = new ComparisonOperator[] { ComparisonOperator.Less, ComparisonOperator.Less };
      var ops2 = new ComparisonOperator[] { ComparisonOperator.Greater, ComparisonOperator.Less };
      var ops3 = new ComparisonOperator[] { ComparisonOperator.Less, ComparisonOperator.Greater };
      var exprs = new Expression[] { second, third };
      var comparison1 = Expression.Comparison(first, ops1, exprs, NewTextRange());
      var comparison2 = Expression.Comparison(first, ops2, exprs, NewTextRange());
      var comparison3 = Expression.Comparison(first, ops3, exprs, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();

      executor.Run(comparison1);
      Assert.Equal(new NumberValue(1), visualizer.ComparisonEvent.First);
      Assert.Equal(ops1, visualizer.ComparisonEvent.Ops);
      var expectedExprs = new IValue[] { new NumberValue(2), new NumberValue(3) };
      Assert.Equal(expectedExprs, visualizer.ComparisonEvent.Exprs);
      Assert.Equal(new BooleanValue(true), visualizer.ComparisonEvent.Result);
      Assert.Equal(NewTextRange(), visualizer.ComparisonEvent.Range);

      executor.Run(comparison2);
      Assert.Equal(new NumberValue(1), visualizer.ComparisonEvent.First);
      Assert.Equal(ops2, visualizer.ComparisonEvent.Ops);
      var expectedShortCircuitExprs = new IValue[] { new NumberValue(2), null };
      Assert.Equal(expectedShortCircuitExprs, visualizer.ComparisonEvent.Exprs);
      Assert.Equal(new BooleanValue(false), visualizer.ComparisonEvent.Result);
      Assert.Equal(NewTextRange(), visualizer.ComparisonEvent.Range);

      executor.Run(comparison3);
      Assert.Equal(new NumberValue(1), visualizer.ComparisonEvent.First);
      Assert.Equal(ops3, visualizer.ComparisonEvent.Ops);
      Assert.Equal(expectedExprs, visualizer.ComparisonEvent.Exprs);
      Assert.Equal(new BooleanValue(false), visualizer.ComparisonEvent.Result);
      Assert.Equal(NewTextRange(), visualizer.ComparisonEvent.Range);
    }

    [Fact]
    public void TestExecuteUnaryExpression() {
      string name = "id";
      var numberConstant = Expression.NumberConstant(1, NewTextRange());
      var unary = Expression.Unary(UnaryOperator.Negative, numberConstant, NewTextRange());
      var identifier = Expression.Identifier(name, NewTextRange());
      var assignment = Statement.Assignment(identifier, unary, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(assignment);
      Assert.Equal(name, visualizer.AssignmentEvent.Identifier);
      Assert.Equal(-1, visualizer.AssignmentEvent.Value.Number);
      Assert.Equal(NewTextRange(), visualizer.AssignmentEvent.Range);
    }

    [Fact]
    public void TestExecuteAssignmentStatement() {
      string name = "id";
      var identifier = Expression.Identifier(name, NewTextRange());
      var numberConstant = Expression.NumberConstant(1, NewTextRange());
      var assignment = Statement.Assignment(identifier, numberConstant, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(assignment);
      Assert.Equal(name, visualizer.AssignmentEvent.Identifier);
      Assert.Equal(1, visualizer.AssignmentEvent.Value.Number);
      Assert.Equal(NewTextRange(), visualizer.AssignmentEvent.Range);
    }

    [Fact]
    public void TestExecuteExpressionStatement() {
      var numberConstant1 = Expression.NumberConstant(1, NewTextRange());
      var numberConstant2 = Expression.NumberConstant(2, NewTextRange());
      var numberConstant3 = Expression.NumberConstant(3, NewTextRange());
      var left = Expression.Binary(numberConstant1, BinaryOperator.Add, numberConstant2, NewTextRange());
      var binary = Expression.Binary(left, BinaryOperator.Multiply, numberConstant3, NewTextRange());
      var expr = Statement.Expression(binary, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(expr);
      Assert.Equal(9, visualizer.EvalEvent.Value.Number);
      Assert.Equal(NewTextRange(), visualizer.EvalEvent.Range);
    }

    [Fact]
    public void TestExecuteExpressionStatementWithVariable() {
      (var executor, var visualizer) = NewExecutorWithVisualizer();

      var identifier = Expression.Identifier("a", NewTextRange());
      var numberConstant = Expression.NumberConstant(2, NewTextRange());
      var assignment = Statement.Assignment(identifier, numberConstant, NewTextRange());
      executor.Run(assignment);

      var right = Expression.NumberConstant(3, NewTextRange());
      var binary = Expression.Binary(identifier, BinaryOperator.Multiply, right, NewTextRange());
      var expr = Statement.Expression(binary, NewTextRange());
      executor.Run(expr);
      Assert.Equal(6, visualizer.EvalEvent.Value.Number);
      Assert.Equal(NewTextRange(), visualizer.EvalEvent.Range);
    }

    [Fact]
    public void TestExecuteDivideByZero() {
      var binary1 = Expression.Binary(Expression.NumberConstant(1, NewTextRange()),
                                      BinaryOperator.Divide,
                                      Expression.NumberConstant(0, NewTextRange()),
                                      NewTextRange());
      var binary2 = Expression.Binary(Expression.NumberConstant(0, NewTextRange()),
                                      BinaryOperator.Divide,
                                      Expression.NumberConstant(0, NewTextRange()),
                                      NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception1 = Assert.Throws<DiagnosticException>(() => executor.Run(binary1));
      Assert.Equal(Message.RuntimeErrorDivideByZero, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => executor.Run(binary2));
      Assert.Equal(Message.RuntimeErrorDivideByZero, exception2.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteOverflow() {
      var exception = Assert.Throws<DiagnosticException>(() =>
          Expression.NumberConstant(double.PositiveInfinity, NewTextRange()));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
      exception = Assert.Throws<DiagnosticException>(() =>
          Expression.NumberConstant(double.NegativeInfinity, NewTextRange()));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
      exception = Assert.Throws<DiagnosticException>(() =>
          Expression.NumberConstant(double.NaN, NewTextRange()));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);

      var binary = Expression.Binary(Expression.NumberConstant(7.997e307, NewTextRange()),
                                     BinaryOperator.Add,
                                     Expression.NumberConstant(9.985e307, NewTextRange()),
                                     NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      exception = Assert.Throws<DiagnosticException>(() => executor.Run(binary));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
    }

    private static TextRange NewTextRange() {
      return new TextRange(0, 1, 2, 3);
    }

    private static (Executor, MockupVisualizer) NewExecutorWithVisualizer() {
      var visualizer = new MockupVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(visualizer);
      var executor = new Executor(visualizerCenter);
      return (executor, visualizer);
    }
  }
}
