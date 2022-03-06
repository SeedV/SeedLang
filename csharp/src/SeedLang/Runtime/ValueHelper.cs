using System.Text;
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

using System.Collections.Generic;
using System.Diagnostics;
using SeedLang.Common;

namespace SeedLang.Runtime {
  // A helper class to do value operations.
  internal static class ValueHelper {
    private static readonly Dictionary<char, char> _escapeCharacters =
        new Dictionary<char, char>() {
          ['\''] = '\'',
          ['"'] = '"',
          ['\\'] = '\\',
          ['n'] = '\n',
          ['r'] = '\r',
          ['t'] = '\t',
          ['b'] = '\b',
          ['f'] = '\f',
        };

    internal static Value Add(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = lhs.AsNumber() + rhs.AsNumber();
        CheckOverflow(result);
        return new Value(result);
      } else if (lhs.IsString && rhs.IsString) {
        return new Value(lhs.AsString() + rhs.AsString());
      } else if (lhs.IsList && rhs.IsList) {
        var list = new List<Value>(lhs.AsList());
        list.AddRange(rhs.AsList());
        return new Value(list);
      } else if (lhs.IsTuple && rhs.IsTuple) {
        var list = new List<Value>(lhs.AsTuple());
        list.AddRange(rhs.AsTuple());
        return new Value(list.ToArray());
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
    }

    internal static Value Subtract(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = lhs.AsNumber() - rhs.AsNumber();
        CheckOverflow(result);
        return new Value(result);
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
    }

    internal static Value Multiply(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = lhs.AsNumber() * rhs.AsNumber();
        CheckOverflow(result);
        return new Value(result);
      } else if (lhs.IsString && (rhs.IsBoolean || rhs.IsNumber) ||
                 (lhs.IsBoolean || lhs.IsNumber) && rhs.IsString) {
        double count = lhs.IsString ? rhs.AsNumber() : lhs.AsNumber();
        string str = lhs.IsString ? lhs.AsString() : rhs.AsString();
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) {
          sb.Append(str);
        }
        return new Value(sb.ToString());
      } else if (lhs.IsList && (rhs.IsBoolean || rhs.IsNumber) ||
                 (lhs.IsBoolean || lhs.IsNumber) && rhs.IsList) {
        double count = lhs.IsList ? rhs.AsNumber() : lhs.AsNumber();
        IReadOnlyList<Value> list = lhs.IsList ? lhs.AsList() : rhs.AsList();
        var newList = new List<Value>();
        for (int i = 0; i < count; i++) {
          newList.AddRange(list);
        }
        return new Value(newList);
      } else if (lhs.IsTuple && (rhs.IsBoolean || rhs.IsNumber) ||
                 (lhs.IsBoolean || lhs.IsNumber) && rhs.IsTuple) {
        double count = lhs.IsTuple ? rhs.AsNumber() : lhs.AsNumber();
        IReadOnlyList<Value> tuple = lhs.IsTuple ? lhs.AsTuple() : rhs.AsTuple();
        var newTuple = new List<Value>();
        for (int i = 0; i < count; i++) {
          newTuple.AddRange(tuple);
        }
        return new Value(newTuple.ToArray());
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
    }

    internal static Value Divide(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        if (rhs.AsNumber() == 0) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                        Message.RuntimeErrorDivideByZero);
        }
        double result = lhs.AsNumber() / rhs.AsNumber();
        CheckOverflow(result);
        return new Value(result);
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
    }

    internal static Value FloorDivide(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        if (rhs.AsNumber() == 0) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                        Message.RuntimeErrorDivideByZero);
        }
        double result = System.Math.Floor(lhs.AsNumber() / rhs.AsNumber());
        CheckOverflow(result);
        return new Value(result);
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
    }

    internal static Value Power(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = System.Math.Pow(lhs.AsNumber(), rhs.AsNumber());
        CheckOverflow(result);
        return new Value(result);
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
    }

    internal static Value Modulo(in Value lhs, in Value rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        if (rhs.AsNumber() == 0) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                        Message.RuntimeErrorDivideByZero);
        }
        double result = lhs.AsNumber() % rhs.AsNumber();
        CheckOverflow(result);
        return new Value(result);
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                      Message.RuntimeErrorUnsupportedOperads);
      }
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
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", range,
                                      Message.RuntimeErrorOverflow);
      }
    }

    internal static string Unescape(string str) {
      var sb = new StringBuilder();
      int i = 0;
      while (i < str.Length) {
        if (str[i] == '\\') {
          i++;
          if (i < str.Length && _escapeCharacters.ContainsKey(str[i])) {
            sb.Append(_escapeCharacters[str[i]]);
          }
        } else {
          sb.Append(str[i]);
        }
        i++;
      }
      return sb.ToString();
    }
  }
}
