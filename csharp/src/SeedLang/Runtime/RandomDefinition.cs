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
using SeedLang.Common;

namespace SeedLang.Runtime {
  // The static class to define all functions for the builtin random library.
  internal static class RandomDefinition {
    public const string RandInt = "randint";
    public const string Random = "random";
    public const string RandRange = "randrange";
    public const string Seed = "seed";

    private static Random _rand = new Random();

    internal static VMValue RandIntFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 2) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      int minValue = ValueHelper.DoubleToInt(args[0].AsNumber());
      int maxValue = ValueHelper.DoubleToInt(args[1].AsNumber()) + 1;
      return new VMValue(_rand.Next(minValue, maxValue));
    }

    internal static VMValue RandomFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 0) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(_rand.NextDouble());
    }

    internal static VMValue RandRangeFunc(ValueSpan args, INativeContext _) {
      var range = args.Count switch {
        1 => new HeapObjects.Range(args[0].AsNumber()),
        2 => new HeapObjects.Range(args[0].AsNumber(), args[1].AsNumber()),
        3 => new HeapObjects.Range(args[0].AsNumber(), args[1].AsNumber(), args[2].AsNumber()),
        _ => throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                           Message.RuntimeErrorIncorrectArgsCount),
      };
      return range[_rand.Next(range.Length)];
    }

    internal static VMValue SeedFunc(ValueSpan args, INativeContext _) {
      _rand = args.Count switch {
        0 => new Random(),
        1 => new Random((int)args[0].AsNumber()),
        _ => throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                           Message.RuntimeErrorIncorrectArgsCount),
      };
      return new VMValue();
    }
  }
}
