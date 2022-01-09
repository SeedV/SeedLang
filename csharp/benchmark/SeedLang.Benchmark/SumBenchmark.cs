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
using SeedLang.Runtime;

namespace SeedLang.Benchmark {
  public class SumBenchmark {
    private readonly Executor _executor = new Executor();

    [Benchmark]
    public void BenchmarkAst() {
      string source = @"sum = 0
i = 1
while i <= 10000000:
  sum = sum + i
  i = i + 1
sum
";
      _executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Ast);
    }

    [Benchmark]
    public void BenchmarkBytecode() {
      string source = @"sum = 0
i = 1
while i <= 10000000:
  sum = sum + i
  i = i + 1
sum
";
      _executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode);
    }

    [Benchmark]
    public void BenchmarkBytecodeLocalScope() {
      string source = @"def func():
  sum = 0
  i = 1
  while i <= 10000000:
    sum = sum + i
    i = i + 1
  return sum
func()
";
      _executor.Run(source, "", SeedXLanguage.SeedPython, RunType.Bytecode);
    }
  }
}
