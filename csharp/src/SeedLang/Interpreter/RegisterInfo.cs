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
  internal class RegisterInfo {
    private enum Type {
      Temporary,
      Local,
      Reference,
    }

    public bool IsTemporary => _type == Type.Temporary;
    public bool IsLocal => _type == Type.Local;
    public bool IsReference => _type == Type.Reference;

    public string Name { get; }
    public VariableType RefVariableType { get; }
    public IReadOnlyList<Value> Keys { get; }

    private readonly Type _type;

    internal RegisterInfo() {
      _type = Type.Temporary;
    }

    internal RegisterInfo(string name) {
      Name = name;
      _type = Type.Local;
    }

    internal RegisterInfo(string name, VariableType refVariableType, IReadOnlyList<Value> keys) {
      Name = name;
      RefVariableType = refVariableType;
      Keys = keys;
      _type = Type.Reference;
    }
  }
}
