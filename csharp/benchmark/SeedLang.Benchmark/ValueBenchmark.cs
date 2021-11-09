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

using BenchmarkDotNet.Attributes;
using SeedLang.Interpreter;
using SeedLang.Runtime;

namespace SeedLang.Benchmark {
  public class ValueBenchmark {
    // Benchmarks binary expression running time of the AST executor.
    [Benchmark]
    public void BenchmarkAddingNumberValue() {
      var left = new NumberValue(1);
      var right = new NumberValue(2);
      double _ = ValueHelper.Add(left, right);
    }

    // Benchmarks binary expression running time of the VM. Compiling time is not included.
    [Benchmark]
    public void BenchmarkAddingVMValue() {
      var left = new VMValue(1);
      var right = new VMValue(2);
      double _ = ValueHelper.Add(in left, in right);
    }
  }
}
