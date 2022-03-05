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
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExecutorErrorTests {
    [Fact]
    public void TestExecuteDivideByZero() {
      var binary1 = AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Divide,
                                     AstHelper.NumberConstant(0));
      var binary2 = AstHelper.Binary(AstHelper.NumberConstant(0), BinaryOperator.Divide,
                                     AstHelper.NumberConstant(0));

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception1 = Assert.Throws<DiagnosticException>(
          () => executor.Run(binary1, RunMode.Script));
      Assert.Equal(Message.RuntimeErrorDivideByZero, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(
          () => executor.Run(binary2, RunMode.Script));
      Assert.Equal(Message.RuntimeErrorDivideByZero, exception2.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteOutOfRange() {
      var eval = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(3)
      ));
      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception = Assert.Throws<DiagnosticException>(
          () => executor.Run(eval, RunMode.Interactive));
      Assert.Equal(Message.RuntimeErrorOutOfRange, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteNotSubscriptable() {
      var subscript = AstHelper.Subscript(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2));

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception = Assert.Throws<DiagnosticException>(
          () => executor.Run(subscript, RunMode.Script));
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestExecuteInvalidListIndex() {
      var subscript = AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2)),
        AstHelper.NumberConstant(0.1)
      );

      (var executor, var visualizer) = NewExecutorWithVisualizer();
      var exception = Assert.Throws<DiagnosticException>(
          () => executor.Run(subscript, RunMode.Script));
      Assert.Equal(Message.RuntimeErrorInvalidIndex, exception.Diagnostic.MessageId);
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
