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
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  // The environment to store names and values of build-in and global variables.
  internal class GlobalRegisters {
    private readonly List<VMValue> _values = new List<VMValue>();
    private readonly HashSet<string> _definedVariables = new HashSet<string>();

    internal uint AllocateRegister(in VMValue initialValue) {
      _values.Add(initialValue);
      return (uint)_values.Count - 1;
    }

    internal VMValue this[uint id] {
      get {
        Debug.Assert(id < _values.Count);
        return _values[(int)id];
      }
      set {
        Debug.Assert(id < _values.Count);
        _values[(int)id] = value;
      }
    }

    internal IReadOnlyList<IVM.VariableInfo> GetGlobals(Module module) {
      var globals = new List<IVM.VariableInfo>();
      foreach (string name in _definedVariables) {
        if (module.FindVariable(name) is uint id) {
          globals.Add(new IVM.VariableInfo(name, new Value(_values[(int)id])));
        }
      }
      return globals;
    }

    internal bool DefineVariable(string name) {
      if (_definedVariables.Contains(name)) {
        return false;
      }
      _definedVariables.Add(name);
      return true;
    }
  }
}
