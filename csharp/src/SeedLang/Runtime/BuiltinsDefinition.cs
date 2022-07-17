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
  using Range = HeapObjects.Range;

  // The static class to define all the build-in native functions.
  internal static class BuiltinsDefinition {
    public const string PrintVal = "__printval__";
    public const string Abs = "abs";
    public const string All = "all";
    public const string Any = "any";
    public const string Append = "append";
    public const string Dir = "dir";
    public const string Len = "len";
    public const string List = "list";
    public const string Print = "print";
    public const string Range = "range";
    public const string Round = "round";
    public const string Slice = "slice";
    public const string Sum = "sum";

    public static Dictionary<string, VMValue> Variables = new Dictionary<string, VMValue> {
      [PrintVal] = new VMValue(new NativeFunction(PrintVal, PrintValFunc)),
      [Abs] = new VMValue(new NativeFunction(Abs, AbsFunc)),
      [All] = new VMValue(new NativeFunction(All, AllFunc)),
      [Any] = new VMValue(new NativeFunction(Any, AnyFunc)),
      [Append] = new VMValue(new NativeFunction(Append, AppendFunc)),
      [Dir] = new VMValue(new NativeFunction(Dir, DirFunc)),
      [Len] = new VMValue(new NativeFunction(Len, LenFunc)),
      [List] = new VMValue(new NativeFunction(List, ListFunc)),
      [Print] = new VMValue(new NativeFunction(Print, PrintFunc)),
      [Range] = new VMValue(new NativeFunction(Range, RangeFunc)),
      [Round] = new VMValue(new NativeFunction(Round, RoundFunc)),
      [Slice] = new VMValue(new NativeFunction(Slice, SliceFunc)),
      [Sum] = new VMValue(new NativeFunction(Sum, SumFunc)),
    };

    internal static bool IsNativeFunc(string name) {
      return Variables.ContainsKey(name);
    }

    // Prints a value when it's not nil. It's used in interactive mode to print the result of an
    // expression statement.
    private static VMValue PrintValFunc(ValueSpan args, INativeContext context) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (!args[0].IsNil) {
        context.Stdout.WriteLine(args[0]);
      }
      return new VMValue();
    }

    private static VMValue AbsFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(Math.Abs(args[0].AsNumber()));
    }

    private static VMValue AllFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      for (int i = 0; i < args[0].Length; i++) {
        if (!args[0][new VMValue(i)].AsBoolean()) {
          return new VMValue(false);
        }
      }
      return new VMValue(true);
    }

    private static VMValue AnyFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      for (int i = 0; i < args[0].Length; i++) {
        if (args[0][new VMValue(i)].AsBoolean()) {
          return new VMValue(true);
        }
      }
      return new VMValue(false);
    }

    // Appends a value to a list. The first argument is the list, the second argument is the
    // value to be appended to the list.
    private static VMValue AppendFunc(ValueSpan args, INativeContext _) {
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

    private static VMValue DirFunc(ValueSpan args, INativeContext context) {
      if (args.Count == 0) {
        return context.Dir();
      } else if (args.Count == 1) {
        return context.Dir(args[0]);
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }

    private static VMValue LenFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(args[0].Length);
    }

    // Creates an empty list if the length of arguments is empty, and a list if the argument is a
    // subscriptable value.
    private static VMValue ListFunc(ValueSpan args, INativeContext _) {
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

    private static VMValue PrintFunc(ValueSpan args, INativeContext context) {
      for (int i = 0; i < args.Count; i++) {
        if (i > 0) {
          context.Stdout.Write(" ");
        }
        context.Stdout.Write(args[i].AsString());
      }
      context.Stdout.WriteLine();
      return new VMValue();
    }

    private static VMValue RangeFunc(ValueSpan args, INativeContext _) {
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

    private static VMValue RoundFunc(ValueSpan args, INativeContext _) {
      if (args.Count == 1) {
        return new VMValue(Math.Round(args[0].AsNumber()));
      } else if (args.Count == 2) {
        var digits = (int)args[1].AsNumber();
        if (digits != args[1].AsNumber()) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidInteger);
        }
        return new VMValue(Math.Round(args[0].AsNumber(), digits));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }

    private static VMValue SliceFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 3) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(new Slice(args[0], args[1], args[2]));
    }

    private static VMValue SumFunc(ValueSpan args, INativeContext _) {
      if (args.Count != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      double sum = 0;
      for (int i = 0; i < args[0].Length; i++) {
        sum += args[0][new VMValue(i)].AsNumber();
      }
      return new VMValue(sum);
    }
  }
}
