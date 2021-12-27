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

using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ExecutorBytecodeTests {
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
    public void TestRunPythonSum() {
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
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode));
      Assert.Equal(55, visualizer.Result.Number);
    }
  }
}
