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
  // The static class to define all constants and functions for the builtin math library.
  internal static class MathDefinition {
    public const string E = "e";
    public const string PI = "pi";

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
    public const string Log10 = "log10";
    public const string Pow = "pow";
    public const string Radians = "radians";
    public const string Sin = "sin";
    public const string Sinh = "sinh";
    public const string Sqrt = "sqrt";
    public const string Tan = "tan";
    public const string Tanh = "tanh";

    internal static readonly VMValue _e = new VMValue(Math.E);
    internal static readonly VMValue _pi = new VMValue(Math.PI);

    internal static VMValue AcosFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Acos);
    }

    internal static VMValue AcoshFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Acosh);
    }

    internal static VMValue AsinFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Asin);
    }

    internal static VMValue AsinhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Asinh);
    }

    internal static VMValue AtanFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Atan);
    }

    internal static VMValue Atan2Func(ValueSpan args, INativeContext _) {
      return NativeFunc2(args, Math.Atan2);
    }

    internal static VMValue AtanhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Atanh);
    }

    internal static VMValue CeilFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Ceiling);
    }

    internal static VMValue CosFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Cos);
    }

    internal static VMValue CoshFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Cosh);
    }

    internal static VMValue DegreesFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, radian => 180 / Math.PI * radian);
    }

    internal static VMValue ExpFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Exp);
    }

    internal static VMValue FloorFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Floor);
    }

    internal static VMValue LogFunc(ValueSpan args, INativeContext _) {
      if (args.Count == 1) {
        return new VMValue(Math.Log(args[0].AsNumber()));
      } else if (args.Count == 2) {
        return new VMValue(Math.Log(args[0].AsNumber(), args[1].AsNumber()));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }

    internal static VMValue Log10Func(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Log10);
    }

    internal static VMValue PowFunc(ValueSpan args, INativeContext _) {
      return NativeFunc2(args, Math.Pow);
    }

    internal static VMValue RadiansFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, degree => Math.PI / 180 * degree);
    }

    internal static VMValue SinFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Sin);
    }

    internal static VMValue SinhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Sinh);
    }

    internal static VMValue SqrtFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Sqrt);
    }

    internal static VMValue TanFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Tan);
    }

    internal static VMValue TanhFunc(ValueSpan args, INativeContext _) {
      return NativeFunc1(args, Math.Tanh);
    }

    internal static VMValue NativeFunc1(ValueSpan args, Func<double, double> func) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(func(args[0].AsNumber()));
    }

    internal static VMValue NativeFunc2(ValueSpan args, Func<double, double, double> func) {
      if (args.Count != 2) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(func(args[0].AsNumber(), args[1].AsNumber()));
    }
  }
}
