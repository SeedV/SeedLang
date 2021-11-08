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

using SeedLang.Common;

namespace SeedLang.Runtime {
  internal static class ValueHelper {
    internal static double Add<V>(V lhs, V rhs) where V : IValue {
      double result = lhs.Number + rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double Subtract<V>(V lhs, V rhs) where V : IValue {
      double result = lhs.Number - rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double Multiply<V>(V lhs, V rhs) where V : IValue {
      double result = lhs.Number * rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double Divide<V>(V lhs, V rhs) where V : IValue {
      if (rhs.Number == 0) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorDivideByZero);
      }
      double result = lhs.Number / rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double BooleanToNumber(bool value) {
      return value ? 1 : 0;
    }

    internal static string BooleanToString(bool value) {
      return $"{value}";
    }

    internal static bool NumberToBoolean(double value) {
      return value != 0;
    }

    internal static string NumberToString(double value) {
      return $"{value}";
    }

    internal static bool StringToBoolean(string value) {
      return value == "True";
    }

    internal static double StringToNumber(string value) {
      try {
        return double.Parse(value);
      } catch (System.Exception) {
        return 0;
      }
    }

    internal static void CheckOverflow(double value) {
      // TODO: do we need separate NaN as another runtime error?
      if (double.IsInfinity(value) || double.IsNaN(value)) {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Error, "", null,
                                      Message.RuntimeOverflow);
      }
    }
  }
}
