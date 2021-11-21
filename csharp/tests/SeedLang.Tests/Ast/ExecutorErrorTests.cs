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
    public void TestExecuteOverflow() {
      var exception = Assert.Throws<DiagnosticException>(() =>
          Expression.NumberConstant(double.PositiveInfinity, _textRange));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
      exception = Assert.Throws<DiagnosticException>(() =>
          Expression.NumberConstant(double.NegativeInfinity, _textRange));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
      exception = Assert.Throws<DiagnosticException>(() =>
          Expression.NumberConstant(double.NaN, _textRange));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);

      var binary = Expression.Binary(Expression.NumberConstant(7.997e307, _textRange),
                                     BinaryOperator.Add,
                                     Expression.NumberConstant(9.985e307, _textRange),
                                     _textRange);

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      exception = Assert.Throws<DiagnosticException>(() => executor.Run(binary));
      Assert.Equal(Message.RuntimeOverflow, exception.Diagnostic.MessageId);
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
