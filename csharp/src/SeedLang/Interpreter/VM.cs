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
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The SeedLang virtual machine to run bytecode stored in a chunk.
  internal class VM {
    public readonly GlobalEnvironment Env = new GlobalEnvironment(NativeFunctions.Funcs);

    // The stack size. Each function can allocate maximun 250 registers in the stack. So the stack
    // can hold maximun 100 recursive function calls.
    private const int _stackSize = 25 * 1024;

    private readonly VisualizerCenter _visualizerCenter;
    private readonly Value[] _stack = new Value[_stackSize];
    private CallStack _callStack;

    internal VM(VisualizerCenter visualizerCenter = null) {
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
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
            case Opcode.LOADBOOL:
              // TODO: implement the LOADBOOL opcode.
              break;
            case Opcode.LOADK:
              _stack[baseRegister + instr.A] = chunk.ValueOfConstId(instr.Bx);
              break;
            case Opcode.NEWTUPLE:
              var tuple = new Value[instr.C];
              for (int i = 0; i < instr.C; i++) {
                tuple[i] = _stack[baseRegister + instr.B + i];
              }
              _stack[baseRegister + instr.A] = new Value(tuple);
              break;
            case Opcode.NEWLIST:
              var list = new List<Value>((int)instr.C);
              for (int i = 0; i < instr.C; i++) {
                list.Add(_stack[baseRegister + instr.B + i]);
              }
              _stack[baseRegister + instr.A] = new Value(list);
              break;
            case Opcode.GETGLOB:
              _stack[baseRegister + instr.A] = Env.GetVariable(instr.Bx);
              break;
            case Opcode.SETGLOB:
              Env.SetVariable(instr.Bx, _stack[baseRegister + instr.A]);
              // TODO: it's hard to send assignment notification in the VM, because it's hard to
              // distinguish assignment to local variable or temporary variables, and the name
              // information of the variables have been removed during compilation. Decide if this
              // kind of notification is needed, or if other kinds of notification can replace it.
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
              HandleBinary(chunk, instr, baseRegister, chunk.Ranges[pc]);
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
            case Opcode.EQ:
              if (ValueOfRK(chunk, instr.B, baseRegister).AsNumber() ==
                  ValueOfRK(chunk, instr.C, baseRegister).AsNumber() == (instr.A == 1)) {
                pc++;
              }
              break;
            case Opcode.LT:
              if ((ValueOfRK(chunk, instr.B, baseRegister).AsNumber() <
                   ValueOfRK(chunk, instr.C, baseRegister).AsNumber()) == (instr.A == 1)) {
                pc++;
              }
              break;
            case Opcode.LE:
              if ((ValueOfRK(chunk, instr.B, baseRegister).AsNumber() <=
                   ValueOfRK(chunk, instr.C, baseRegister).AsNumber()) == (instr.A == 1)) {
                pc++;
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
            case Opcode.EVAL:
              if (!_visualizerCenter.EvalPublisher.IsEmpty()) {
                var ee = new EvalEvent(new ValueWrapper(_stack[baseRegister + instr.A]),
                                       chunk.Ranges[pc]);
                _visualizerCenter.EvalPublisher.Notify(ee);
              }
              break;
            case Opcode.FORPREP:
              _stack[baseRegister + instr.A] = new Value(
                  ValueHelper.Subtract(_stack[baseRegister + instr.A],
                                       _stack[baseRegister + instr.A + 2]));
              pc += instr.SBx;
              break;
            case Opcode.FORLOOP:
              _stack[baseRegister + instr.A] = new Value(
                  ValueHelper.Add(_stack[baseRegister + instr.A],
                                  _stack[baseRegister + instr.A + 2]));
              if (_stack[baseRegister + instr.A].AsNumber() <
                  _stack[baseRegister + instr.A + 1].AsNumber()) {
                pc += instr.SBx;
              }
              break;
            case Opcode.CALL:
              CallFunction(ref chunk, ref pc, ref baseRegister, instr, chunk.Ranges[pc]);
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
      double index = ValueOfRK(chunk, instr.C, baseRegister).AsNumber();
      _stack[baseRegister + instr.A] = _stack[baseRegister + instr.B][index];
    }

    private void SetElement(Chunk chunk, Instruction instr, uint baseRegister) {
      double index = ValueOfRK(chunk, instr.B, baseRegister).AsNumber();
      _stack[baseRegister + instr.A][index] = ValueOfRK(chunk, instr.C, baseRegister);
    }

    private void HandleBinary(Chunk chunk, Instruction instr, uint baseRegister, Range range) {
      BinaryOperator op = BinaryOperator.Add;
      double result = 0;
      Value left = ValueOfRK(chunk, instr.B, baseRegister);
      Value right = ValueOfRK(chunk, instr.C, baseRegister);
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
      _stack[baseRegister + instr.A] = new Value(result);
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(new ValueWrapper(left), op, new ValueWrapper(right),
                                 new ValueWrapper(_stack[baseRegister + instr.A]), range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    private void CallFunction(ref Chunk chunk, ref int pc, ref uint baseRegister,
                              Instruction instr, Range range) {
      int calleeRegister = (int)(baseRegister + instr.A);
      var callee = _stack[calleeRegister].AsFunction();
      switch (callee) {
        case HeapObject.NativeFunction nativeFunc:
          _stack[calleeRegister] = nativeFunc.Call(_stack, calleeRegister + 1, (int)instr.B,
                                                   _visualizerCenter, range);
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
