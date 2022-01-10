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
  // The environment to store names and values of build-in and global variables.
  //
  // TODO: handle build-in variables.
  internal class GlobalEnvironment {
    private readonly Dictionary<string, uint> _globals = new Dictionary<string, uint>();
    private readonly List<Value> _values = new List<Value>();

    internal uint DefineVariable(string name) {
      Debug.Assert(!_globals.ContainsKey(name));
      _values.Add(Value.None());
      _globals[name] = (uint)_values.Count - 1;
      return _globals[name];
    }

    internal uint? FindVariable(string name) {
      if (_globals.ContainsKey(name)) {
        return _globals[name];
      }
      return null;
    }

    internal void SetVariable(uint id, in Value value) {
      Debug.Assert(id < _values.Count);
      _values[(int)id] = value;
    }

    internal Value GetVariable(uint id) {
      Debug.Assert(id < _values.Count);
      return _values[(int)id];
    }
  }
}
