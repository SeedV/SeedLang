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

using BenchmarkDotNet.Attributes;

namespace SeedLang.Benchmark {
  public class FibBenchmark {
    private readonly string _fibSource = @"def fib(n):
  a, b = 0, 1
  i = 1
  while i < n:
    a, b = b, a + b
    i = i + 1
  return b
fib(35)
";

    private readonly string _forFibSource = @"def fib(n):
  a, b = 0, 1
  for i in range(1, n):
    a, b = b, a + b
  return b
fib(35)
";

    private readonly string _recursiveFibSource = @"def fib(n):
  if n == 1 or n == 2:
    return 1
  else:
    return fib(n - 1) + fib(n - 2)
fib(35)
";

    [Benchmark]
    public void BenchmarkFib() {
      Run(_fibSource);
    }

    [Benchmark]
    public void BenchmarkForFib() {
      Run(_forFibSource);
    }

    [Benchmark]
    public void BenchmarkRecursiveFib() {
      Run(_recursiveFibSource);
    }

    private static void Run(string source) {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      engine.Compile(source, "");
      engine.Run();
    }
  }
}
