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

using System.IO;
using Xunit;

namespace SeedLang.Tests {
  public class RunPythonTests {
    [Fact]
    public void TestOperators() {
      string source = @"
1 + 2
2 - 1
2 * 2
3 / 2
5 // 2
3 % 2
3 ** 2
'123' + '456'
[1, 2, 3] + [4, 5]
(1, 2, 3) + (4,)
'123' * 3
'123' * 0
2 * [1, 2, 3]
(1, 2, 3) * 3
-1 * (1, 2, 3)
1 < 2 < 3
1 < 2 == 3
1 < 2 < 3 and 1 < 2 == 3
1 < 2 < 3 or 1 < 2 == 3
None == None
None != None
";
      string result = @"3
1
4
1.5
2
1
9
'123456'
[1, 2, 3, 4, 5]
(1, 2, 3, 4)
'123123123'
''
[1, 2, 3, 1, 2, 3]
(1, 2, 3, 1, 2, 3, 1, 2, 3)
()
True
False
False
True
True
False
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestWhileSum() {
      string source = @"
sum = 0
i = 1
while i <= 10:
  sum += i
  i += 1
sum
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestForInListSumFunc() {
      string source = @"
sum = 0
for i in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]:
  sum = sum + i
print(sum)
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestWhileSumFunc() {
      string source = @"
def func():
  sum = 0
  i = 1
  while i <= 10:
    sum = sum + i
    i = i + 1
  return sum
func()
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestForInRangeSumFunc() {
      string source = @"
def func():
  sum = 0
  for i in range(1, 11):
    sum += i
  return sum
print(func())
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestRecursiveSum() {
      string source = @"
def sum(n):
  if n == 1:
    return 1
  else:
    return n + sum(n - 1)
print(sum(10))
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestFib() {
      string source = @"
a, b = 0, 1
i = 1
while i < 10:
  a, b = b, a + b
  i += 1
b
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestFibFunc() {
      string source = @"
def fib(n):
  a, b = 0, 1
  i = 1
  while i < n:
    a, b = b, a + b
    i += 1
  return b
print(fib(10))
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestRecursiveFib() {
      string source = @"
def fib(n):
  if n == 1 or n == 2:
    return 1
  else:
    return fib(n - 1) + fib(n - 2)
fib(10)
";
      string result = @"55
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestNestedFunction() {
      string source = @"
def func():
  def inner_func():
    return 2
  return inner_func()
print(func())
";
      string result = @"2
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestBubbleSort() {
      string source = @"
array = [64, 34, 25, 12, 22, 11, 90]
n = len(array)
for i in range(n):
  for j in range(n - i - 1):
    if array[j] > array[j + 1]:
      array[j], array[j + 1] = array[j + 1], array[j]
array
";
      string result = @"[11, 12, 22, 25, 34, 64, 90]
";
      TestExecutor(source, result);
    }

    [Fact]
    public void TestArrayAppend() {
      string source = @"
array = []
for i in range(5):
  array.append(i)
print(array)
";
      string result = @"[0, 1, 2, 3, 4]
";
      TestExecutor(source, result);
    }

    private static void TestExecutor(string source, string result) {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Interactive);
      var stringWriter = new StringWriter();
      engine.RedirectStdout(stringWriter);
      engine.Compile(source, "");
      engine.Run();
      Assert.Equal(result, stringWriter.ToString());
    }
  }
}
