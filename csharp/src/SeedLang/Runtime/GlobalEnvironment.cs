// Copyright 2021 The Aha001 Team.
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
  // The global environment to store names and values of global variables. The generic parameter
  // Value could be runtime Value or VMValue.
  internal class GlobalEnvironment<Value> {
    private readonly Dictionary<string, Value> _globals = new Dictionary<string, Value>();

    internal void SetVariable(string name, Value value) {
      _globals[name] = value;
    }

    internal bool TryGetVariable(string name, out Value value) {
      return _globals.TryGetValue(name, out value);
    }
  }
}
