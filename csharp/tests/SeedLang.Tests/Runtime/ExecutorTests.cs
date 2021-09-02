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
    public void TestRun() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      executor.Run("eval 1 + 2", "", ProgrammingLanguage.Python, ParseRule.Statement, RunType.Ast);
      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
    }
  }
}
