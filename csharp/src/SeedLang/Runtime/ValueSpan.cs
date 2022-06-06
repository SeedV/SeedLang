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

using System.Diagnostics;

namespace SeedLang.Runtime {
  internal class ValueSpan {
    public int Count { get; }

    private readonly VMValue[] _values;
    private readonly int _start;

    internal ValueSpan(VMValue[] values, int start, int count) {
      _values = values;
      _start = start;
      Count = count;
    }

    internal ref readonly VMValue this[int index] {
      get {
        Debug.Assert(index < Count && _start + index < _values.Length);
        return ref _values[_start + index];
      }
    }
  }
}
