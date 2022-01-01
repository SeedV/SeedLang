// Copyright 2021-2022 The SeedV Lab.
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
  public class ExecutorErrorTests {
    private static TextRange _textRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestExecuteDivideByZero() {
      var zero = Expression.NumberConstant(0, _textRange);
      var one = Expression.NumberConstant(1, _textRange);
      var binary1 = Expression.Binary(one, BinaryOperator.Divide, zero, _textRange);
      var binary2 = Expression.Binary(zero, BinaryOperator.Divide, zero, _textRange);

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception1 = Assert.Throws<DiagnosticException>(() => executor.Run(binary1));
      Assert.Equal(Message.RuntimeErrorDivideByZero, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => executor.Run(binary2));
      Assert.Equal(Message.RuntimeErrorDivideByZero, exception2.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteOutOfRange() {
      var one = Expression.NumberConstant(1, _textRange);
      var two = Expression.NumberConstant(2, _textRange);
      var three = Expression.NumberConstant(3, _textRange);
      var list = Expression.List(new Expression[] { one, two, three }, _textRange);
      var subscript = Expression.Subscript(list, three, _textRange);
      var eval = Statement.Expression(subscript, _textRange);
      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception = Assert.Throws<DiagnosticException>(() => executor.Run(eval));
      Assert.Equal(Message.RuntimeErrorOutOfRange, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteNotSubscriptable() {
      var one = Expression.NumberConstant(1, _textRange);
      var two = Expression.NumberConstant(2, _textRange);
      var subscript = Expression.Subscript(one, two, _textRange);

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception = Assert.Throws<DiagnosticException>(() => executor.Run(subscript));
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteInvalidListIndex() {
      var one = Expression.NumberConstant(1, _textRange);
      var two = Expression.NumberConstant(2, _textRange);
      var list = Expression.List(new Expression[] { one, two }, _textRange);
      var index = Expression.NumberConstant(0.1, _textRange);
      var subscript = Expression.Subscript(list, index, _textRange);

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception = Assert.Throws<DiagnosticException>(() => executor.Run(subscript));
      Assert.Equal(Message.RuntimeErrorInvalidListIndex, exception.Diagnostic.MessageId);
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
