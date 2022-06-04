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
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  // The class to store variable information of registers.
  internal interface RegisterInfo { }

  internal class TempVariable : RegisterInfo { }

  internal class LocalVariable : RegisterInfo {
    public string Name { get; }

    internal LocalVariable(string name) {
      Name = name;
    }
  }

  internal class Reference : RegisterInfo {
    public VariableType Type { get; }
    public string Name { get; }
    public IReadOnlyList<Value> Keys { get; }

    internal Reference(VariableType type, string name, IReadOnlyList<Value> keys) {
      Type = type;
      Name = name;
      Keys = keys;
    }
  }
}
