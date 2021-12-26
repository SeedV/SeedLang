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

namespace SeedLang.Runtime {
  internal class GlobalEnvironment {
    private readonly Dictionary<string, Value> _globals = new Dictionary<string, Value>();
    private readonly Value _defaultValue;

    internal GlobalEnvironment(in Value defaultValue) {
      _defaultValue = defaultValue;
    }

    internal void SetVariable(string name, in Value value) {
      _globals[name] = value;
    }

    internal Value GetVariable(string name) {
      if (!_globals.ContainsKey(name)) {
        _globals[name] = _defaultValue;
      }
      return _globals[name];
    }
  }
}
