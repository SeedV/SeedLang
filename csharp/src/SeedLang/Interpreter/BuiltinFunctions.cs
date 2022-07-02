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
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;

namespace SeedLang.Interpreter {
  // The static class to define all the build-in native functions.
  internal static class BuiltinFunctions {
    public const string PrintVal = "__printval__";
    public const string Append = "append";
    public const string Len = "len";
    public const string List = "list";
    public const string Print = "print";
    public const string Range = "range";
    public const string Slice = "slice";

    public static Dictionary<string, NativeFunction> Funcs =
        new Dictionary<string, NativeFunction> {
          [PrintVal] = new NativeFunction(PrintVal, PrintValFunc),
          [Append] = new NativeFunction(Append, AppendFunc),
          [Len] = new NativeFunction(Len, LenFunc),
          [List] = new NativeFunction(List, ListFunc),
          [Print] = new NativeFunction(Print, PrintFunc),
          [Range] = new NativeFunction(Range, RangeFunc),
          [Slice] = new NativeFunction(Slice, SliceFunc),
        };

    internal static bool IsNativeFunc(string name) {
      return Funcs.ContainsKey(name);
    }

    internal static bool IsInternalFunction(string name) {
      return name.StartsWith("__") && name.EndsWith("__");
    }

    // Prints a value when it's not nil. It's used in interactive mode to print the result of an
    // expression statement.
    private static VMValue PrintValFunc(ValueSpan args, Sys sys) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (!args[0].IsNil) {
        sys.Stdout.WriteLine(args[0]);
      }
      return new VMValue();
    }

    // Appends a value to a list. The first argument is the list, the second argument is the
    // value to be appended to the list.
    private static VMValue AppendFunc(ValueSpan args, Sys _) {
      if (args.Count != 2) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      VMValue value = args[0];
      if (value.IsList) {
        List<VMValue> list = value.AsList();
        list.Add(args[1]);
      }
      return new VMValue();
    }

    private static VMValue LenFunc(ValueSpan args, Sys _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(args[0].Length);
    }

    // Creates an empty list if the length of arguments is empty, and a list if the argument is a
    // subscriptable value.
    private static VMValue ListFunc(ValueSpan args, Sys _) {
      if (args.Count < 0 || args.Count > 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (args.Count == 0) {
        return new VMValue(new List<VMValue>());
      }
      var list = new List<VMValue>();
      for (int i = 0; i < args[0].Length; i++) {
        list.Add(args[0][new VMValue(i)]);
      }
      return new VMValue(list);
    }

    private static VMValue PrintFunc(ValueSpan args, Sys sys) {
      for (int i = 0; i < args.Count; i++) {
        if (i > 0) {
          sys.Stdout.Write(" ");
        }
        sys.Stdout.Write(args[i].AsString());
      }
      sys.Stdout.WriteLine();
      return new VMValue();
    }

    private static VMValue RangeFunc(ValueSpan args, Sys _) {
      if (args.Count == 1) {
        return new VMValue(new Range((int)args[0].AsNumber()));
      } else if (args.Count == 2) {
        var range = new Range((int)args[0].AsNumber(), (int)args[1].AsNumber());
        return new VMValue(range);
      } else if (args.Count == 3) {
        return new VMValue(new Range((int)args[0].AsNumber(),
                                     (int)args[1].AsNumber(),
                                     (int)args[2].AsNumber()));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }

    private static VMValue SliceFunc(ValueSpan args, Sys _) {
      if (args.Count != 3) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(new Slice(args[0], args[1], args[2]));
    }
  }
}
