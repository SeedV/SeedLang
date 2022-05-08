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
using System.Diagnostics;

namespace SeedLang.Interpreter {
  internal class VariableResolver {
    internal enum VariableType {
      Global,
      Local,
      Temporary,
      Upvalue,
    }

    internal class VariableInfo {
      public VariableType Type { get; }
      public uint Id { get; }
      public string Name { get; }

      public VariableInfo(VariableType type, uint id, string name) {
        Type = type;
        Id = id;
        Name = name;
      }
    }

    private class Registers {
      public int Count => _registers.Count;
      private readonly List<VariableInfo> _registers = new List<VariableInfo>();

      internal VariableInfo AllocateRegister(string name = null) {
        var type = name is null ? VariableType.Temporary : VariableType.Local;
        var info = new VariableInfo(type, (uint)_registers.Count, name);
        _registers.Add(info);
        return info;
      }

      internal VariableInfo this[int index] {
        get {
          Debug.Assert(index >= 0 && index < _registers.Count);
          return _registers[index];
        }
      }

      internal void RemoveFrom(int start) {
        _registers.RemoveRange(start, _registers.Count - start);
      }
    }

    private interface IScope {
      string Path { get; }
      Registers Registers { get; }
      VariableInfo DefineVariable(string name);
      VariableInfo FindVariable(string name);
      uint DefineTempVariable();
    }

    private class GlobalScope : IScope {
      public string Path => "global";
      public Registers Registers { get; } = new Registers();

      private readonly GlobalEnvironment _env;

      public GlobalScope(GlobalEnvironment env) {
        _env = env;
      }

      public VariableInfo DefineVariable(string name) {
        return VariableInfoOf(name, _env.DefineVariable(name));
      }

      public VariableInfo FindVariable(string name) {
        return _env.FindVariable(name) is uint id ? VariableInfoOf(name, id) : null;
      }

      public uint DefineTempVariable() {
        return Registers.AllocateRegister().Id;
      }

      private VariableInfo VariableInfoOf(string name, uint id) {
        return new VariableInfo(VariableType.Global, id, $"{Path}.{name}");
      }
    }

    private class ExprScope : IScope {
      public string Path => "";
      public Registers Registers { get; }

      private readonly int _start;

      internal ExprScope(IScope parent) {
        Registers = parent.Registers;
        _start = Registers.Count;
      }

      public VariableInfo DefineVariable(string name) {
        throw new NotImplementedException("Cannot define variables in expression scopes.");
      }

      public VariableInfo FindVariable(string name) {
        return null;
      }

      public uint DefineTempVariable() {
        return Registers.AllocateRegister().Id;
      }

      internal void ClearTempVariables() {
        Registers.RemoveFrom(_start);
      }
    }

    private class FuncScope : IScope {
      public string Path { get; }
      public Registers Registers { get; } = new Registers();

      private readonly Dictionary<string, uint> _variables = new Dictionary<string, uint>();

      internal FuncScope(IScope parent, string name) {
        Path = $"{parent.Path}.{name}";
      }

      public VariableInfo DefineVariable(string name) {
        VariableInfo info = Registers.AllocateRegister($"{Path}.{name}");
        _variables[name] = info.Id;
        return info;
      }

      public VariableInfo FindVariable(string name) {
        return _variables.ContainsKey(name) ? Registers[(int)_variables[name]] : null;
      }

      public uint DefineTempVariable() {
        return Registers.AllocateRegister().Id;
      }
    }

    public uint LastRegister {
      get {
        Debug.Assert(_currentScope.Registers.Count > 0);
        return (uint)_currentScope.Registers.Count - 1;
      }
    }

    private readonly List<IScope> _scopes = new List<IScope>();
    private IScope _currentScope => _scopes[_scopes.Count - 1];

    internal VariableResolver(GlobalEnvironment env) {
      _scopes.Add(new GlobalScope(env));
    }

    internal void BeginExprScope() {
      _scopes.Add(new ExprScope(_currentScope));
    }

    internal void EndExprScope() {
      Debug.Assert(_scopes.Count > 0 && _currentScope is ExprScope);
      (_currentScope as ExprScope).ClearTempVariables();
      EndScope();
    }

    internal void BeginFuncScope(string name) {
      Debug.Assert(!(_currentScope is ExprScope));
      _scopes.Add(new FuncScope(_currentScope, name));
    }

    internal void EndFuncScope() {
      Debug.Assert(_scopes.Count > 0 && _currentScope is FuncScope);
      EndScope();
    }

    internal VariableInfo DefineVariable(string name) {
      return _currentScope.DefineVariable(name);
    }

    internal VariableInfo FindVariable(string name) {
      for (int i = _scopes.Count - 1; i >= 0; i--) {
        if (_scopes[i].FindVariable(name) is VariableInfo info) {
          return info;
        }
      }
      return null;
    }

    internal uint DefineTempVariable() {
      return _currentScope.DefineTempVariable();
    }

    private void EndScope() {
      _scopes.RemoveAt(_scopes.Count - 1);
    }
  }
}
