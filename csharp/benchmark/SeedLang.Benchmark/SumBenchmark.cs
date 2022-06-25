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
  public class SumBenchmark {
    private readonly string _globalScopeSum = @"
sum = 0
i = 1
while i < 10000000:
  sum += i
  i += 1

sum
";

    private readonly string _globalScopeForSum = @"
sum = 0
for i in range(1, 10000000):
  sum += i

sum
";

    private readonly string _localScopeSum = @"
def func():
  sum = 0
  i = 1
  while i < 10000000:
    sum += i
    i += 1
  return sum

func()
";

    private readonly string _localScopeForSum = @"
def func():
  sum = 0
  for i in range(1, 10000000):
    sum += i
  return sum

func()
";

    [Benchmark(Baseline = true)]
    public void BenchmarkCSharpSum() {
      double sum = 0;
      for (double i = 0; i < 10000000; i++) {
        sum += i;
      }
    }

    [Benchmark]
    public void BenchmarkGlobalScopeSum() {
      Run(_globalScopeSum, false);
    }

    [Benchmark]
    public void BenchmarkGlobalScopeForSum() {
      Run(_globalScopeForSum, false);
    }


    [Benchmark]
    public void BenchmarkLocalScopeSum() {
      Run(_localScopeSum, false);
    }

    [Benchmark]
    public void BenchmarkLocalScopeForSum() {
      Run(_localScopeForSum, false);
    }

    [Benchmark]
    public void BenchmarkGlobalScopeSumWithVariableTrackingEnabled() {
      Run(_globalScopeSum, true);
    }

    [Benchmark]
    public void BenchmarkGlobalScopeForSumWithVariableTrackingEnabled() {
      Run(_globalScopeForSum, true);
    }


    // TODO: the result of local scope sum with variable tracking is almost as same as the one
    // without variable tracking. It seems not possible. Check it later.
    [Benchmark]
    public void BenchmarkLocalScopeSumWithVariableTrackingEnabled() {
      Run(_localScopeSum, true);
    }

    [Benchmark]
    public void BenchmarkLocalScopeForSumWithVariableTrackingEnabled() {
      Run(_localScopeForSum, true);
    }

    private static void Run(string source, bool isVariableTrackingEnabled) {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script) {
        IsVariableTrackingEnabled = isVariableTrackingEnabled
      };
      engine.Compile(source, "");
      engine.Run();
    }
  }
}
