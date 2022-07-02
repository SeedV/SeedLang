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
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  internal sealed class VariableInfo : IEquatable<VariableInfo> {
    public string Name { get; }
    public VariableType Type { get; }
    public uint Id { get; }

    internal VariableInfo(string name, VariableType type, uint id) {
      Name = name;
      Type = type;
      Id = id;
    }

    public static bool operator ==(VariableInfo lhs, VariableInfo rhs) {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(VariableInfo lhs, VariableInfo rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(VariableInfo other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      return Name == other.Name && Type == other.Type && Id == other.Id;
    }

    public override bool Equals(object obj) {
      return Equals(obj as VariableInfo);
    }

    public override int GetHashCode() {
      return new { Name, Type, Id }.GetHashCode();
    }

    public override string ToString() {
      return $"'{Name}' {Type} {Id}";
    }
  }

  // The class to resolve global, local and temporary variables. It defines global variables in the
  // global environment, and allocates register slots for local and temporary variables.
  internal class VariableResolver {
    private class Registers {
      public int Count => _variableInfos.Count;

      private readonly List<VariableInfo> _variableInfos = new List<VariableInfo>();

      internal uint AllocateRegister(string name = null) {
        var id = (uint)_variableInfos.Count;
        var info = name is null ? null : new VariableInfo(name, VariableType.Local, id);
        _variableInfos.Add(info);
        return id;
      }

      internal VariableInfo this[int index] {
        get {
          Debug.Assert(index >= 0 && index < _variableInfos.Count);
          return _variableInfos[index];
        }
      }

      internal void RemoveFrom(int start) {
        _variableInfos.RemoveRange(start, _variableInfos.Count - start);
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
      public string Path => null;
      public Registers Registers { get; } = new Registers();

      private readonly Module _module;

      public GlobalScope(Module module) {
        _module = module;
      }

      public VariableInfo DefineVariable(string name) {
        return new VariableInfo(name, VariableType.Global,
                                _module.DefineVariable(name, new VMValue()));
      }

      public VariableInfo FindVariable(string name) {
        if (_module.FindVariable(name) is uint id) {
          return new VariableInfo(name, VariableType.Global, id);
        }
        return null;
      }

      public uint DefineTempVariable() {
        return Registers.AllocateRegister();
      }
    }

    private class ExprScope : IScope {
      public string Path => null;
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
        return Registers.AllocateRegister();
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
        Path = !(parent.Path is null) ? $"{parent.Path}.{name}" : $"{name}";
      }

      public VariableInfo DefineVariable(string name) {
        string chainedName = $"{Path}.{name}";
        uint id = Registers.AllocateRegister(chainedName);
        _variables[name] = id;
        return new VariableInfo(chainedName, VariableType.Local, id);
      }

      public VariableInfo FindVariable(string name) {
        return _variables.ContainsKey(name) ? Registers[(int)_variables[name]] : null;
      }

      public uint DefineTempVariable() {
        return Registers.AllocateRegister();
      }
    }

    public uint RegisterCount => (uint)_currentScope.Registers.Count;

    private readonly List<IScope> _scopes = new List<IScope>();
    private IScope _currentScope => _scopes[^1];

    internal VariableResolver(Module module) {
      _scopes.Add(new GlobalScope(module));
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
