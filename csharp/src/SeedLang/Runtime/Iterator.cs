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
  internal class Iterator {
    private readonly Value _sequence;
    private int _nextPos = 0;

    internal Iterator(in Value sequence) {
      _sequence = sequence;
    }

    public bool HasNext() {
      return _nextPos < _sequence.Length();
    }

    public Value Next() {
      Debug.Assert(_nextPos >= 0 && _nextPos < _sequence.Length());
      return _sequence[_nextPos++];
    }
  }
}
