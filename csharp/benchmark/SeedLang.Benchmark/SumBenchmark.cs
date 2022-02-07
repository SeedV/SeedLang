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
    private readonly string _globalScopeSum = @"sum = 0
i = 1
while i <= 10000000:
  sum = sum + i
  i = i + 1
sum
";

    private readonly string _globalScopeForSum = @"sum = 0
for i in range(1, 10000001):
  sum = sum + i
sum
";

    private readonly string _localScopeSum = @"def func():
  sum = 0
  i = 1
  while i <= 10000000:
    sum = sum + i
    i = i + 1
  return sum
func()
";

    private readonly string _localScopeForSum = @"def func():
  sum = 0
  for i in range(1, 10000001):
    sum = sum + i
  return sum
func()
";

    [Benchmark]
    public void BenchmarkAstSum() {
      var executor = new Executor();
      executor.Run(_globalScopeSum, "", SeedXLanguage.SeedPython, RunType.Ast);
    }

    [Benchmark]
    public void BenchmarkBytecodeGlobalScopeSum() {
      var executor = new Executor();
      executor.Run(_globalScopeSum, "", SeedXLanguage.SeedPython, RunType.Bytecode);
    }

    [Benchmark]
    public void BenchmarkBytecodeGlobalScopeForSum() {
      var executor = new Executor();
      executor.Run(_globalScopeForSum, "", SeedXLanguage.SeedPython, RunType.Bytecode);
    }


    [Benchmark]
    public void BenchmarkBytecodeLocalScopeSum() {
      var executor = new Executor();
      executor.Run(_localScopeSum, "", SeedXLanguage.SeedPython, RunType.Bytecode);
    }

    [Benchmark]
    public void BenchmarkBytecodeLocalScopeForSum() {
      var executor = new Executor();
      executor.Run(_localScopeForSum, "", SeedXLanguage.SeedPython, RunType.Bytecode);
    }
  }
}
