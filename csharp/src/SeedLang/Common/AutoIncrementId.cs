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

using System.Threading;

namespace SeedLang.Common {
  // Generates an auto-increment ID, in a thread-safe way.
  //
  // The generated ID can be used as an identifier that is unique in a particular local scope. For
  // example, a block program can have more than one AutoIncrementId instances, each associated with
  // a module scope - in this case, different modules may have conflicting IDs, but each module
  // maintains a unique ID set separately.
  internal class AutoIncrementId {
    private int _lastId;

    internal AutoIncrementId(int lastId = -1) {
      _lastId = lastId;
    }

    // Gets the next ID.
    internal int NextInt() {
      // Atomic increment operation.
      return Interlocked.Increment(ref _lastId);
    }

    // Gets the string representation of the next ID. If the string length is less than minLength,
    // the result will be padded with leading zeros.
    internal string NextString(int minLength = 0) {
      return NextInt().ToString($"D{minLength}");
    }

    // Resets the auto-increment ID.
    internal void Reset(int lastId = -1) {
      // C# spec: Reads and writes of the following data types are atomic: bool, char, byte, sbyte,
      // short, ushort, uint, int, float, and reference types.
      _lastId = lastId;
    }
  }
}
