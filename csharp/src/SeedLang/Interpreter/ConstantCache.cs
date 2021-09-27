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
using System.Collections.Generic;

namespace SeedLang.Interpreter {
  // A cache class to cache the constant id of constants. It only adds the unique constant into the
  // constant list of the chunk.
  internal class ConstantCache {
    private readonly Chunk _chunk;
    private readonly Dictionary<double, uint> _numbers = new Dictionary<double, uint>();

    internal ConstantCache(Chunk chunk) {
      _chunk = chunk;
    }

    // Returns the id of the given number constant and adds the constant into the constant list of
    // the chunk if it is not in the constants list.
    internal uint IdOfConstant(double number) {
      if (!_numbers.ContainsKey(number)) {
        _numbers[number] = _chunk.AddConstant(number);
      }
      return _numbers[number];
    }

    internal uint IdOfConstant(string str) {
      throw new NotImplementedException();
    }
  }
}
