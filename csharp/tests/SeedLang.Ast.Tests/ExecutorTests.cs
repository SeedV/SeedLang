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

using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Ast.Tests {
  internal class MockupVisualizer : IVisualizer {
    public IValue Left { get; private set; }
    public IValue Right { get; private set; }
    public IValue Result { get; private set; }

    public void OnBinaryExpression(IValue left, IValue right, IValue result) {
      Left = left;
      Right = right;
      Result = result;
    }

    public void OnEvalStatement(IValue value) {
      Result = value;
    }
  }

  public class ExecutorTests {
    [Fact]
    public void TestExecuteBinaryExpression() {
      var left = Expression.Number(1);
      var right = Expression.Number(2);
      var binary = Expression.Binary(left, BinaryOperator.Add, right);
      var visualizer = new MockupVisualizer();
      var executor = new Executor(visualizer);
      executor.Run(binary);

      Assert.NotNull(visualizer.Left);
      Assert.NotNull(visualizer.Right);
      Assert.NotNull(visualizer.Result);

      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestExecuteEvalStatement() {
      var one = Expression.Number(1);
      var two = Expression.Number(2);
      var three = Expression.Number(3);
      var left = Expression.Binary(one, BinaryOperator.Add, two);
      var binary = Expression.Binary(left, BinaryOperator.Multiply, three);
      var eval = Statement.Eval(binary);
      var visualizer = new MockupVisualizer();
      var executor = new Executor(visualizer);
      executor.Run(eval);

      Assert.NotNull(visualizer.Result);
      Assert.Equal(9, visualizer.Result.ToNumber());
    }
  }
}
