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
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // A function value type that is only used in SeedVM component to encapsulate a compiled bytecode
  // chunk.
  internal class Function : IFunction {
    public string Name { get; }
    public Chunk Chunk { get; } = new Chunk();

    internal Function(string name) {
      Name = name;
    }

    // Calls the function with given arguments that locate in the "args" array starting from
    // "offset". The number of arguments is "length".
    public Value Call(Value[] args, int offset, int length) {
      throw new NotImplementedException();
    }

    public override string ToString() {
      return $"Func <{Name}>";
    }
  }
}
