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

namespace SeedLang.Interpreter {
  // The allocator class to allocate register slots for local and temporary variables.
  internal class RegisterAllocator {
    // The maximum count of the allocated registers. This number is never decreased even when a
    // register is deallocated.
    public uint MaxRegisterCount = 0;
    // Current allocated registers count. This number is decreased if a register is deallocated.
    private uint _registerCount = 0;

    // Allocates a temporary variable and returns the index of the register slot.
    internal uint AllocateTempVariable() {
      _registerCount++;
      if (_registerCount > MaxRegisterCount) {
        MaxRegisterCount = _registerCount;
        if (MaxRegisterCount > Chunk.MaxRegisterCount) {
          // TODO: throw a compile error exception and handle it in the executor to generate the
          // corresponding diagnostic information.
        }
      }
      return _registerCount - 1;
    }

    // Deallocate a register slot.
    internal void DeallocateVariable() {
      _registerCount--;
    }
  }
}
