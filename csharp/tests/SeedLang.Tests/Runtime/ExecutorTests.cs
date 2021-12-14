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

using SeedLang.Block;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ExecutorTests {
    private class MockupVisualizer : IVisualizer<BinaryEvent>,
                                     IVisualizer<EvalEvent> {
      public IValue Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public IValue Right { get; private set; }
      public IValue Result { get; private set; }

      public void On(BinaryEvent be) {
        Left = be.Left;
        Op = be.Op;
        Right = be.Right;
        Result = be.Result;
      }

      public void On(EvalEvent ee) {
        Result = ee.Value;
      }
    }

    [Fact]
    public void TestRunBlockProgram() {
      var program = new Program();
      var module = new Module();
      var expressionBlock = new ExpressionBlock();
      var numberBlock1 = new NumberBlock { Value = "1" };
      var operatorBlock = new ArithmeticOperatorBlock { Name = "+" };
      var numberBlock2 = new NumberBlock { Value = "2" };
      module.AddStandaloneBlock(expressionBlock);
      module.AddStandaloneBlock(numberBlock1);
      module.AddStandaloneBlock(operatorBlock);
      module.AddStandaloneBlock(numberBlock2);
      expressionBlock.Dock(numberBlock1, Position.DockType.Input, 0);
      expressionBlock.Dock(operatorBlock, Position.DockType.Input, 1);
      expressionBlock.Dock(numberBlock2, Position.DockType.Input, 2);
      program.Add(module);

      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      executor.Run(program);

      Assert.Equal(1, visualizer.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.Number);
      Assert.Equal(3, visualizer.Result.Number);
    }

    [Fact]
    public void TestRunPythonSumProgram() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);

      string source = @"sum = 0
i = 1
while i <= 10:
  sum = sum + i
  i = i + 1
sum
";
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Ast));
      Assert.Equal(55, visualizer.Result.Number);
    }

    [Fact]
    public void TestRunPythonBubbleSortProgram() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);

      string source = @"array = [64, 34, 25, 12, 22, 11, 90]
n = 7
i = 0
while i < n:
  j = 0
  while j < n - i - 1:
    if array[j] > array[j + 1]:
      temp = array[j]
      array[j] = array[j + 1]
      array[j + 1] = temp
    j = j + 1
  i = i + 1
array
";
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Ast));
      Assert.Equal("[11, 12, 22, 25, 34, 64, 90]", visualizer.Result.ToString());
    }
  }
}
