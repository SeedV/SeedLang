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
    private class MockupVisualizer : IVisualizer<EvalEvent> {
      public IValue Result { get; private set; }

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

    [Fact]
    public void TestRunPythonSumFunc() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      string source = @"def func():
  sum = 0
  i = 1
  while i <= 10:
    sum = sum + i
    i = i + 1
  return sum
func()
";
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode));
      Assert.Equal(55, visualizer.Result.Number);
    }

    [Fact]
    public void TestRunPythonRecursiveSum() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      string source = @"def sum(n):
  if n == 1:
    return 1
  else:
    return n + sum(n - 1)
sum(10)
";
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode));
      Assert.Equal(55, visualizer.Result.Number);
    }

    [Fact]
    public void TestRunPythonRecursiveFib() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      string source = @"def fib(n):
  if n == 1 or n == 2:
    return 1
  else:
    return fib(n - 1) + fib(n - 2)
fib(10)
";
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode));
      Assert.Equal(55, visualizer.Result.Number);
    }

    [Fact]
    public void TestRunPythonNestedFunc() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      string source = @"def func():
  def inner_func():
    return 2
  return inner_func()
func()
";
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode));
      Assert.Equal(2, visualizer.Result.Number);
    }

    [Fact]
    public void TestRunPythonBubbleSort() {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      string source = @"array = [64, 34, 25, 12, 22, 11, 90]
n = len(array)
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
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode));
      Assert.Equal("[11, 12, 22, 25, 34, 64, 90]", visualizer.Result.ToString());
    }
  }
}
