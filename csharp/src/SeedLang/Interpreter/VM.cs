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

using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The SeedLang virtual machine to run bytecode stored in a chunk.
  internal class VM {
    private readonly VisualizerCenter _visualizerCenter;
    private Chunk _chunk;
    private VMValue[] _registers;

    internal VM(VisualizerCenter visualizerCenter = null) {
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
    }

    internal void Run(Chunk chunk) {
      _chunk = chunk;
      _registers = new VMValue[chunk.RegisterCount];
      int pc = 0;
      while (pc < _chunk.Bytecode.Count) {
        Instruction instr = _chunk.Bytecode[pc];
        switch (instr.Opcode) {
          case Opcode.LOADK:
            _registers[instr.A] = _chunk.ValueOfConstId(instr.Bx);
            break;
          case Opcode.ADD:
          case Opcode.SUB:
          case Opcode.MUL:
          case Opcode.DIV:
            HandleBinary(instr, chunk.Ranges[pc]);
            break;
          case Opcode.EVAL:
            if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
              var ee = new EvalEvent(_registers[instr.A].ToValue(), chunk.Ranges[pc]);
              _visualizerCenter.EvalPublisher.Notify(ee);
            }
            break;
          case Opcode.RETURN:
            return;
        }
        ++pc;
      }
    }

    private void HandleBinary(Instruction instr, Range range) {
      BinaryOperator op = BinaryOperator.Add;
      switch (instr.Opcode) {
        case Opcode.ADD:
          _registers[instr.A] = ValueOfRK(instr.B) + ValueOfRK(instr.C);
          op = BinaryOperator.Add;
          break;
        case Opcode.SUB:
          _registers[instr.A] = ValueOfRK(instr.B) - ValueOfRK(instr.C);
          op = BinaryOperator.Subtract;
          break;
        case Opcode.MUL:
          _registers[instr.A] = ValueOfRK(instr.B) * ValueOfRK(instr.C);
          op = BinaryOperator.Multiply;
          break;
        case Opcode.DIV:
          _registers[instr.A] = ValueOfRK(instr.B) / ValueOfRK(instr.C);
          op = BinaryOperator.Divide;
          break;
      }
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(ValueOfRK(instr.B).ToValue(), op, ValueOfRK(instr.C).ToValue(),
                                 _registers[instr.A].ToValue(), range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    private VMValue ValueOfRK(uint rkPos) {
      if (rkPos < Chunk.MaxRegisterCount) {
        return _registers[rkPos];
      }
      return _chunk.ValueOfConstId(rkPos);
    }
  }
}
