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
using System.Collections.Immutable;
using System.IO;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The SeedLang virtual machine to run bytecode stored in a chunk.
  internal class VM {
    public GlobalEnvironment Env { get; } = new GlobalEnvironment(NativeFunctions.Funcs);
    public VisualizerCenter VisualizerCenter { get; } = new VisualizerCenter();

    private readonly Sys _sys = new Sys();

    // The stack size. Each function can allocate maximun 250 registers in the stack. So the stack
    // can hold maximun 100 recursive function calls.
    private const int _stackSize = 25 * 1024;

    private readonly Value[] _stack = new Value[_stackSize];
    private CallStack _callStack;

    internal void RedirectStdout(TextWriter stdout) {
      _sys.Stdout = stdout;
    }

    internal void Run(Function func) {
      uint baseRegister = 0;
      _callStack = new CallStack();
      _callStack.PushFunc(func, baseRegister, 0);
      Chunk chunk = func.Chunk;
      int pc = 0;
      while (pc < chunk.Bytecode.Count) {
        Instruction instr = chunk.Bytecode[pc];
        try {
          switch (instr.Opcode) {
            case Opcode.MOVE:
              _stack[baseRegister + instr.A] = _stack[baseRegister + instr.B];
              break;
            case Opcode.LOADNIL:
              for (int i = 0; i < instr.B; i++) {
                _stack[baseRegister + instr.A + i] = new Value();
              }
              break;
            case Opcode.LOADBOOL:
              _stack[baseRegister + instr.A] = new Value(instr.B == 1);
              if (instr.C == 1) {
                pc++;
              }
              break;
            case Opcode.LOADK:
              _stack[baseRegister + instr.A] = chunk.ValueOfConstId(instr.Bx);
              break;
            case Opcode.NEWTUPLE:
              var builder = ImmutableArray.CreateBuilder<Value>((int)instr.C);
              for (int i = 0; i < instr.C; i++) {
                builder.Add(_stack[baseRegister + instr.B + i]);
              }
              _stack[baseRegister + instr.A] = new Value(builder.MoveToImmutable());
              break;
            case Opcode.NEWLIST:
              var list = new List<Value>((int)instr.C);
              for (int i = 0; i < instr.C; i++) {
                list.Add(_stack[baseRegister + instr.B + i]);
              }
              _stack[baseRegister + instr.A] = new Value(list);
              break;
            case Opcode.NEWDICT:
              int count = (int)instr.C / 2;
              var dict = new Dictionary<Value, Value>(count);
              uint dictRegister = baseRegister + instr.A;
              uint kvStart = baseRegister + instr.B;
              for (uint i = 0; i < count; i++) {
                uint keyRegister = kvStart + i * 2;
                dict[_stack[keyRegister]] = _stack[keyRegister + 1];
              }
              _stack[dictRegister] = new Value(dict);
              break;
            case Opcode.GETGLOB:
              _stack[baseRegister + instr.A] = Env.GetVariable(instr.Bx);
              break;
            case Opcode.SETGLOB:
              Env.SetVariable(instr.Bx, _stack[baseRegister + instr.A]);
              break;
            case Opcode.GETELEM:
              GetElement(chunk, instr, baseRegister);
              break;
            case Opcode.SETELEM:
              SetElement(chunk, instr, baseRegister);
              break;
            case Opcode.ADD:
            case Opcode.SUB:
            case Opcode.MUL:
            case Opcode.DIV:
            case Opcode.FLOORDIV:
            case Opcode.POW:
            case Opcode.MOD:
              HandleBinary(chunk, instr, baseRegister);
              break;
            case Opcode.UNM:
              _stack[baseRegister + instr.A] =
                  new Value(-ValueOfRK(chunk, instr.B, baseRegister).AsNumber());
              break;
            case Opcode.LEN:
              _stack[baseRegister + instr.A] = new Value(_stack[baseRegister + instr.B].Length);
              break;
            case Opcode.JMP:
              pc += instr.SBx;
              break;
            case Opcode.EQ: {
                bool result = ValueOfRK(chunk, instr.B, baseRegister).Equals(
                    ValueOfRK(chunk, instr.C, baseRegister));
                if (result == (instr.A == 1)) {
                  pc++;
                }
              }
              break;
            case Opcode.LT: {
                bool result = ValueHelper.Less(ValueOfRK(chunk, instr.B, baseRegister),
                                               ValueOfRK(chunk, instr.C, baseRegister));
                if (result == (instr.A == 1)) {
                  pc++;
                }
              }
              break;
            case Opcode.LE: {
                bool result = ValueHelper.LessEqual(ValueOfRK(chunk, instr.B, baseRegister),
                                                    ValueOfRK(chunk, instr.C, baseRegister));
                if (result == (instr.A == 1)) {
                  pc++;
                }
              }
              break;
            case Opcode.IN: {
                bool result = ValueHelper.Contains(ValueOfRK(chunk, instr.C, baseRegister),
                                                   ValueOfRK(chunk, instr.B, baseRegister));
                if (result == (instr.A == 1)) {
                  pc++;
                }
              }
              break;
            case Opcode.TEST:
              if (_stack[baseRegister + instr.A].AsBoolean() == (instr.C == 1)) {
                pc++;
              }
              break;
            case Opcode.TESTSET:
              // TODO: implement the TESTSET opcode.
              break;
            case Opcode.FORPREP: {
                uint loopReg = baseRegister + instr.A;
                const uint stepOff = 2;
                _stack[loopReg] = ValueHelper.Subtract(_stack[loopReg], _stack[loopReg + stepOff]);
                pc += instr.SBx;
                break;
              }
            case Opcode.FORLOOP: {
                uint loopReg = baseRegister + instr.A;
                const uint limitOff = 1;
                const uint stepOff = 2;
                _stack[loopReg] = ValueHelper.Add(_stack[loopReg], _stack[loopReg + stepOff]);
                if (_stack[loopReg].AsNumber() < _stack[loopReg + limitOff].AsNumber()) {
                  pc += instr.SBx;
                }
                break;
              }
            case Opcode.CALL:
              CallFunction(ref chunk, ref pc, ref baseRegister, instr);
              break;
            case Opcode.RETURN:
              // TODO: only support one return value now.
              if (baseRegister > 0) {
                uint returnRegister = baseRegister - 1;
                _stack[returnRegister] = instr.B > 0 ? _stack[baseRegister + instr.A] : new Value();
              }
              _callStack.PopFunc();
              if (!_callStack.IsEmpty) {
                chunk = _callStack.CurrentChunk();
                baseRegister = _callStack.CurrentBase();
                pc = _callStack.CurrentPC();
              }
              break;
            case Opcode.VISNOTIFY:
              if (instr.Bx > 0) {
                chunk.Notifications[(int)instr.Bx - 1].Notify(VisualizerCenter, (uint id) => {
                  return ValueOfRK(chunk, id, baseRegister);
                }, instr.A, chunk.Ranges[pc]);
              } else {
                var ae = new Event.SingleStep(chunk.Ranges[pc]);
                VisualizerCenter.Notify(ae);
              }
              break;
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

    private void GetElement(Chunk chunk, Instruction instr, uint baseRegister) {
      Value index = ValueOfRK(chunk, instr.C, baseRegister);
      _stack[baseRegister + instr.A] = _stack[baseRegister + instr.B][index];
    }

    private void SetElement(Chunk chunk, Instruction instr, uint baseRegister) {
      Value index = ValueOfRK(chunk, instr.B, baseRegister);
      _stack[baseRegister + instr.A][index] = ValueOfRK(chunk, instr.C, baseRegister);
    }

    private void HandleBinary(Chunk chunk, Instruction instr, uint baseRegister) {
      Value left = ValueOfRK(chunk, instr.B, baseRegister);
      Value right = ValueOfRK(chunk, instr.C, baseRegister);
      uint register = baseRegister + instr.A;
      switch (instr.Opcode) {
        case Opcode.ADD:
          _stack[register] = ValueHelper.Add(left, right);
          break;
        case Opcode.SUB:
          _stack[register] = ValueHelper.Subtract(left, right);
          break;
        case Opcode.MUL:
          _stack[register] = ValueHelper.Multiply(left, right);
          break;
        case Opcode.DIV:
          _stack[register] = ValueHelper.Divide(left, right);
          break;
        case Opcode.FLOORDIV:
          _stack[register] = ValueHelper.FloorDivide(left, right);
          break;
        case Opcode.POW:
          _stack[register] = ValueHelper.Power(left, right);
          break;
        case Opcode.MOD:
          _stack[register] = ValueHelper.Modulo(left, right);
          break;
      }
    }

    private void CallFunction(ref Chunk chunk, ref int pc, ref uint baseRegister,
                              Instruction instr) {
      int calleeRegister = (int)(baseRegister + instr.A);
      var callee = _stack[calleeRegister].AsFunction();
      switch (callee) {
        case HeapObject.NativeFunction nativeFunc:
          _stack[calleeRegister] = nativeFunc.Call(_stack, calleeRegister + 1, (int)instr.B, _sys);
          break;
        case Function func:
          baseRegister += instr.A + 1;
          _callStack.PushFunc(func, baseRegister, pc);
          chunk = func.Chunk;
          pc = -1;
          break;
      }
    }

    // Gets the register value or constant value according to rkPos. Returns a readonly reference to
    // avoid copying.
    private ref readonly Value ValueOfRK(Chunk chunk, uint rkPos, uint baseRegister) {
      if (rkPos < Chunk.MaxRegisterCount) {
        return ref _stack[baseRegister + rkPos];
      }
      return ref chunk.ValueOfConstId(rkPos);
    }
  }
}
