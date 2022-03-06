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

    public static NativeFunction[] Funcs = new NativeFunction[] {
        new NativeFunction(PrintVal, PrintValFunc),
        new NativeFunction(Append, AppendFunc),
        new NativeFunction(Len, LenFunc),
        new NativeFunction(List, ListFunc),
        new NativeFunction(Print, PrintFunc),
        new NativeFunction(Range, RangeFunc),
    };

    // Prints a value when it's not none. It's used in interactive mode to print the result of an
    // expression statement.
    private static Value PrintValFunc(Value[] args, int offset, int length, Sys sys) {
      if (length != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (!args[offset].IsNone) {
        if (args[offset].IsString) {
          sys.Stdout.WriteLine($"'{args[offset].AsString()}'");
        } else {
          sys.Stdout.WriteLine(args[offset].AsString());
        }
      }
      return new Value();
    }

    // Appends a value to a list. The first argument is the list, the second argument is the
    // value to be appended to the list.
    private static Value AppendFunc(Value[] args, int offset, int length, Sys _) {
      if (length != 2) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      Value value = args[offset];
      if (value.IsList) {
        List<Value> list = value.AsList();
        list.Add(args[offset + 1]);
      }
      return new Value();
    }

    private static Value LenFunc(Value[] args, int offset, int length, Sys _) {
      if (length != 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      return new Value(args[offset].Length);
    }

    // Creates an empty list if the length of arguments is empty, and a list if the argument is a
    // subscriptable value.
    private static Value ListFunc(Value[] args, int offset, int length, Sys _) {
      if (length < 0 || length > 1) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      if (length == 0) {
        return new Value(new List<Value>());
      }
      if (args[offset].IsList) {
        return args[offset];
      }
      var list = new List<Value>();
      for (int i = 0; i < args[offset].Length; i++) {
        list.Add(args[offset][i]);
      }
      return new Value(list);
    }

    private static Value PrintFunc(Value[] args, int offset, int length, Sys sys) {
      for (int i = 0; i < length; i++) {
        sys.Stdout.WriteLine(args[offset + i].AsString());
      }
      return new Value();
    }

    private static Value RangeFunc(Value[] args, int offset, int length, Sys _) {
      if (length == 1) {
        return new Value(new HeapObject.Range((int)args[offset].AsNumber()));
      } else if (length == 2) {
        var range = new HeapObject.Range((int)args[offset].AsNumber(),
                                         (int)args[offset + 1].AsNumber());
        return new Value(range);
      } else if (length == 3) {
        return new Value(new HeapObject.Range((int)args[offset].AsNumber(),
                                              (int)args[offset + 1].AsNumber(),
                                              (int)args[offset + 2].AsNumber()));
      }
      throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                    Message.RuntimeErrorIncorrectArgsCount);
    }
  }
}
