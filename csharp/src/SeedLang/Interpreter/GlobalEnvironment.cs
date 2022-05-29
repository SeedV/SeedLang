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
using SeedLang.Runtime.HeapObjects;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  // The environment to store names and values of build-in and global variables.
  internal class GlobalEnvironment {
    public IEnumerable<IVM.VariableInfo> Globals {
      get {
        var globals = new List<IVM.VariableInfo>();
        foreach ((string name, uint id) in _globals) {
          // TODO: global variables are defined in compile-time. How to handle it?
          if (!_values[(int)id].IsNil) {
            globals.Add(new IVM.VariableInfo(name, new Value(_values[(int)id])));
          }
        }
        return globals;
      }
    }

    private readonly Dictionary<string, uint> _globals = new Dictionary<string, uint>();
    private readonly List<VMValue> _values = new List<VMValue>();

    internal GlobalEnvironment(IEnumerable<NativeFunction> nativeFunctions) {
      foreach (var func in nativeFunctions) {
        _values.Add(new VMValue(func));
        _globals[func.Name] = (uint)_values.Count - 1;
      }
    }

    internal uint DefineVariable(string name) {
      Debug.Assert(!_globals.ContainsKey(name));
      _values.Add(new VMValue());
      _globals[name] = (uint)_values.Count - 1;
      return _globals[name];
    }

    internal uint? FindVariable(string name) {
      if (_globals.ContainsKey(name)) {
        return _globals[name];
      }
      return null;
    }

    internal void SetVariable(uint id, in VMValue value) {
      Debug.Assert(id < _values.Count);
      _values[(int)id] = value;
    }

    internal VMValue GetVariable(uint id) {
      Debug.Assert(id < _values.Count);
      return _values[(int)id];
    }
  }
}
