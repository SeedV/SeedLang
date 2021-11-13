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
                                     IVisualizer<CompareEvent>,
                                     IVisualizer<EvalEvent> {
      public AssignmentEvent AssignmentEvent { get; private set; }
      public BinaryEvent BinaryEvent { get; private set; }
      public CompareEvent CompareEvent { get; private set; }
      public EvalEvent EvalEvent { get; private set; }

      public void On(AssignmentEvent ae) {
        AssignmentEvent = ae;
      }

      public void On(BinaryEvent be) {
        BinaryEvent = be;
      }

      public void On(CompareEvent ce) {
        CompareEvent = ce;
      }

      public void On(EvalEvent ee) {
        EvalEvent = ee;
      }
    }

    [Fact]
    public void TestExecuteBinaryExpression() {
      var left = Expression.Number(1, NewTextRange());
      var right = Expression.Number(2, NewTextRange());
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
    public void TestExecuteCompareExpression() {
      var left = Expression.Number(1, NewTextRange());
      var right = Expression.Number(2, NewTextRange());
      var exprs = new Expression[] { left, right };
      var ops = new CompareOperator[] { CompareOperator.Less };
      var compare = Expression.Compare(exprs, ops, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(compare);

      var expectedExprs = new IValue[] { new NumberValue(1), new NumberValue(2) };
      Assert.Equal(expectedExprs, visualizer.CompareEvent.Exprs);
      var expectedOps = new CompareOperator[] { CompareOperator.Less };
      Assert.Equal(expectedOps, visualizer.CompareEvent.Ops);
      Assert.Equal(new BooleanValue(true), visualizer.CompareEvent.Result);
      Assert.Equal(NewTextRange(), visualizer.CompareEvent.Range);
    }

    [Fact]
    public void TestExecuteUnaryExpression() {
      string name = "id";
      var number = Expression.Number(1, NewTextRange());
      var unary = Expression.Unary(UnaryOperator.Negative, number, NewTextRange());
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
      var number = Expression.Number(1, NewTextRange());
      var assignment = Statement.Assignment(identifier, number, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(assignment);
      Assert.Equal(name, visualizer.AssignmentEvent.Identifier);
      Assert.Equal(1, visualizer.AssignmentEvent.Value.Number);
      Assert.Equal(NewTextRange(), visualizer.AssignmentEvent.Range);
    }

    [Fact]
    public void TestExecuteExpressionStatement() {
      var number1 = Expression.Number(1, NewTextRange());
      var number2 = Expression.Number(2, NewTextRange());
      var number3 = Expression.Number(3, NewTextRange());
      var left = Expression.Binary(number1, BinaryOperator.Add, number2, NewTextRange());
      var binary = Expression.Binary(left, BinaryOperator.Multiply, number3, NewTextRange());
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
      var number = Expression.Number(2, NewTextRange());
      var assignment = Statement.Assignment(identifier, number, NewTextRange());
      executor.Run(assignment);

      var right = Expression.Number(3, NewTextRange());
      var binary = Expression.Binary(identifier, BinaryOperator.Multiply, right, NewTextRange());
      var expr = Statement.Expression(binary, NewTextRange());
      executor.Run(expr);
      Assert.Equal(6, visualizer.EvalEvent.Value.Number);
      Assert.Equal(NewTextRange(), visualizer.EvalEvent.Range);
    }

    [Fact]
    public void TestExecuteDivideByZero() {
      var binary1 = Expression.Binary(Expression.Number(1, NewTextRange()),
                                      BinaryOperator.Divide,
                                      Expression.Number(0, NewTextRange()),
                                      NewTextRange());
      var binary2 = Expression.Binary(Expression.Number(0, NewTextRange()),
                                      BinaryOperator.Divide,
                                      Expression.Number(0, NewTextRange()),
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
          Expression.Number(double.PositiveInfinity, NewTextRange()));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
      exception = Assert.Throws<DiagnosticException>(() =>
          Expression.Number(double.NegativeInfinity, NewTextRange()));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
      exception = Assert.Throws<DiagnosticException>(() =>
          Expression.Number(double.NaN, NewTextRange()));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);

      var binary = Expression.Binary(Expression.Number(7.997e307, NewTextRange()),
                                     BinaryOperator.Add,
                                     Expression.Number(9.985e307, NewTextRange()),
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
