using System.Diagnostics;
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
  // A helper class to do value operations. The generic parameter Value could be IValue or VMValue.
  internal static class ValueHelper {
    internal static double Add<Value>(in Value lhs, in Value rhs) where Value : IValue {
      double result = lhs.Number + rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double Subtract<Value>(in Value lhs, in Value rhs) where Value : IValue {
      double result = lhs.Number - rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double Multiply<Value>(in Value lhs, in Value rhs) where Value : IValue {
      double result = lhs.Number * rhs.Number;
      CheckOverflow(result);
      return result;
    }

    internal static double Divide<Value>(in Value lhs, in Value rhs) where Value : IValue {
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
      Debug.Assert(!(value is null));
      return value != "";
    }

    internal static double StringToNumber(string value) {
      try {
        return double.Parse(value);
      } catch (System.Exception) {
        return 0;
      }
    }

    internal static void CheckOverflow(double value, Range range = null) {
      // TODO: do we need separate NaN as another runtime error?
      if (double.IsInfinity(value) || double.IsNaN(value)) {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Error, "", range,
                                      Message.RuntimeOverflow);
      }
    }
  }
}