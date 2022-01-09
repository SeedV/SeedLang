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

  // The class to resolve global and local variables. It defines and finds global variables in the
  // global environment, and allocates register slots for local and temporary variables.
  internal class VariableResolver {
    // The flag to indicate if it's in the global scope.
    public bool IsInGlobalScope => _scopes.Count == 1;
    // The last allocated register in the current scope.
    public uint LastRegister => _currentRegisterCount - 1;

    // The global environment.
    private readonly Environment _env;
    // A stack of dictionaries to store names and register indices of local variables for function
    // and block scopes.
    private readonly Stack<Scope> _scopes = new Stack<Scope>();
    // The stack of the current allocated registers count of function scopes.
    private readonly Stack<uint> _registerCounts = new Stack<uint>();
    // A Stack to store the base of registers before parsing an expression. The expresion visitor
    // should call EnterExpressionScope() before allocating temporary variables for intermediate
    // results. The ExitExpressionScope() call will deallocate all temporary variables that are
    // allocated in this expression scope.
    private readonly Stack<uint> _baseOfExpressionScopes = new Stack<uint>();
    private uint _currentRegisterCount => _registerCounts.Peek();

    internal VariableResolver(Environment env) {
      _env = env;
      // Begins global scope. It's used for temporary variables in the global scope.
      BeginFunctionScope();
    }

    internal void BeginFunctionScope() {
      _scopes.Push(new Scope());
      _registerCounts.Push(0);
    }

    internal void EndFunctionScope() {
      _scopes.Pop();
      _registerCounts.Pop();
    }

    internal void BeginBlockScope() {
      _scopes.Push(new Scope());
    }

    internal void EndBlockScope() {
      _scopes.Pop();
    }

    internal void BeginExpressionScope() {
      _baseOfExpressionScopes.Push(_currentRegisterCount);
    }

    internal void EndExpressionScope() {
      SetCurrentRegisterCount(_baseOfExpressionScopes.Peek());
      _baseOfExpressionScopes.Pop();
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

    internal uint AllocateVariable() {
      uint registerCount = _currentRegisterCount + 1;
      if (registerCount > Chunk.MaxRegisterCount) {
        // TODO: throw a compile error exception and handle it in the executor to generate the
        // corresponding diagnostic information.
      }
      SetCurrentRegisterCount(registerCount);
      return registerCount - 1;
    }

    private void SetCurrentRegisterCount(uint registerCount) {
      _registerCounts.Pop();
      _registerCounts.Push(registerCount);
    }
  }
}
