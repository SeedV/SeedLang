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
using System.Diagnostics;
using System.Text;

namespace SeedLang.Interpreter {
  // A data structure to hold bytecode and constants generated from the AST tree by the compiler.
  internal class Chunk {
    // The maximum number of registers that can be allocated in the stack of a chunk.
    public const uint MaxRegisterCount = 250;

    // The actual count of the registers that is needed for this chunk.
    public uint RegisterCount = 0;

    private readonly List<Instruction> _bytecode = new List<Instruction>();
    // The constant list to hold all the constants used in this chunk.
    private readonly List<Value> _constants = new List<Value>();

    public override string ToString() {
      var sb = new StringBuilder();
      foreach (var instr in _bytecode) {
        sb.AppendLine($"{instr,-20}{ConstantOperandToString(instr)}");
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

    // Adds a number constant into the constant list and returns the id of the input constant.
    //
    // The returned constant id is the index in the constant list plus the maximum register count.
    internal uint AddConstant(double number) {
      _constants.Add(new Value(number));
      return (uint)_constants.Count - 1 + MaxRegisterCount;
    }

    // Converts the constant id to the index in the constant list.
    private int IndexOfConstId(uint constId) {
      Debug.Assert(constId >= MaxRegisterCount && constId - MaxRegisterCount < _constants.Count,
                   "Constant id is not in the range of the constant list.");
      return (int)(constId - MaxRegisterCount);
    }

    private string ConstantOperandToString(Instruction instr) {
      if (instr.Opcode == Opcode.LOADK) {
        return $"; {_constants[IndexOfConstId(instr.Bx)]}";
      }
      return "";
    }
  }
}
