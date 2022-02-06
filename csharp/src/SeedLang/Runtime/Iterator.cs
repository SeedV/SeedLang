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
using System.Diagnostics;

namespace SeedLang.Runtime {
  internal interface IIterator {
    bool HasNext();
    Value Next();
  }

  internal class ListIterator : IIterator {
    private readonly IReadOnlyList<Value> _list;
    private int _nextPos = 0;

    internal ListIterator(IReadOnlyList<Value> list) {
      _list = list;
    }

    public bool HasNext() {
      return _nextPos < _list.Count;
    }

    public Value Next() {
      Debug.Assert(_nextPos >= 0 && _nextPos < _list.Count);
      return _list[_nextPos++];
    }
  }
}
