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
      public IValue Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public IValue Right { get; private set; }
      public IValue Result { get; private set; }
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
      var left = Expression.Number(1, NewTextRage());
      var right = Expression.Number(2, NewTextRage());
      var binary = Expression.Binary(left, BinaryOperator.Add, right, NewTextRage());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(binary);
      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRage(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteUnaryExpression() {
      string name = "id";
      var number = Expression.Number(1, NewTextRage());
      var unary = Expression.Unary(UnaryOperator.Negative, number, NewTextRage());
      var identifier = Expression.Identifier(name, NewTextRage());
      var assignment = Statement.Assignment(identifier, unary, NewTextRage());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(assignment);
      Assert.Equal(name, visualizer.Identifier);
      Assert.Equal(-1, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRage(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteAssignmentStatement() {
      string name = "id";
      var identifier = Expression.Identifier(name, NewTextRage());
      var number = Expression.Number(1, NewTextRage());
      var assignment = Statement.Assignment(identifier, number, NewTextRage());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(assignment);
      Assert.Equal(name, visualizer.Identifier);
      Assert.Equal(1, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRage(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteEvalStatement() {
      var number1 = Expression.Number(1, NewTextRage());
      var number2 = Expression.Number(2, NewTextRage());
      var number3 = Expression.Number(3, NewTextRage());
      var left = Expression.Binary(number1, BinaryOperator.Add, number2, NewTextRage());
      var binary = Expression.Binary(left, BinaryOperator.Multiply, number3, NewTextRage());
      var eval = Statement.Eval(binary, NewTextRage());

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(eval);
      Assert.Equal(9, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRage(), visualizer.Range);
    }

    [Fact]
    public void TestExecuteEvalWithVariable() {
      (var executor, var visualizer) = NewExecutorWithVisualizer();

      var identifier = Expression.Identifier("a", NewTextRage());
      var number = Expression.Number(2, NewTextRage());
      var assignment = Statement.Assignment(identifier, number, NewTextRage());
      executor.Run(assignment);

      var right = Expression.Number(3, NewTextRage());
      var binary = Expression.Binary(identifier, BinaryOperator.Multiply, right, NewTextRage());
      var eval = Statement.Eval(binary, NewTextRage());
      executor.Run(eval);
      Assert.Equal(6, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRage(), visualizer.Range);
    }

    private static TextRange NewTextRage() {
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
