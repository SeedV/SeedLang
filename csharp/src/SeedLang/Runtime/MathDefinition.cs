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
using SeedLang.Common;
using SeedLang.Runtime.HeapObjects;

namespace SeedLang.Runtime {
  internal static class MathDefinition {
    public const string PI = "pi";
    public const string E = "e";

    public const string FAbs = "fabs";
    public const string Sin = "sin";

    public static Dictionary<string, VMValue> Variables = new Dictionary<string, VMValue> {
      [PI] = new VMValue(Math.PI),
      [E] = new VMValue(Math.E),
      [FAbs] = new VMValue(new NativeFunction(FAbs, FAbsFunc)),
      [Sin] = new VMValue(new NativeFunction(Sin, SinFunc)),
    };

    private static VMValue FAbsFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(Math.Abs(args[0].AsNumber()));
    }

    private static VMValue SinFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(Math.Sin(args[0].AsNumber()));
    }
  }
}
