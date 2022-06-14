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
using SeedLang.Common;

namespace SeedLang.Runtime.HeapObjects {
  using BuildInFunctionType = Func<ValueSpan, Sys, VMValue>;

  // An empty interface for all function value types. It's only used to identify function types.
  internal interface IFunction {
  }

  // The native function class to encapsulate build-in functions written by the host language.
  internal class NativeFunction : IFunction {
    public readonly string Name;

    private readonly BuildInFunctionType _func;

    internal NativeFunction(string name, BuildInFunctionType func) {
      Name = name;
      _func = func;
    }

    // Calls the build-in function with given arguments that locate in the "args" array starting
    // from "offset". The number of arguments is "length".
    internal VMValue Call(ValueSpan args, Sys sys) {
      return _func(args, sys);
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

    internal VMValue this[double index] {
      get {
        return new VMValue(_start + (int)index * _step);
      }
    }
  }

  internal class Slice {
    public readonly int? Start;
    public readonly int? Stop;
    public readonly int? Step;

    internal Slice(double? stop = null) : this(null, stop) { }

    internal Slice(double? start, double? stop, double? step = null) {
      Start = ToInt(start);
      Stop = ToInt(stop);
      Step = ToInt(step);
    }

    internal Slice(in VMValue start, in VMValue stop, in VMValue step) :
        this(ToNumber(start), ToNumber(stop), ToNumber(step)) { }

    public override string ToString() {
      return $"slice({Start?.ToString() ?? "None"}, {Stop?.ToString() ?? "None"}, " +
             $"{Step?.ToString() ?? "None"})";
    }

    private static int? ToInt(double? value) {
      if (value is double doubleValue) {
        if ((int)doubleValue == doubleValue) {
          return (int)doubleValue;
        } else {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidIntIndex);
        }
      }
      return null;
    }

    private static double? ToNumber(VMValue value) {
      return value.IsNumber ? value.AsNumber() : default(double?);
    }
  }
}

