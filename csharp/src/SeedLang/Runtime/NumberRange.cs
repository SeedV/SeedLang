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
  // A class to store the range information of "for in range" statements.
  internal class NumberRange {
    private readonly int _start;
    private readonly int _stop;
    private readonly int _step;

    internal NumberRange(int stop) : this(0, stop) { }

    internal NumberRange(int start, int stop, int step = 1) {
      _start = start;
      _stop = stop;
      _step = step;
    }

    public override string ToString() {
      return $"range({_start}, {_stop}, {_step})";
    }

    internal int Length() {
      int distance = _stop - _start;
      int length = distance / _step + (distance % _step == 0 ? 0 : 1);
      return Math.Max(length, 0);
    }

    internal Value this[double index] {
      get {
        return new Value(_start + (int)index * _step);
      }
    }
  }
}
