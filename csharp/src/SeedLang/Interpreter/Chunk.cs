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
using System.Text;

namespace SeedLang.Interpreter {
  internal class Chunk {
    private readonly List<Instruction> _bytecode = new List<Instruction>();
    private readonly List<Value> _constants = new List<Value>();

    public override string ToString() {
      var sb = new StringBuilder();
      foreach (var instr in _bytecode) {
        sb.AppendLine($"{instr}");
      }
      return sb.ToString();
    }

    internal void Emit(Opcode opcode, uint a) {
      _bytecode.Add(new Instruction(opcode, a));
    }

    internal void Emit(Opcode opcode, uint a, uint b, uint c) {
      _bytecode.Add(new Instruction(opcode, a, b, c));
    }

    internal void Emit(Opcode opcode, uint a, uint bx) {
      _bytecode.Add(new Instruction(opcode, a, bx));
    }

    internal uint AddConstant(double number) {
      _constants.Add(new Value(number));
      return (uint)_constants.Count - 1;
    }
  }
}
