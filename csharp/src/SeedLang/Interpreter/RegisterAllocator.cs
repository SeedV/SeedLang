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

namespace SeedLang.Interpreter {
  // The allocator class to allocate register slots for local and temporary variables.
  internal class RegisterAllocator {
    // The maximum count of the allocated registers. This number is never decreased even when a
    // register is deallocated.
    public uint MaxRegisterCount { get; private set; } = 0;
    // The flag to indicate if it's in the global scope.
    //
    // TODO: set to false when entering an function or block scope.
    public bool IsInGlobalScope { get; private set; } = true;

    // A dictionary to store names and register indices of local variables.
    //
    // TODO: add a frame stack to support registers for each function call.
    private readonly Dictionary<string, uint> _locals = new Dictionary<string, uint>();
    // Current allocated registers count. This number is decreased if a register is deallocated.
    private uint _registerCount = 0;
    // A Stack to store the beginning of registers before parsing an expression. The expresion
    // visitor should call EnterExpressionScope() before allocating temporary variables for
    // intermediate results. The ExitExpressionScope() call will deallocate all temporary variables
    // that are allocated in this expression scope.
    private readonly Stack<uint> _beginningOfExpressionScopes = new Stack<uint>();

    // Allocates a register for a local variable with the given name.
    internal uint RegisterOfVariable(string name) {
      // Temporary variables are allocated and deallocated within one statement. So all expression
      // scopes shall be exited before allocating registers for local variables.
      Debug.Assert(_beginningOfExpressionScopes.Count == 0);
      if (!_locals.ContainsKey(name)) {
        _locals[name] = AllocateVariable();
      }
      return _locals[name];
    }

    internal void EnterExpressionScope() {
      _beginningOfExpressionScopes.Push(_registerCount);
    }

    internal void ExitExpressionScope() {
      _registerCount = _beginningOfExpressionScopes.Peek();
      _beginningOfExpressionScopes.Pop();
    }

    // Allocates a temporary variable and returns the index of the register slot.
    internal uint AllocateTempVariable() {
      return AllocateVariable();
    }

    private uint AllocateVariable() {
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
  }
}
