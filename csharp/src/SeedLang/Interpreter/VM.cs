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

    // The global environment to store names and values of global variables.
    private readonly GlobalEnvironment<VMValue> _globals =
        new GlobalEnvironment<VMValue>(new VMValue());

    private Chunk _chunk;
    private VMValue[] _registers;

    internal VM(VisualizerCenter visualizerCenter = null) {
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
    }

    internal void Run(Chunk chunk) {
      _chunk = chunk;
      _registers = new VMValue[_chunk.RegisterCount];
      int pc = 0;
      while (pc < _chunk.Bytecode.Count) {
        Instruction instr = _chunk.Bytecode[pc];
        try {
          switch (instr.Opcode) {
            case Opcode.LOADK:
              _registers[instr.A] = LoadConstantValue(instr.Bx);
              break;
            case Opcode.GETGLOB:
              HandleGetGlobal(instr);
              break;
            case Opcode.SETGLOB:
              HandleSetGlobal(instr, _chunk.Ranges[pc]);
              break;
            case Opcode.ADD:
            case Opcode.SUB:
            case Opcode.MUL:
            case Opcode.DIV:
              HandleBinary(instr, _chunk.Ranges[pc]);
              break;
            case Opcode.UNM:
              _registers[instr.A] = new VMValue(-ValueOfRK(instr.B).Number);
              break;
            case Opcode.EVAL:
              if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
                var ee = new EvalEvent(_registers[instr.A], _chunk.Ranges[pc]);
                _visualizerCenter.EvalPublisher.Notify(ee);
              }
              break;
            case Opcode.RETURN:
              return;
          }
        } catch (DiagnosticException ex) {
          throw new DiagnosticException(SystemReporters.SeedVM, ex.Diagnostic.Severity,
                                        ex.Diagnostic.Module, _chunk.Ranges[pc],
                                        ex.Diagnostic.MessageId);
        }
        ++pc;
      }
    }

    private void HandleGetGlobal(Instruction instr) {
      var name = _chunk.ValueOfConstId(instr.Bx).ToString();
      _registers[instr.A] = _globals.
      GetVariable(name);
    }

    private void HandleSetGlobal(Instruction instr, Range range) {
      var name = _chunk.ValueOfConstId(instr.Bx).ToString();
      _globals.SetVariable(name, _registers[instr.A]);
      if (!_visualizerCenter.AssignmentPublisher.IsEmpty()) {
        var ae = new AssignmentEvent(name, _registers[instr.A], range);
        _visualizerCenter.AssignmentPublisher.Notify(ae);
      }
    }

    private void HandleBinary(Instruction instr, Range range) {
      BinaryOperator op = BinaryOperator.Add;
      double result = 0;
      VMValue left = ValueOfRK(instr.B);
      VMValue right = ValueOfRK(instr.C);
      switch (instr.Opcode) {
        case Opcode.ADD:
          op = BinaryOperator.Add;
          result = ValueHelper.Add(left, right);
          break;
        case Opcode.SUB:
          op = BinaryOperator.Subtract;
          result = ValueHelper.Subtract(left, right);
          break;
        case Opcode.MUL:
          op = BinaryOperator.Multiply;
          result = ValueHelper.Multiply(left, right);
          break;
        case Opcode.DIV:
          op = BinaryOperator.Divide;
          result = ValueHelper.Divide(left, right);
          break;
      }
      _registers[instr.A] = new VMValue(result);
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(left, op, right, _registers[instr.A], range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    private ref readonly VMValue ValueOfRK(uint rkPos) {
      if (rkPos < Chunk.MaxRegisterCount) {
        return ref _registers[rkPos];
      }
      return ref LoadConstantValue(rkPos);
    }

    private ref readonly VMValue LoadConstantValue(uint constId) {
      return ref _chunk.ValueOfConstId(constId);
    }
  }
}
