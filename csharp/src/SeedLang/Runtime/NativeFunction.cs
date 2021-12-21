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
  internal class NativeFunction : IFunction {
    public readonly string Name;
    private readonly Func<Value[], Value> _func;

    internal NativeFunction(string name, Func<Value[], Value> func) {
      Name = name;
      _func = func;
    }

    public Value Call(Value[] parameters) {
      return _func(parameters);
    }

    public override string ToString() {
      return $"NativeFunction <{Name}>";
    }
  }

  // Defines all the native functions.
  // TODO: move this class to ...
  internal static class NativeFunctions {
    public static NativeFunction[] Funcs = new NativeFunction[] {
      new NativeFunction("len", (Value[] parameters) => {
        if (parameters.Length != 1) {
          // TODO: throw parameters count not correct runtime exception.
          throw new NotImplementedException();
        }
        return Value.Number(parameters[0].Count());
      }),
    };
  }
}
