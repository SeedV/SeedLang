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

using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The SeedLang virtual machine to run bytecode stored in a chunk.
  internal class VM {
    public readonly Environment Env = new Environment();
    private readonly VisualizerCenter _visualizerCenter;
    private readonly CallStack _callStack = new CallStack();

    private Value[] _registers;

    internal VM(VisualizerCenter visualizerCenter = null) {
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
    }

    internal void Run(Function func, uint maxRegisterCount) {
      _registers = new Value[maxRegisterCount];
      _callStack.PushFunc(func);
      Chunk chunk = func.Chunk;
      int pc = 0;
      while (pc < chunk.Bytecode.Count) {
        Instruction instr = chunk.Bytecode[pc];
        try {
          switch (instr.Opcode) {
            case Opcode.MOVE:
              _registers[instr.A] = _registers[instr.B];
              break;
            case Opcode.LOADK:
              _registers[instr.A] = chunk.ValueOfConstId(instr.Bx);
              break;
            case Opcode.GETGLOB:
              _registers[instr.A] = Env.GetVariable(instr.Bx);
              break;
            case Opcode.SETGLOB:
              Env.SetVariable(instr.Bx, _registers[instr.A]);
              // TODO: comments for not notify.
              break;
            case Opcode.ADD:
            case Opcode.SUB:
            case Opcode.MUL:
            case Opcode.DIV:
              HandleBinary(chunk, instr, chunk.Ranges[pc]);
              break;
            case Opcode.UNM:
              _registers[instr.A] = Value.Number(-ValueOfRK(chunk, instr.B).AsNumber());
              break;
            case Opcode.JMP:
              pc += instr.SBx;
              break;
            case Opcode.EQ:
              if (ValueOfRK(chunk, instr.B).AsNumber() == ValueOfRK(chunk, instr.C).AsNumber() ==
                  (instr.A == 1)) {
                pc++;
              }
              break;
            case Opcode.LT:
              if ((ValueOfRK(chunk, instr.B).AsNumber() < ValueOfRK(chunk, instr.C).AsNumber()) ==
                  (instr.A == 1)) {
                pc++;
              }
              break;
            case Opcode.LE:
              if ((ValueOfRK(chunk, instr.B).AsNumber() <= ValueOfRK(chunk, instr.C).AsNumber()) ==
                  (instr.A == 1)) {
                pc++;
              }
              break;
            case Opcode.EVAL:
              if (!_visualizerCenter.EvalPublisher.IsEmpty()) {
                var ee = new EvalEvent(new ValueWrapper(_registers[instr.A]), chunk.Ranges[pc]);
                _visualizerCenter.EvalPublisher.Notify(ee);
              }
              break;
            case Opcode.RETURN:
              _callStack.PopFunc();
              return;
            default:
              throw new System.NotImplementedException($"Unimplemented opcode: {instr.Opcode}");
          }
        } catch (DiagnosticException ex) {
          throw new DiagnosticException(SystemReporters.SeedVM, ex.Diagnostic.Severity,
                                        ex.Diagnostic.Module, chunk.Ranges[pc],
                                        ex.Diagnostic.MessageId);
        }
        pc++;
      }
    }

    private void HandleBinary(Chunk chunk, Instruction instr, Range range) {
      BinaryOperator op = BinaryOperator.Add;
      double result = 0;
      Value left = ValueOfRK(chunk, instr.B);
      Value right = ValueOfRK(chunk, instr.C);
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
      _registers[instr.A] = Value.Number(result);
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(new ValueWrapper(left), op, new ValueWrapper(right),
                                 new ValueWrapper(_registers[instr.A]), range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    // Gets the register value or constant value according to rkPos. Returns a readonly reference to
    // avoid copying.
    private ref readonly Value ValueOfRK(Chunk chunk, uint rkPos) {
      if (rkPos < Chunk.MaxRegisterCount) {
        return ref _registers[rkPos];
      }
      return ref chunk.ValueOfConstId(rkPos);
    }
  }
}
