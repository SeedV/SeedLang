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
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ExecutorTests {
    private class MockupVisualizer : IVisualizer<BinaryEvent>,
                                     IVisualizer<EvalEvent> {
      public IValue Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public IValue Right { get; private set; }
      public IValue Result { get; private set; }

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

      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestRunBlockStatement() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);

      executor.Run("set a to 1", "", ProgrammingLanguage.Block, ParseRule.Statement, RunType.Ast);
      executor.Run("set b to 2", "", ProgrammingLanguage.Block, ParseRule.Statement, RunType.Ast);
      executor.Run("eval a + b", "", ProgrammingLanguage.Block, ParseRule.Statement, RunType.Ast);

      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
    }

    [Fact]
    public void TestRunPythonStatement() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);

      executor.Run("a = 1", "", ProgrammingLanguage.Python, ParseRule.Statement, RunType.Ast);
      executor.Run("b = 2", "", ProgrammingLanguage.Python, ParseRule.Statement, RunType.Ast);
      executor.Run("eval a + -b", "", ProgrammingLanguage.Python, ParseRule.Statement, RunType.Ast);

      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(-2, visualizer.Right.ToNumber());
      Assert.Equal(-1, visualizer.Result.ToNumber());
    }
  }
}
