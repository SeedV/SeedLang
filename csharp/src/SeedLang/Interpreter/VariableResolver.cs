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
  using Scope = Dictionary<string, uint>;

  // The allocator class to allocate register slots for local and temporary variables.
  internal class VariableResolver {
    // The maximum count of the allocated registers. This number is never decreased even when a
    // register is deallocated.
    public uint MaxRegisterCount { get; private set; } = 0;
    // The flag to indicate if it's in the global scope.
    //
    // TODO: set to false when entering an function or block scope.
    public bool IsInGlobalScope => _scopes.Count == 0;

    private readonly Environment _env;
    // A dictionary to store names and register indices of local variables.
    private readonly Stack<Scope> _scopes = new Stack<Scope>();
    // Current allocated registers count. This number is decreased if a register is deallocated.
    private uint _registerCount = 0;
    private readonly Stack<uint> _baseOfFunctionScopes = new Stack<uint>();
    // A Stack to store the beginning of registers before parsing an expression. The expresion
    // visitor should call EnterExpressionScope() before allocating temporary variables for
    // intermediate results. The ExitExpressionScope() call will deallocate all temporary variables
    // that are allocated in this expression scope.
    private readonly Stack<uint> _baseOfExpressionScopes = new Stack<uint>();

    internal VariableResolver(Environment env) {
      _env = env;
    }

    internal void BeginFunctionScope() {
      _baseOfFunctionScopes.Push(_registerCount);
      _scopes.Push(new Scope());
    }

    internal void EndFunctionScope() {
      _registerCount = _baseOfFunctionScopes.Peek();
      _baseOfFunctionScopes.Pop();
      _scopes.Pop();
    }

    internal uint DefineVariable(string name) {
      if (IsInGlobalScope) {
        return _env.DefineVariable(name);
      } else {
        // Temporary variables are allocated and deallocated within one statement. So all expression
        // scopes shall be ended before allocating registers for local variables.
        Debug.Assert(_baseOfExpressionScopes.Count == 0);
        Scope scope = _scopes.Peek();
        Debug.Assert(!scope.ContainsKey(name));
        scope[name] = AllocateVariable();
        return scope[name];
      }
    }

    internal uint? FindVariable(string name) {
      if (!IsInGlobalScope) {
        foreach (var scope in _scopes) {
          if (scope.ContainsKey(name)) {
            return scope[name];
          }
        }
      }
      return _env.FindVariable(name);
    }

    internal void BeginExpressionScope() {
      _baseOfExpressionScopes.Push(_registerCount);
    }

    internal void EndExpressionScope() {
      _registerCount = _baseOfExpressionScopes.Peek();
      _baseOfExpressionScopes.Pop();
    }

    // Allocates a temporary variable and returns the index of the register slot.
    internal uint AllocateTempVariable() {
      return AllocateVariable();
    }

    private uint AllocateVariable() {
      _registerCount++;
      Debug.Assert(_baseOfFunctionScopes.Count > 0);
      if (_registerCount - _baseOfFunctionScopes.Peek() > Chunk.MaxRegisterCount) {
        // TODO: throw a compile error exception and handle it in the executor to generate the
        // corresponding diagnostic information.
      }
      if (_registerCount > MaxRegisterCount) {
        MaxRegisterCount = _registerCount;
      }
      return _registerCount - 1;
    }
  }
}
