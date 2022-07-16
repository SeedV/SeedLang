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
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  internal class Module {
    public string Name { get; }
    public GlobalRegisters Globals { get; }

    private const string _builtinsAliasName = "__builtins__";
    private const string _builtinsModuleName = "builtins";

    private readonly Dictionary<string, Module> _submodules = new Dictionary<string, Module>();
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
      Globals = globals;
    }

    internal void ImportBuiltinModule(string name) {
      switch (name) {
        case _builtinsAliasName:
          ImportBuiltinModule(name, name, BuiltinsDefinition.Variables);
          break;
        case "math":
          ImportBuiltinModule(name, name, MathDefinition.Variables);
          break;
      };
    }

    internal uint DefineVariable(string name, in VMValue initialValue) {
      Debug.Assert(!_nameToIdMap.ContainsKey(name));
      _nameToIdMap[name] = Globals.AllocateRegister(initialValue);
      return _nameToIdMap[name];
    }

    internal uint? FindVariable(string name, string submoduleName = null) {
      if (!(submoduleName is null)) {
        if (_submodules.ContainsKey(submoduleName) &&
            _submodules[submoduleName].FindVariable(name) is uint id) {
          return id;
        }
      } else if (_nameToIdMap.ContainsKey(name)) {
        return _nameToIdMap[name];
      } else if (_submodules.ContainsKey(_builtinsAliasName) &&
                 _submodules[_builtinsAliasName].FindVariable(name) is uint id) {
        return id;
      }
      return null;
    }

    private void ImportBuiltinModule(string aliasName, string name,
                                     Dictionary<string, VMValue> variables) {
      if (!_submodules.ContainsKey(aliasName)) {
        var module = new Module(name, Globals);
        foreach (var v in variables) {
          module.DefineVariable(v.Key, v.Value);
        }
        _submodules[aliasName] = module;
      }
    }
  }
}
