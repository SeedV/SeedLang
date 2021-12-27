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

using System.Diagnostics;
using System.Text;
using SeedLang.Common;

namespace SeedLang.Interpreter {
  // An utility class to generate the disassembly text of a chunk.
  internal class Disassembler {
    private readonly Chunk _chunk;

    internal Disassembler(Chunk chunk) {
      _chunk = chunk;
    }

    public override string ToString() {
      var sb = new StringBuilder();
      for (int i = 0; i < _chunk.Bytecode.Count; ++i) {
        Debug.Assert(i < _chunk.Ranges.Count);
        Instruction instr = _chunk.Bytecode[i];
        Range range = _chunk.Ranges[i];
        sb.Append($"{i + 1,-5}{instr.Opcode,-10}{OperandsToString(instr),-15}");
        string rangeString = range is null ? "" : range.ToString();
        sb.Append($"{ConstOperandsToString(instr),-20}{rangeString}");
        sb.AppendLine();
      }
      return sb.ToString();
    }

    private string OperandsToString(Instruction instr) {
      bool ignoreC = instr.Opcode == Opcode.MOVE || instr.Opcode == Opcode.UNM;
      OpcodeType type = instr.Opcode.Type();
      switch (type) {
        case OpcodeType.A:
          return $"{instr.A}";
        case OpcodeType.ABC:
          return $"{instr.A} {RegisterOrConstantIndex(instr.B)}" +
                 (ignoreC ? "" : $" {RegisterOrConstantIndex(instr.C)}");
        case OpcodeType.ABx:
          return $"{instr.A} {RegisterOrConstantIndex(instr.Bx)}";
        case OpcodeType.AsBx:
          return $"{instr.A} {instr.SBx}";
        default:
          throw new System.NotImplementedException($"Unsupported opcode type: {type}");
      }
    }

    private int RegisterOrConstantIndex(uint rk) {
      return _chunk.IsConstIdValid(rk) ? (int)Chunk.MaxRegisterCount - (int)rk - 1 : (int)rk;
    }

    private string ConstOperandsToString(Instruction instr) {
      OpcodeType type = instr.Opcode.Type();
      switch (type) {
        case OpcodeType.ABC:
          string b = _chunk.IsConstIdValid(instr.B) ? $" {_chunk.ValueOfConstId(instr.B)}" : "";
          string c = _chunk.IsConstIdValid(instr.C) ? $" {_chunk.ValueOfConstId(instr.C)}" : "";
          if (string.IsNullOrEmpty(b) && string.IsNullOrEmpty(c)) {
            return "";
          }
          return $";{b}{c}";
        case OpcodeType.ABx:
          return _chunk.IsConstIdValid(instr.Bx) ? $"; {_chunk.ValueOfConstId(instr.Bx)}" : "";
        default:
          return "";
      }
    }
  }
}
