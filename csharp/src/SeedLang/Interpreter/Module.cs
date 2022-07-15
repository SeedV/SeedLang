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

namespace SeedLang.Interpreter {
  internal class Module {
    public string Name { get; }
    public GlobalRegisters Globals { get; }

    private readonly Dictionary<string, Module> _submodules = new Dictionary<string, Module>();
    private readonly Dictionary<string, uint> _nameToIdMap = new Dictionary<string, uint>();

    internal static Module Create(string name) {
      var globals = new GlobalRegisters();
      var module = new Module(name, globals);
      module.Import("__builtins__", CreateFrom("builtins", BuiltinsDefinition.Variables, globals));
      return module;
    }

    internal static Module CreateFrom(string name, Dictionary<string, VMValue> variables,
                                      GlobalRegisters globals) {
      var module = new Module(name, globals);
      foreach (var v in variables) {
        module.DefineVariable(v.Key, v.Value);
      }
      return module;
    }

    internal Module(string name, GlobalRegisters globals) {
      Name = name;
      Globals = globals;
    }

    internal void Import(string name, Module module) {
      _submodules[name] = module;
    }

    internal uint DefineVariable(string name, in VMValue initialValue) {
      Debug.Assert(!_nameToIdMap.ContainsKey(name));
      _nameToIdMap[name] = Globals.AllocateRegister(initialValue);
      return _nameToIdMap[name];
    }

    internal uint? FindVariable(string name) {
      if (_nameToIdMap.ContainsKey(name)) {
        return _nameToIdMap[name];
      } else if (_submodules.ContainsKey("__builtins__") &&
                 _submodules["__builtins__"].FindVariable(name) is uint id) {
        return id;
      } else {
        return null;
      }
    }

    internal static bool IsInternalFunction(string name) {
      return name.StartsWith("_");
    }
  }
}
