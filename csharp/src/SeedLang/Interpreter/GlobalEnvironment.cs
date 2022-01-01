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
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The global environment class to store names and values of global variables.
  //
  // TODO: looking up a global variable by strings in the dictionary is quite slow. So the
  // performance of register-based local variables is much faster than global variables. It's
  // possible to cache all global variable names during compilation, and use indices to search
  // global variables in the dictionary. Decide if we need such kind of optimization.
  internal class GlobalEnvironment {
    private readonly Dictionary<string, Value> _globals = new Dictionary<string, Value>();

    internal void SetVariable(string name, in Value value) {
      _globals[name] = value;
    }

    internal Value GetVariable(string name) {
      if (!_globals.ContainsKey(name)) {
        _globals[name] = Value.None();
      }
      return _globals[name];
    }
  }
}
