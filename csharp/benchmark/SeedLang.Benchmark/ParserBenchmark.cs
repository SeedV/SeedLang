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
  // Code editors can use SeedLang's syntax parsing result to highlight syntax/semantic tokens.
  // Hence, we measure the performance of the parser separately with this benchmark.
  public class ParserBenchmark {
    private readonly string _pythonCode = @"def get_sum(min, max):
  i = min
  sum = 0
  while i <= max:
    sum = sum + i
    i = i + 1
  return sum

def fib1(n):
  a, b = 0, 1
  i = 1
  while i < n:
    a, b = b, a + b
    i = i + 1
  return b

def fib2(n):
  a, b = 0, 1
  for i in range(1, n):
    a, b = b, a + b
  return b

def fib3(n):
  if n == 1 or n == 2:
    return 1
  else:
    return fib(n - 1) + fib(n - 2)
";

    [Benchmark]
    public void BenchmarkSyntaxParsePython() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      engine.ParseSyntaxTokens(_pythonCode, "");
    }

    [Benchmark]
    public void BenchmarkParsePythonWithSyntaxErrors() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      var codeWithErrors = _pythonCode.Replace("return", "$$$");
      engine.ParseSyntaxTokens(codeWithErrors, "");
    }

    [Benchmark]
    public void BenchmarkSemanticParsePython() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      engine.Compile(_pythonCode, "");
    }
  }
}
