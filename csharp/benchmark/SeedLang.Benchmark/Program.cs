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

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using CommandLine;
using CommandLine.Text;

namespace SeedLang.Benchmark {
  class Program {
    internal enum BenchmarkType {
      Fib,
      Parser,
      Sum,
      All,
    }

    internal class Options {
      [Option('b', "Benchmark", Required = false, Default = BenchmarkType.All,
              HelpText = "The benchmark to run.")]
      public BenchmarkType BenchmarkType { get; set; }
    }

    private static readonly Dictionary<BenchmarkType, Type> _benchmarks =
        new Dictionary<BenchmarkType, Type> {
          [BenchmarkType.Fib] = typeof(FibBenchmark),
          [BenchmarkType.Parser] = typeof(ParserBenchmark),
          [BenchmarkType.Sum] = typeof(SumBenchmark),
        };

    static void Main(string[] args) {
      var parser = new Parser(with => with.HelpWriter = null);
      var result = parser.ParseArguments<Options>(args);
      var helpText = HelpText.AutoBuild(result, h => {
        h.AddEnumValuesToHelpText = true;
        return HelpText.DefaultParsingErrorsHandler(result, h);
      }, e => e);
      result.WithParsed<Options>(options => {
        if (options.BenchmarkType == BenchmarkType.All) {
          foreach (var benchmark in _benchmarks.Values) {
            BenchmarkRunner.Run(benchmark);
          }
        } else {
          BenchmarkRunner.Run(_benchmarks[options.BenchmarkType]);
        }
      }).WithNotParsed(errors => {
        Console.Error.Write(helpText);
      });
    }
  }
}
