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
