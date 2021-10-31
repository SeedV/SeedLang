using System.Security.Principal;
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
                                     IVisualizer<EvalEvent> {
      public string Identifier { get; private set; }
      public Value Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public Value Right { get; private set; }
      public Value Result { get; private set; }
      public Range Range { get; private set; }

      public void On(AssignmentEvent ae) {
        Identifier = ae.Identifier;
        Result = ae.Value;
        Range = ae.Range;
      }

      public void On(BinaryEvent be) {
        Left = be.Left;
        Op = be.Op;
        Right = be.Right;
        Result = be.Result;
        Range = be.Range;
      }

      public void On(EvalEvent ee) {
        Result = ee.Value;
        Range = ee.Range;
      }
    }

    [Fact]
    public void TestExecuteBinaryExpression() {
      var left = Expression.Number(1, NewTextRange());
      var right = Expression.Number(2, NewTextRange());
      var binary = Expression.Binary(left, BinaryOperator.Add, right, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(binary);
      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRange(), visualizer.Range);
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
      Assert.Equal(name, visualizer.Identifier);
      Assert.Equal(-1, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRange(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteAssignmentStatement() {
      string name = "id";
      var identifier = Expression.Identifier(name, NewTextRange());
      var number = Expression.Number(1, NewTextRange());
      var assignment = Statement.Assignment(identifier, number, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(assignment);
      Assert.Equal(name, visualizer.Identifier);
      Assert.Equal(1, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRange(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteEvalStatement() {
      var number1 = Expression.Number(1, NewTextRange());
      var number2 = Expression.Number(2, NewTextRange());
      var number3 = Expression.Number(3, NewTextRange());
      var left = Expression.Binary(number1, BinaryOperator.Add, number2, NewTextRange());
      var binary = Expression.Binary(left, BinaryOperator.Multiply, number3, NewTextRange());
      var eval = Statement.Expression(binary, NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(eval);
      Assert.Equal(9, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRange(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteEvalWithVariable() {
      (var executor, var visualizer) = NewExecutorWithVisualizer();

      var identifier = Expression.Identifier("a", NewTextRange());
      var number = Expression.Number(2, NewTextRange());
      var assignment = Statement.Assignment(identifier, number, NewTextRange());
      executor.Run(assignment);

      var right = Expression.Number(3, NewTextRange());
      var binary = Expression.Binary(identifier, BinaryOperator.Multiply, right, NewTextRange());
      var eval = Statement.Expression(binary, NewTextRange());
      executor.Run(eval);
      Assert.Equal(6, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRange(), visualizer.Range);
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
      var binary1 = Expression.Binary(Expression.Number(7.997e307, NewTextRange()),
                                      BinaryOperator.Add,
                                      Expression.Number(9.985e307, NewTextRange()),
                                      NewTextRange());
      var binary2 = Expression.Binary(Expression.Number(double.PositiveInfinity, NewTextRange()),
                                      BinaryOperator.Add,
                                      Expression.Number(1, NewTextRange()),
                                      NewTextRange());
      var assign = Statement.Assignment(Expression.Identifier("id", NewTextRange()),
                                        Expression.Number(double.NegativeInfinity, NewTextRange()),
                                        NewTextRange());
      var eval = Statement.Expression(Expression.Number(double.NaN, NewTextRange()), NewTextRange());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception1 = Assert.Throws<DiagnosticException>(() => executor.Run(binary1));
      Assert.Equal(Message.RuntimeOverflow, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => executor.Run(binary2));
      Assert.Equal(Message.RuntimeOverflow, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => executor.Run(assign));
      Assert.Equal(Message.RuntimeOverflow, exception3.Diagnostic.MessageId);
      var exception4 = Assert.Throws<DiagnosticException>(() => executor.Run(eval));
      Assert.Equal(Message.RuntimeOverflow, exception4.Diagnostic.MessageId);
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
