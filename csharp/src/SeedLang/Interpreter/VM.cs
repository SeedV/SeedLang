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
    private readonly GlobalEnvironment<VMValue> _globals = new GlobalEnvironment<VMValue>();

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
        switch (instr.Opcode) {
          case Opcode.LOADK:
            _registers[instr.A] = _chunk.ValueOfConstId(instr.Bx);
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
            _registers[instr.A] = new VMValue(-ValueOfRK(instr.B).ToNumber());
            break;
          case Opcode.EVAL:
            if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
              var ee = new EvalEvent(_registers[instr.A].ToValue(), _chunk.Ranges[pc]);
              _visualizerCenter.EvalPublisher.Notify(ee);
            }
            break;
          case Opcode.RETURN:
            return;
        }
        ++pc;
      }
    }

    private void HandleGetGlobal(Instruction instr) {
      var name = _chunk.ValueOfConstId(instr.Bx).ToString();
      if (_globals.TryGetVariable(name, out VMValue value)) {
        _registers[instr.A] = value;
      }
    }

    private void HandleSetGlobal(Instruction instr, Range range) {
      var name = _chunk.ValueOfConstId(instr.Bx).ToString();
      _globals.SetVariable(name, _registers[instr.A]);
      if (!_visualizerCenter.AssignmentPublisher.IsEmpty()) {
        var ae = new AssignmentEvent(name, _registers[instr.A].ToValue(), range);
        _visualizerCenter.AssignmentPublisher.Notify(ae);
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
          VMValue divisor = ValueOfRK(instr.C);
          CheckDivideByZero(divisor.ToNumber(), range);
          _registers[instr.A] = ValueOfRK(instr.B) / divisor;
          op = BinaryOperator.Divide;
          break;
      }
      CheckOverflow(_registers[instr.A].ToNumber(), range);
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

    // TODO: extract this utility method into a common place like runtime component.
    private static void CheckDivideByZero(double divisor, Range range) {
      if (divisor == 0) {
        // TODO: how to get the module name?
        throw new DiagnosticException(SystemReporters.SeedVM, Severity.Error, "", range,
                                      Message.RuntimeErrorDivideByZero);
      }
    }

    // TODO: extract this utility method into a common place like runtime component.
    private static void CheckOverflow(double value, Range range) {
      // TODO: do we need separate NaN as another runtime error?
      if (double.IsInfinity(value) || double.IsNaN(value)) {
        // TODO: how to get the module name?
        throw new DiagnosticException(SystemReporters.SeedVM, Severity.Error, "", range,
                                      Message.RuntimeOverflow);
      }
    }
  }
}
