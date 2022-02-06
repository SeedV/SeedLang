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
  using NativeFunctionType = Func<IList<Value>, Value>;

  // The native function class to encapsulate build-in functions written by the host language.
  internal class NativeFunction : IFunction {
    public readonly string Name;

    private readonly NativeFunctionType _func;

    internal NativeFunction(string name, NativeFunctionType func) {
      Name = name;
      _func = func;
    }

    public Value Call(IList<Value> parameters) {
      return _func(parameters);
    }

    public override string ToString() {
      return $"NativeFunction <{Name}>";
    }
  }

  // The static class to define all the build-in native functions.
  internal static class NativeFunctions {
    public const string List = "list";
    public const string Len = "len";
    public const string Iter = "__iter__";
    public const string HasNext = "__has_next__";
    public const string Next = "__next__";

    public static NativeFunction[] Funcs = new NativeFunction[] {
      new NativeFunction(List, (IList<Value> arguments) => {
        return new Value(new List<Value>(arguments));
      }),

      new NativeFunction(Len, (IList<Value> arguments) => {
        if (arguments.Count != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return new Value(arguments[0].Count());
      }),

      new NativeFunction(Iter, (IList<Value> arguments) => {
        if (arguments.Count != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        if (!arguments[0].IsList) {
          // TODO: throw uniterable...
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return new Value(new ListIterator(arguments[0].AsList()));
      }),

      new NativeFunction(HasNext, (IList<Value> arguments) => {
        if (arguments.Count != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        if (!arguments[0].IsIterator) {
          // TODO: throw uniterable...
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return new Value(arguments[0].AsIterator().HasNext());
      }),

      new NativeFunction(Next, (IList<Value> arguments) => {
        if (arguments.Count != 1) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        if (!arguments[0].IsIterator) {
          // TODO: throw uniterable...
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorIncorrectArgsCount);
        }
        return arguments[0].AsIterator().Next();
      }),
    };
  }
}
