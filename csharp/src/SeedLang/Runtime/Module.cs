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
using SeedLang.Visualization;

namespace SeedLang.Runtime {
  internal class Module {
    public string Name { get; }
    public GlobalRegisters Registers { get; }

    public VMValue Dir {
      get {
        var list = new List<VMValue>();
        foreach (string name in _nameToIdMap.Keys) {
          list.Add(new VMValue(name));
        }
        return new VMValue(list);
      }
    }

    public IReadOnlyList<IVM.VariableInfo> Globals {
      get {
        return Registers.GetGlobals(this);
      }
    }

    private const string _builtinsAliasName = "__builtins__";
    private const string _builtinsModuleName = "builtins";

    private readonly Dictionary<string, uint> _nameToIdMap = new Dictionary<string, uint>();

    internal static Module Create(string name) {
      var module = new Module(name, new GlobalRegisters());
      module.ImportBuiltinModule(_builtinsAliasName, _builtinsModuleName,
                                 BuiltinsDefinition.Variables);
      return module;
    }

    internal static bool IsInternalFunction(string name) {
      return name.StartsWith("_");
    }

    internal Module(string name, GlobalRegisters globals) {
      Name = name;
      Registers = globals;
    }

    internal void ImportBuiltinModule(string name) {
      switch (name) {
        case _builtinsModuleName:
          ImportBuiltinModule(name, name, BuiltinsDefinition.Variables);
          break;
        case "math":
          ImportBuiltinModule(name, name, MathDefinition.Variables);
          break;
      };
    }

    internal uint DefineVariable(string name, in VMValue initialValue) {
      Debug.Assert(!_nameToIdMap.ContainsKey(name));
      _nameToIdMap[name] = Registers.AllocateRegister(initialValue);
      return _nameToIdMap[name];
    }

    internal uint? FindVariable(string name) {
      if (_nameToIdMap.ContainsKey(name)) {
        return _nameToIdMap[name];
      } else if (_nameToIdMap.ContainsKey(_builtinsAliasName)) {
        VMValue value = Registers[_nameToIdMap[_builtinsAliasName]];
        if (value.IsModule) {
          return value.AsModule().FindVariable(name);
        }
      }
      return null;
    }

    internal uint? FindVariable(Span<string> names) {
      if (names.Length > 1) {
        if (_nameToIdMap.ContainsKey(names[0])) {
          VMValue value = Registers[_nameToIdMap[names[0]]];
          if (value.IsModule) {
            return value.AsModule().FindVariable(names[1..]);
          }
        }
      } else if (names.Length == 1) {
        return FindVariable(names[0]);
      }
      return null;
    }

    private void ImportBuiltinModule(string aliasName, string name,
                                     Dictionary<string, VMValue> variables) {
      if (!_nameToIdMap.ContainsKey(aliasName)) {
        var module = new Module(name, Registers);
        foreach (var v in variables) {
          module.DefineVariable(v.Key, v.Value);
        }
        DefineVariable(aliasName, new VMValue(module));
      }
    }
  }
}
