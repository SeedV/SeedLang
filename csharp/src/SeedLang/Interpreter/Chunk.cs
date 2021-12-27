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
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // A data structure to hold bytecode and constants generated from the AST tree by the compiler.
  internal class Chunk {
    // The maximum number of registers that can be allocated in the stack of a chunk.
    public const uint MaxRegisterCount = 250;

    // The bytecode of this chunk.
    public IReadOnlyList<Instruction> Bytecode => _bytecode;

    // Source code ranges of the instructions in bytecode.
    //
    // The length of Bytecode and Range lists shall be the same.
    public IReadOnlyList<Range> Ranges => _ranges;

    // The actual count of the registers that is needed for this chunk.
    public uint RegisterCount { get; set; }
    public int BytecodeCount => _bytecode.Count;

    private readonly List<Instruction> _bytecode = new List<Instruction>();

    private readonly List<Range> _ranges = new List<Range>();

    // The constant list to hold all the constants used in this chunk.
    private Value[] _constants;

    internal void EmitA(Opcode opcode, uint a, Range range = null) {
      _bytecode.Add(Instruction.TypeA(opcode, a));
      _ranges.Add(range);
    }

    internal void EmitABC(Opcode opcode, uint a, uint b, uint c, Range range = null) {
      _bytecode.Add(Instruction.TypeABC(opcode, a, b, c));
      _ranges.Add(range);
    }

    internal void EmitABx(Opcode opcode, uint a, uint bx, Range range = null) {
      _bytecode.Add(Instruction.TypeABx(opcode, a, bx));
      _ranges.Add(range);
    }

    internal void EmitAsBx(Opcode opcode, uint a, int sbx, Range range = null) {
      _bytecode.Add(Instruction.TypeAsBx(opcode, a, sbx));
      _ranges.Add(range);
    }

    internal void PatchJumpAt(int pos, int sbx) {
      _bytecode[pos] = Instruction.TypeAsBx(_bytecode[pos].Opcode, _bytecode[pos].A, sbx);
    }

    // Sets the constant list. It must be called by the compiler after compilation.
    internal void SetConstants(Value[] constants) {
      _constants = constants;
    }

    internal bool IsConstIdValid(uint constId) {
      return constId >= MaxRegisterCount && constId - MaxRegisterCount < _constants.Length;
    }

    // Gets the constant value of the given constId. Returns a readonly reference to avoid copying.
    internal ref readonly Value ValueOfConstId(uint constId) {
      return ref _constants[IndexOfConstId(constId)];
    }

    // Converts the constant id to the index in the constant list.
    private int IndexOfConstId(uint constId) {
      Debug.Assert(IsConstIdValid(constId), "Invalid constant id.");
      return (int)(constId - MaxRegisterCount);
    }
  }
}
