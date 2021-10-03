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
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExecutorTests : IDisposable {
    private class MockupVisualizer : IVisualizer<AssignmentEvent>,
                                     IVisualizer<BinaryEvent>,
                                     IVisualizer<EvalEvent> {
      public string Identifier { get; private set; }
      public IValue Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public IValue Right { get; private set; }
      public IValue Result { get; private set; }

      public void On(AssignmentEvent e) {
        Identifier = e.Identifier;
        Result = e.Value;
      }

      public void On(BinaryEvent e) {
        Left = e.Left;
        Op = e.Op;
        Right = e.Right;
        Result = e.Result;
      }

      public void On(EvalEvent e) {
        Result = e.Value;
      }
    }

    private readonly MockupVisualizer _visualizer = new MockupVisualizer();
    private readonly VisualizerCenter _visualizerCenter = new VisualizerCenter();
    private readonly Executor _executor;

    public ExecutorTests() {
      _visualizerCenter.Register(_visualizer);
      _executor = new Executor(_visualizerCenter);
    }

    public void Dispose() {
      _visualizerCenter.Unregister(_visualizer);
      GC.SuppressFinalize(this);
    }

    [Fact]
    public void TestExecuteBinaryExpression() {
      // var left = Expression.Number(1);
      // var right = Expression.Number(2);
      // var binary = Expression.Binary(left, BinaryOperator.Add, right);
      // _executor.Run(binary);

      // Assert.Equal(1, _visualizer.Left.ToNumber());
      // Assert.Equal(BinaryOperator.Add, _visualizer.Op);
      // Assert.Equal(2, _visualizer.Right.ToNumber());
      // Assert.Equal(3, _visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestExecuteUnaryExpression() {
      // string name = "id";
      // var unary = Expression.Unary(UnaryOperator.Negative, Expression.Number(1));
      // var assignment = Statement.Assignment(Expression.Identifier(name, null), unary);
      // _executor.Run(assignment);
      // Assert.Equal(name, _visualizer.Identifier);
      // Assert.Equal(-1, _visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestExecuteAssignmentStatement() {
      // string name = "id";
      // var assignment = Statement.Assignment(Expression.Identifier(name, null), Expression.Number(1));
      // _executor.Run(assignment);
      // Assert.Equal(name, _visualizer.Identifier);
      // Assert.Equal(1, _visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestExecuteEvalStatement() {
      // var left = Expression.Binary(Expression.Number(1), BinaryOperator.Add, Expression.Number(2));
      // var binary = Expression.Binary(left, BinaryOperator.Multiply, Expression.Number(3));
      // var eval = Statement.Eval(binary);
      // _executor.Run(eval);
      // Assert.Equal(9, _visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestExecuteEvalWithVariable() {
      // var identifier = Expression.Identifier("a", null);
      // var assignment = Statement.Assignment(identifier, Expression.Number(2));
      // _executor.Run(assignment);

      // var binary = Expression.Binary(identifier, BinaryOperator.Multiply, Expression.Number(3));
      // var eval = Statement.Eval(binary);
      // _executor.Run(eval);
      // Assert.Equal(6, _visualizer.Result.ToNumber());
    }
  }
}
