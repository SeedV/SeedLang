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
using System.Collections.Generic;

namespace SeedLang.Interpreter {
  // The class to resolve global and local variables. It defines and finds global variables in the
  // global environment, and allocates register slots for local and temporary variables.
  internal class VariableResolver {
    internal enum VariableType {
      Global,
      Local,
      Upvalue,
    }

    internal struct VariableInfo {
      // TODO: add information for upvalues
      public VariableType Type { get; }
      public uint Id { get; }

      internal VariableInfo(VariableType type, uint id) {
        Type = type;
        Id = id;
      }
    }

    private interface IScope {
      uint FreeRegister { get; }
      int FuncLevel { get; }

      VariableInfo DefineVariable(string name);
      uint? FindVariable(string name);
      uint AllocateRegister();
    }

    private class GlobalScope : IScope {
      public const int GlobalFuncLevel = -1;

      public uint FreeRegister { get; } = 0;
      public int FuncLevel { get; } = GlobalFuncLevel;

      private readonly GlobalEnvironment _env;

      internal GlobalScope(GlobalEnvironment env) {
        _env = env;
      }

      public VariableInfo DefineVariable(string name) {
        return new VariableInfo(VariableType.Global, _env.DefineVariable(name));
      }

      public uint? FindVariable(string name) {
        return _env.FindVariable(name) is uint id ? id : null;
      }

      public uint AllocateRegister() {
        throw new NotImplementedException("Cannot allocate registers in the global scope.");
      }
    }

    private class ExpressionScope : IScope {
      public uint FreeRegister { get; private set; }
      public int FuncLevel { get; }

      internal ExpressionScope(uint freeRegister, int currentFuncLevel) {
        FreeRegister = freeRegister;
        FuncLevel = currentFuncLevel;
      }

      public virtual VariableInfo DefineVariable(string name) {
        throw new NotImplementedException("Cannot define variables in the expression scope.");
      }

      public virtual uint? FindVariable(string name) {
        return null;
      }

      public uint AllocateRegister() {
        if (FreeRegister == Chunk.MaxRegisterCount) {
          // TODO: throw a compile error exception and handle it in the executor to generate the
          // corresponding diagnostic information.
        }
        return FreeRegister++;
      }
    }

    private class BlockScope : ExpressionScope {
      private readonly Dictionary<string, uint> _variables = new Dictionary<string, uint>();

      internal BlockScope(uint freeRegister, int currentFuncLevel) :
          base(freeRegister, currentFuncLevel) { }

      public override VariableInfo DefineVariable(string name) {
        _variables[name] = AllocateRegister();
        return new VariableInfo(VariableType.Local, _variables[name]);
      }

      public override uint? FindVariable(string name) {
        return _variables.ContainsKey(name) ? _variables[name] : null;
      }
    }

    private class FunctionScope : BlockScope {
      internal FunctionScope(uint freeRegister, int currentFuncLevel) :
          base(freeRegister, currentFuncLevel + 1) { }
    }

    // The last allocated register in the current scope.
    public uint LastRegister => _currentScope.FreeRegister - 1;

    private readonly List<IScope> _scopes = new List<IScope>();

    private IScope _currentScope => _scopes[_scopes.Count - 1];

    internal VariableResolver(GlobalEnvironment env) {
      _scopes.Add(new GlobalScope(env));
    }

    internal void BeginExpressionScope() {
      _scopes.Add(new ExpressionScope(_currentScope.FreeRegister, _currentScope.FuncLevel));
    }

    internal void EndExpressionScope() {
      EndScope();
    }

    internal void BeginBlockScope() {
      _scopes.Add(new BlockScope(_currentScope.FreeRegister, _currentScope.FuncLevel));
    }

    internal void EndBlockScope() {
      EndScope();
    }

    internal void BeginFunctionScope() {
      _scopes.Add(new FunctionScope(0, _currentScope.FuncLevel + 1));
    }

    internal void EndFunctionScope() {
      EndScope();
    }

    internal VariableInfo DefineVariable(string name) {
      return _currentScope.DefineVariable(name);
    }

    internal VariableInfo? FindVariable(string name) {
      for (int i = _scopes.Count - 1; i >= 0; i--) {
        if (_scopes[i].FindVariable(name) is uint id) {
          if (_scopes[i] is GlobalScope) {
            return new VariableInfo(VariableType.Global, id);
          } else if (_scopes[i].FuncLevel < _currentScope.FuncLevel) {
            return new VariableInfo(VariableType.Upvalue, id);
          } else {
            return new VariableInfo(VariableType.Local, id);
          }
        }
      }
      return null;
    }

    internal uint AllocateRegister() {
      return _currentScope.AllocateRegister();
    }

    private void EndScope() {
      _scopes.RemoveAt(_scopes.Count - 1);
    }
  }
}
