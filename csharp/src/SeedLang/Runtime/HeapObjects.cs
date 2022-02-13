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

namespace SeedLang.Runtime {
  using NativeFunctionType = Func<Value[], int, int, Value>;

  internal partial class HeapObject {
    // The function interface of function value types.
    internal interface IFunction {
      // Calls the function with given arguments that locate in the "args" array starting from
      // "offset". The number of arguments is "length".
      Value Call(Value[] args, int offset, int length);
    }

    // The native function class to encapsulate build-in functions written by the host language.
    internal class NativeFunction : IFunction {
      public readonly string Name;

      private readonly NativeFunctionType _func;

      internal NativeFunction(string name, NativeFunctionType func) {
        Name = name;
        _func = func;
      }

      // Calls the function with given arguments that locate in the "args" array starting from
      // "offset". The number of arguments is "length".
      public Value Call(Value[] args, int offset, int length) {
        return _func(args, offset, length);
      }

      public override string ToString() {
        return $"NativeFunction <{Name}>";
      }
    }

    internal class Range {
      public int Length {
        get {
          int distance = _stop - _start;
          int length = distance / _step + (distance % _step == 0 ? 0 : 1);
          return Math.Max(length, 0);
        }
      }

      private readonly int _start;
      private readonly int _stop;
      private readonly int _step;

      internal Range(int stop) : this(0, stop) { }

      internal Range(int start, int stop, int step = 1) {
        _start = start;
        _stop = stop;
        _step = step;
      }

      public override string ToString() {
        return $"range({_start}, {_stop}, {_step})";
      }

      internal Value this[double index] {
        get {
          return new Value(_start + (int)index * _step);
        }
      }
    }
  }
}
