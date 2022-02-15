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
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ExecutorBytecodeTests {
    private class MockupVisualizer : IVisualizer<EvalEvent> {
      public IValue Result { get; private set; }

      public void On(EvalEvent ee) {
        Result = ee.Value;
      }
    }

    [Theory]
    [InlineData(@"sum = 0
i = 1
while i <= 10:
  sum = sum + i
  i = i + 1
sum
",

    "55")]

    [InlineData(@"sum = 0
for i in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]:
  sum = sum + i
sum
",

    "55")]

    [InlineData(@"def func():
  sum = 0
  i = 1
  while i <= 10:
    sum = sum + i
    i = i + 1
  return sum
func()
",

    "55")]

    [InlineData(@"def func():
  sum = 0
  for i in range(1, 11):
    sum = sum + i
  return sum
func()
",

    "55")]

    [InlineData(@"def sum(n):
  if n == 1:
    return 1
  else:
    return n + sum(n - 1)
sum(10)
",

    "55")]

    [InlineData(@"a, b = 0, 1
i = 1
while i < 10:
  a, b = b, a + b
  i = i + 1
b
",

    "55")]

    [InlineData(@"def fib(n):
  a, b = 0, 1
  i = 1
  while i < n:
    a, b = b, a + b
    i = i + 1
  return b
fib(10)
",

    "55")]

    [InlineData(@"def fib(n):
  if n == 1 or n == 2:
    return 1
  else:
    return fib(n - 1) + fib(n - 2)
fib(10)
",

    "55")]

    [InlineData(@"def func():
  def inner_func():
    return 2
  return inner_func()
func()
",

    "2")]

    [InlineData(@"array = [64, 34, 25, 12, 22, 11, 90]
n = len(array)
for i in range(n):
  for j in range(n - i - 1):
    if array[j] > array[j + 1]:
      array[j], array[j + 1] = array[j + 1], array[j]
array
",

    "[11, 12, 22, 25, 34, 64, 90]")]

    [InlineData(@"array = []
for i in range(5):
  array.append(i)
array
",

    "[0, 1, 2, 3, 4]")]
    public void TestExecutor(string source, string result) {
      TestWithRunType(source, result, RunType.Ast);
      TestWithRunType(source, result, RunType.Bytecode);
    }

    private static void TestWithRunType(string source, string result, RunType type) {
      var executor = new Executor();
      var visualizer = new MockupVisualizer();
      executor.Register(visualizer);
      Assert.True(executor.Run(source, "", SeedXLanguage.SeedPython, type));
      Assert.Equal(result, visualizer.Result.ToString());
    }
  }
}
