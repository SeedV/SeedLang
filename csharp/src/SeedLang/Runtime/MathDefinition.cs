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
  // The static class to define all constants and functions for the builtin math library.
  internal static class MathDefinition {
    public const string E = "e";
    public const string PI = "pi";
    public const string Tau = "tau";

    public const string Acos = "acos";
    public const string Acosh = "acosh";
    public const string Asin = "asin";
    public const string Asinh = "asinh";
    public const string Atan = "atan";
    public const string Atan2 = "atan2";
    public const string Atanh = "atanh";
    public const string Ceil = "ceil";
    public const string Cos = "cos";
    public const string Cosh = "cosh";
    public const string Degrees = "degrees";
    public const string Exp = "exp";
    public const string Floor = "floor";
    public const string Log = "log";
    public const string Log2 = "log2";
    public const string Log10 = "log10";
    public const string Pow = "pow";
    public const string Radians = "radians";
    public const string Sin = "sin";
    public const string Sinh = "sinh";
    public const string Sqrt = "sqrt";
    public const string Tan = "tan";
    public const string Tanh = "tanh";

    public static Dictionary<string, VMValue> Variables = new Dictionary<string, VMValue> {
      [E] = new VMValue(Math.E),
      [PI] = new VMValue(Math.PI),
      [Tau] = new VMValue(Math.Tau),

      [Acos] = new VMValue(new NativeFunction(Acos, AcosFunc)),
      [Acosh] = new VMValue(new NativeFunction(Acosh, AcoshFunc)),
      [Asin] = new VMValue(new NativeFunction(Asin, AsinFunc)),
      [Asinh] = new VMValue(new NativeFunction(Asinh, AsinhFunc)),
      [Atan] = new VMValue(new NativeFunction(Atan, AtanFunc)),
      [Atan2] = new VMValue(new NativeFunction(Atan2, Atan2Func)),
      [Atanh] = new VMValue(new NativeFunction(Atanh, AtanhFunc)),
      [Ceil] = new VMValue(new NativeFunction(Ceil, CeilFunc)),
      [Cos] = new VMValue(new NativeFunction(Cos, CosFunc)),
      [Cosh] = new VMValue(new NativeFunction(Cosh, CoshFunc)),
      [Degrees] = new VMValue(new NativeFunction(Degrees, DegreesFunc)),
      [Exp] = new VMValue(new NativeFunction(Exp, ExpFunc)),
      [Floor] = new VMValue(new NativeFunction(Floor, FloorFunc)),
      [Log] = new VMValue(new NativeFunction(Log, LogFunc)),
      [Log2] = new VMValue(new NativeFunction(Log2, Log2Func)),
      [Log10] = new VMValue(new NativeFunction(Log10, Log10Func)),
      [Pow] = new VMValue(new NativeFunction(Pow, PowFunc)),
      [Radians] = new VMValue(new NativeFunction(Radians, RadiansFunc)),
      [Sin] = new VMValue(new NativeFunction(Sin, SinFunc)),
      [Sinh] = new VMValue(new NativeFunction(Sinh, SinhFunc)),
      [Sqrt] = new VMValue(new NativeFunction(Sqrt, SqrtFunc)),
      [Tan] = new VMValue(new NativeFunction(Tan, TanFunc)),
      [Tanh] = new VMValue(new NativeFunction(Tanh, TanhFunc)),
    };

    private static VMValue AcosFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Acos);
    }

    private static VMValue AcoshFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Acosh);
    }

    private static VMValue AsinFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Asin);
    }

    private static VMValue AsinhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Asinh);
    }

    private static VMValue AtanFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Atan);
    }

    private static VMValue Atan2Func(ValueSpan args, INativeContext _) {
      return NativeFunc2(args, Math.Atan2);
    }

    private static VMValue AtanhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Atanh);
    }

    private static VMValue CeilFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Ceiling);
    }

    private static VMValue CosFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Cos);
    }

    private static VMValue CoshFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Cosh);
    }

    private static VMValue DegreesFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, radian => 180 / Math.PI * radian);
    }

    private static VMValue ExpFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Exp);
    }

    private static VMValue FloorFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Floor);
    }

    private static VMValue LogFunc(ValueSpan args, INativeContext _) {
      if (args.Count == 1) {
        return new VMValue(Math.Log(args[0].AsNumber()));
      } else if (args.Count == 2) {
        return new VMValue(Math.Log(args[0].AsNumber(), args[1].AsNumber()));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }

    private static VMValue Log2Func(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Log2);
    }

    private static VMValue Log10Func(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Log10);
    }

    private static VMValue PowFunc(ValueSpan args, INativeContext _) {
      return NativeFunc2(args, Math.Pow);
    }

    private static VMValue RadiansFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, degree => Math.PI / 180 * degree);
    }

    private static VMValue SinFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Sin);
    }

    private static VMValue SinhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Sinh);
    }

    private static VMValue SqrtFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Sqrt);
    }

    private static VMValue TanFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Tan);
    }

    private static VMValue TanhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Tanh);
    }

    private static VMValue NativeFunc1(ValueSpan args, Func<double, double> func) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(func(args[0].AsNumber()));
    }

    private static VMValue NativeFunc2(ValueSpan args, Func<double, double, double> func) {
      if (args.Count != 2) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(func(args[0].AsNumber(), args[1].AsNumber()));
    }
  }
}
