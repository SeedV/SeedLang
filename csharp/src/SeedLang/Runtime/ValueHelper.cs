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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
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

    internal static VMValue Add(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = lhs.AsNumber() + rhs.AsNumber();
        CheckOverflow(result);
        return new VMValue(result);
      } else if (lhs.IsString && rhs.IsString) {
        return new VMValue(lhs.AsString() + rhs.AsString());
      } else if (lhs.IsList && rhs.IsList) {
        var list = lhs.AsList();
        list.AddRange(rhs.AsList());
        return new VMValue(list);
      } else if (lhs.IsTuple && rhs.IsTuple) {
        return new VMValue(lhs.AsTuple().AddRange(rhs.AsTuple()));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static VMValue Subtract(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = lhs.AsNumber() - rhs.AsNumber();
        CheckOverflow(result);
        return new VMValue(result);
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static VMValue Multiply(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = lhs.AsNumber() * rhs.AsNumber();
        CheckOverflow(result);
        return new VMValue(result);
      } else if (lhs.IsString && (rhs.IsBoolean || rhs.IsNumber) ||
                 (lhs.IsBoolean || lhs.IsNumber) && rhs.IsString) {
        double count = lhs.IsString ? rhs.AsNumber() : lhs.AsNumber();
        string str = lhs.IsString ? lhs.AsString() : rhs.AsString();
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) {
          sb.Append(str);
        }
        return new VMValue(sb.ToString());
      } else if (lhs.IsList && (rhs.IsBoolean || rhs.IsNumber) ||
                 (lhs.IsBoolean || lhs.IsNumber) && rhs.IsList) {
        double count = lhs.IsList ? rhs.AsNumber() : lhs.AsNumber();
        IReadOnlyList<VMValue> list = lhs.IsList ? lhs.AsList() : rhs.AsList();
        var newList = new List<VMValue>();
        for (int i = 0; i < count; i++) {
          newList.AddRange(list);
        }
        return new VMValue(newList);
      } else if (lhs.IsTuple && (rhs.IsBoolean || rhs.IsNumber) ||
                 (lhs.IsBoolean || lhs.IsNumber) && rhs.IsTuple) {
        int count = lhs.IsTuple ? (int)rhs.AsNumber() : (int)lhs.AsNumber();
        if (count <= 0) {
          return new VMValue(ImmutableArray.Create<VMValue>());
        }
        ImmutableArray<VMValue> tuple = lhs.IsTuple ? lhs.AsTuple() : rhs.AsTuple();
        var builder = ImmutableArray.CreateBuilder<VMValue>(tuple.Length * count);
        for (int i = 0; i < count; i++) {
          builder.AddRange(tuple);
        }
        return new VMValue(builder.MoveToImmutable());
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static VMValue Divide(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        if (rhs.AsNumber() == 0) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                        Message.RuntimeErrorDivideByZero);
        }
        double result = lhs.AsNumber() / rhs.AsNumber();
        CheckOverflow(result);
        return new VMValue(result);
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static VMValue FloorDivide(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        if (rhs.AsNumber() == 0) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                        Message.RuntimeErrorDivideByZero);
        }
        double result = System.Math.Floor(lhs.AsNumber() / rhs.AsNumber());
        CheckOverflow(result);
        return new VMValue(result);
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static VMValue Power(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        double result = System.Math.Pow(lhs.AsNumber(), rhs.AsNumber());
        CheckOverflow(result);
        return new VMValue(result);
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static VMValue Modulo(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        if (rhs.AsNumber() == 0) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                        Message.RuntimeErrorDivideByZero);
        }
        double result = lhs.AsNumber() % rhs.AsNumber();
        CheckOverflow(result);
        return new VMValue(result);
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static bool Less(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        return lhs.AsNumber() < rhs.AsNumber();
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static bool LessEqual(in VMValue lhs, in VMValue rhs) {
      if ((lhs.IsBoolean || lhs.IsNumber) && (rhs.IsBoolean || rhs.IsNumber)) {
        return lhs.AsNumber() <= rhs.AsNumber();
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
    }

    internal static bool Contains(in VMValue container, in VMValue value) {
      if (container.IsDict) {
        return container.AsDict().ContainsKey(value);
      } else if (container.IsTuple) {
        return container.AsTuple().Contains(value);
      } else if (container.IsList) {
        return container.AsList().Contains(value);
      } else if (container.IsString && value.IsString) {
        return container.AsString().Contains(value.AsString());
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Error, "", null,
                                    Message.RuntimeErrorUnsupportedOperands);
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

    internal static void CheckOverflow(double value, TextRange range = null) {
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
