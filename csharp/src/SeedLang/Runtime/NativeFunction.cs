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

namespace SeedLang.Runtime {
  using NativeFunctionType = Func<Value[], int, int, Value>;

  // The native function class to encapsulate build-in functions written by the host language.
  internal class NativeFunction : IFunction {
    public readonly string Name;

    private readonly NativeFunctionType _func;

    internal NativeFunction(string name, NativeFunctionType func) {
      Name = name;
      _func = func;
    }

    public Value Call(Value[] args, int offset, int length) {
      return _func(args, offset, length);
    }

    public override string ToString() {
      return $"NativeFunction <{Name}>";
    }
  }

  // The static class to define all the build-in native functions.
  internal static class NativeFunctions {
    public const string HasNext = "hasnext";
    public const string Iter = "iter";
    public const string Len = "len";
    public const string List = "list";
    public const string Next = "next";
    public const string Range = "range";

    public static NativeFunction[] Funcs = new NativeFunction[] {
      new NativeFunction(HasNext, (Value[] args, int offset, int length) => {
        if (length != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        if (!args[offset].IsIterator) {
          // TODO: throw uniterable...
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return new Value(args[offset].AsIterator().HasNext());
      }),

      new NativeFunction(Iter, (Value[] args, int offset, int length) => {
        if (length != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return new Value(new Iterator(args[offset]));
      }),

      new NativeFunction(Len, (Value[] args, int offset, int length) => {
        if (length != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return new Value(args[offset].Length());
      }),

      new NativeFunction(List, (Value[] args, int offset, int length) => {
        var list = new List<Value>();
        for (int i = 0; i < length; i++) {
          list.Add(args[offset + i]);
        }
        return new Value(list);
      }),

      new NativeFunction(Next, (Value[] args, int offset, int length) => {
        if (length != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        if (!args[offset].IsIterator) {
          // TODO: throw uniterable...
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return args[offset].AsIterator().Next();
      }),

      new NativeFunction(Range, (Value[] args, int offset, int length) => {
        if (length == 1) {
          return new Value(new NumberRange((int)args[offset].AsNumber()));
        } else if (length == 2) {
          return new Value(new NumberRange((int)args[offset].AsNumber(),
                                           (int)args[offset + 1].AsNumber()));
        } else if (length == 3) {
          return new Value(new NumberRange((int)args[offset].AsNumber(),
                                           (int)args[offset + 1].AsNumber(),
                                           (int)args[offset + 2].AsNumber()));
        }
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }),
    };
  }
}
