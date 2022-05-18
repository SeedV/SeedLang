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

namespace SeedLang.Runtime {
  using NativeFunction = HeapObject.NativeFunction;

  // The static class to define all the build-in native functions.
  internal static class NativeFunctions {
    public const string PrintVal = "__printval__";
    public const string Append = "append";
    public const string Len = "len";
    public const string List = "list";
    public const string Print = "print";
    public const string Range = "range";
    public const string Slice = "slice";

    public static NativeFunction[] Funcs = new NativeFunction[] {
        new NativeFunction(PrintVal, PrintValFunc),
        new NativeFunction(Append, AppendFunc),
        new NativeFunction(Len, LenFunc),
        new NativeFunction(List, ListFunc),
        new NativeFunction(Print, PrintFunc),
        new NativeFunction(Range, RangeFunc),
        new NativeFunction(Slice, SliceFunc),
    };

    internal static bool IsInternalFunction(string name) {
      return name.StartsWith("__") && name.EndsWith("__");
    }

    // Prints a value when it's not nil. It's used in interactive mode to print the result of an
    // expression statement.
    private static VMValue PrintValFunc(VMValue[] args, int offset, int length, Sys sys) {
      if (length != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (!args[offset].IsNil) {
        sys.Stdout.WriteLine(args[offset]);
      }
      return new VMValue();
    }

    // Appends a value to a list. The first argument is the list, the second argument is the
    // value to be appended to the list.
    private static VMValue AppendFunc(VMValue[] args, int offset, int length, Sys _) {
      if (length != 2) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      VMValue value = args[offset];
      if (value.IsList) {
        List<VMValue> list = value.AsList();
        list.Add(args[offset + 1]);
      }
      return new VMValue();
    }

    private static VMValue LenFunc(VMValue[] args, int offset, int length, Sys _) {
      if (length != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new VMValue(args[offset].Length);
    }

    // Creates an empty list if the length of arguments is empty, and a list if the argument is a
    // subscriptable value.
    private static VMValue ListFunc(VMValue[] args, int offset, int length, Sys _) {
      if (length < 0 || length > 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (length == 0) {
        return new VMValue(new List<VMValue>());
      }
      var list = new List<VMValue>();
      for (int i = 0; i < args[offset].Length; i++) {
        list.Add(args[offset][new VMValue(i)]);
      }
      return new VMValue(list);
    }

    private static VMValue PrintFunc(VMValue[] args, int offset, int length, Sys sys) {
      for (int i = 0; i < length; i++) {
        if (i > 0) {
          sys.Stdout.Write(" ");
        }
        sys.Stdout.Write(args[offset + i].AsString());
      }
      sys.Stdout.WriteLine();
      return new VMValue();
    }

    private static VMValue RangeFunc(VMValue[] args, int offset, int length, Sys _) {
      if (length == 1) {
        return new VMValue(new HeapObject.Range((int)args[offset].AsNumber()));
      } else if (length == 2) {
        var range = new HeapObject.Range((int)args[offset].AsNumber(),
                                         (int)args[offset + 1].AsNumber());
        return new VMValue(range);
      } else if (length == 3) {
        return new VMValue(new HeapObject.Range((int)args[offset].AsNumber(),
                                                (int)args[offset + 1].AsNumber(),
                                                (int)args[offset + 2].AsNumber()));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }

    private static VMValue SliceFunc(VMValue[] args, int offset, int length, Sys _) {
      if (length != 3) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      int? start = args[offset].IsNumber ? (int)args[offset].AsNumber() : null;
      int? stop = args[offset + 1].IsNumber ? (int)args[offset + 1].AsNumber() : null;
      int? step = args[offset + 2].IsNumber ? (int)args[offset + 2].AsNumber() : null;
      return new VMValue(new HeapObject.Slice(start, stop, step));
    }
  }
}
