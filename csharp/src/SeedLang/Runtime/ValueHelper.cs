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

using System;

namespace SeedLang.Runtime {
  internal static class ValueHelper {
    internal static double Add<V>(V lhs, V rhs) where V : IValue {
      return lhs.Number + rhs.Number;
    }

    internal static double Subtract<V>(V lhs, V rhs) where V : IValue {
      return lhs.Number - rhs.Number;
    }

    internal static double Multiply<V>(V lhs, V rhs) where V : IValue {
      return lhs.Number * rhs.Number;
    }

    internal static double Divide<V>(V lhs, V rhs) where V : IValue {
      return lhs.Number / rhs.Number;
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
      } catch (Exception) {
        return 0;
      }
    }
  }
}
