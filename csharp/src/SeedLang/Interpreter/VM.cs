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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  // The SeedLang virtual machine to run bytecode stored in a chunk.
  internal class VM {
    private enum State {
      Running,
      Paused,
      Stopped,
    }

    // The class to store variable information of registers.
    private class RegisterInfo {
      public string Name { get; }

      internal RegisterInfo(string name) {
        Name = name;
      }
    }

    public GlobalEnvironment Env { get; } = new GlobalEnvironment(NativeFunctions.Funcs.Values);
    public VisualizerCenter VisualizerCenter { get; } = new VisualizerCenter();

    public bool IsRunning => _state == State.Running;
    public bool IsPaused => _state == State.Paused;
    public bool IsStopped => _state == State.Stopped;

    private State _state = State.Stopped;

    private readonly Sys _sys = new Sys();

    // The stack size. Each function can allocate maximun 250 registers in the stack. So the stack
    // can hold maximun 100 recursive function calls.
    private const int _stackSize = 25 * 1024;

    private readonly VMValue[] _stack = new VMValue[_stackSize];
    private CallStack _callStack;
    private Chunk _chunk;
    private uint _baseRegister;
    private int _pc;

    // The hash table to store defined global variable names.
    private HashSet<string> _globals;
    // The list to store variable information of registers.
    private List<RegisterInfo> _registerInfos;

    internal void RedirectStdout(TextWriter stdout) {
      _sys.Stdout = stdout;
    }

    internal bool GetGlobals(out IReadOnlyList<IVM.VariableInfo> globals) {
      if (!VisualizerCenter.VariableTrackingEnabled) {
        globals = new List<IVM.VariableInfo>();
        return false;
      }
      var globalList = new List<IVM.VariableInfo>();
      foreach (string name in _globals) {
        if (Env.FindVariable(name) is uint id) {
          globalList.Add(new IVM.VariableInfo(name, new Value(Env.GetVariable(id))));
        }
      }
      globals = globalList;
      return true;
    }

    internal bool GetLocals(out IReadOnlyList<IVM.VariableInfo> locals) {
      if (!VisualizerCenter.VariableTrackingEnabled) {
        locals = new List<IVM.VariableInfo>();
        return false;
      }
      var localList = new List<IVM.VariableInfo>();
      for (int i = (int)_baseRegister; i < _registerInfos.Count; i++) {
        if (_registerInfos[i] is RegisterInfo info) {
          localList.Add(new IVM.VariableInfo(info.Name, new Value(_stack[i])));
        }
      }
      locals = localList;
      return true;
    }

    internal void Run(Function func) {
      Debug.Assert(!IsRunning, "VM shall not be running.");
      if (!IsStopped) {
        Stop();
      }
      _state = State.Running;

      _callStack = new CallStack();
      _baseRegister = 0;
      _callStack.PushFunc(func, _baseRegister, 0);
      _chunk = func.Chunk;
      _pc = 0;
      _globals = new HashSet<string>();
      _registerInfos = new List<RegisterInfo>();
      RunLoop();
    }

    internal void Pause() {
      Debug.Assert(IsRunning, "VM shall be running.");
      _state = State.Paused;
      _chunk.SetBreakpointAt(_pc + 1);
    }

    internal void Continue() {
      Debug.Assert(IsPaused, "VM shall be paused.");
      _state = State.Running;
      _chunk.RestoreBreakpoint();
      RunLoop();
    }

    internal void Stop() {
      _state = State.Stopped;
      _chunk.RestoreBreakpoint();
    }

    internal void Notify<Event>(Event e) {
      var vmProxy = new VMProxy(this);
      VisualizerCenter.Notify(e, vmProxy);
      vmProxy.Invalid();
    }

    private void RunLoop() {
      while (_pc < _chunk.Bytecode.Count) {
        Instruction instr = _chunk.Bytecode[_pc];
        try {
          switch (instr.Opcode) {
            case Opcode.MOVE:
              _stack[_baseRegister + instr.A] = _stack[_baseRegister + instr.B];
              break;
            case Opcode.LOADNIL:
              for (int i = 0; i < instr.B; i++) {
                _stack[_baseRegister + instr.A + i] = new VMValue();
              }
              break;
            case Opcode.LOADBOOL:
              _stack[_baseRegister + instr.A] = new VMValue(instr.B == 1);
              if (instr.C == 1) {
                _pc++;
              }
              break;
            case Opcode.LOADK:
              _stack[_baseRegister + instr.A] = _chunk.ValueOfConstId(instr.Bx);
              break;
            case Opcode.NEWTUPLE:
              var builder = ImmutableArray.CreateBuilder<VMValue>((int)instr.C);
              for (int i = 0; i < instr.C; i++) {
                builder.Add(_stack[_baseRegister + instr.B + i]);
              }
              _stack[_baseRegister + instr.A] = new VMValue(builder.MoveToImmutable());
              break;
            case Opcode.NEWLIST:
              var list = new List<VMValue>((int)instr.C);
              for (int i = 0; i < instr.C; i++) {
                list.Add(_stack[_baseRegister + instr.B + i]);
              }
              _stack[_baseRegister + instr.A] = new VMValue(list);
              break;
            case Opcode.NEWDICT:
              int count = (int)instr.C / 2;
              var dict = new Dictionary<VMValue, VMValue>(count);
              uint dictRegister = _baseRegister + instr.A;
              uint kvStart = _baseRegister + instr.B;
              for (uint i = 0; i < count; i++) {
                uint keyRegister = kvStart + i * 2;
                dict[_stack[keyRegister]] = _stack[keyRegister + 1];
              }
              _stack[dictRegister] = new VMValue(dict);
              break;
            case Opcode.GETGLOB:
              _stack[_baseRegister + instr.A] = Env.GetVariable(instr.Bx);
              break;
            case Opcode.SETGLOB:
              Env.SetVariable(instr.Bx, _stack[_baseRegister + instr.A]);
              break;
            case Opcode.GETELEM:
              GetElement(instr);
              break;
            case Opcode.SETELEM:
              SetElement(instr);
              break;
            case Opcode.ADD:
            case Opcode.SUB:
            case Opcode.MUL:
            case Opcode.DIV:
            case Opcode.FLOORDIV:
            case Opcode.POW:
            case Opcode.MOD:
              HandleBinary(instr);
              break;
            case Opcode.UNM:
              _stack[_baseRegister + instr.A] = new VMValue(-ValueOfRK(instr.B).AsNumber());
              break;
            case Opcode.LEN:
              _stack[_baseRegister + instr.A] = new VMValue(_stack[_baseRegister + instr.B].Length);
              break;
            case Opcode.JMP:
              _pc += instr.SBx;
              break;
            case Opcode.EQ: {
                bool result = ValueOfRK(instr.B).Equals(ValueOfRK(instr.C));
                if (result == (instr.A == 1)) {
                  _pc++;
                }
              }
              break;
            case Opcode.LT: {
                bool result = ValueHelper.Less(ValueOfRK(instr.B), ValueOfRK(instr.C));
                if (result == (instr.A == 1)) {
                  _pc++;
                }
              }
              break;
            case Opcode.LE: {
                bool result = ValueHelper.LessEqual(ValueOfRK(instr.B), ValueOfRK(instr.C));
                if (result == (instr.A == 1)) {
                  _pc++;
                }
              }
              break;
            case Opcode.IN: {
                bool result = ValueHelper.Contains(ValueOfRK(instr.C), ValueOfRK(instr.B));
                if (result == (instr.A == 1)) {
                  _pc++;
                }
              }
              break;
            case Opcode.TEST:
              if (_stack[_baseRegister + instr.A].AsBoolean() == (instr.C == 1)) {
                _pc++;
              }
              break;
            case Opcode.TESTSET:
              // TODO: implement the TESTSET opcode.
              break;
            case Opcode.FORPREP: {
                uint loopReg = _baseRegister + instr.A;
                const uint stepOff = 2;
                _stack[loopReg] = ValueHelper.Subtract(_stack[loopReg], _stack[loopReg + stepOff]);
                _pc += instr.SBx;
                break;
              }
            case Opcode.FORLOOP: {
                uint loopReg = _baseRegister + instr.A;
                const uint limitOff = 1;
                const uint stepOff = 2;
                _stack[loopReg] = ValueHelper.Add(_stack[loopReg], _stack[loopReg + stepOff]);
                if (_stack[loopReg].AsNumber() < _stack[loopReg + limitOff].AsNumber()) {
                  _pc += instr.SBx;
                }
                break;
              }
            case Opcode.CALL:
              CallFunc(instr);
              break;
            case Opcode.RETURN:
              ReturnFromFunc(instr);
              break;
            case Opcode.VISNOTIFY:
              HandleVisNotify(instr);
              if (IsStopped) {
                // The execution is stopped by the visualizers.
                return;
              }
              break;
            case Opcode.HALT:
              _state = instr.A == (uint)HaltReason.Breakpoint ? State.Paused : State.Stopped;
              return;
            default:
              _state = State.Stopped;
              throw new NotImplementedException($"Unimplemented opcode: {instr.Opcode}");
          }
        } catch (DiagnosticException ex) {
          _state = State.Stopped;
          throw new DiagnosticException(SystemReporters.SeedVM, ex.Diagnostic.Severity,
                                        ex.Diagnostic.Module, _chunk.Ranges[_pc],
                                        ex.Diagnostic.MessageId);
        }
        _pc++;
      }
    }

    private void GetElement(Instruction instr) {
      VMValue index = ValueOfRK(instr.C);
      _stack[_baseRegister + instr.A] = _stack[_baseRegister + instr.B][index];
    }

    private void SetElement(Instruction instr) {
      VMValue index = ValueOfRK(instr.B);
      _stack[_baseRegister + instr.A][index] = ValueOfRK(instr.C);
    }

    private void HandleBinary(Instruction instr) {
      VMValue left = ValueOfRK(instr.B);
      VMValue right = ValueOfRK(instr.C);
      uint register = _baseRegister + instr.A;
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

    private void CallFunc(Instruction instr) {
      int calleeRegister = (int)(_baseRegister + instr.A);
      var callee = _stack[calleeRegister].AsFunction();
      switch (callee) {
        case NativeFunction nativeFunc:
          _stack[calleeRegister] = nativeFunc.Call(_stack, calleeRegister + 1, (int)instr.B, _sys);
          break;
        case Function func:
          _baseRegister += instr.A + 1;
          _callStack.PushFunc(func, _baseRegister, _pc);
          _chunk = func.Chunk;
          _pc = -1;
          break;
      }
    }

    private void ReturnFromFunc(Instruction instr) {
      // TODO: only support one return value now.
      if (_baseRegister > 0) {
        uint returnRegister = _baseRegister - 1;
        _stack[returnRegister] = instr.B > 0 ? _stack[_baseRegister + instr.A] :
                                               new VMValue();
      }
      _callStack.PopFunc();
      Debug.Assert(!_callStack.IsEmpty);
      _chunk = _callStack.CurrentChunk();
      _baseRegister = _callStack.CurrentBase();
      _pc = _callStack.CurrentPC();
    }

    private void HandleVisNotify(Instruction instr) {
      var notification = _chunk.Notifications[(int)instr.Bx];
      switch (notification) {
        case Notification.VariableDefined defined:
          switch (defined.Info.Type) {
            case VariableType.Global:
              Debug.Assert(!_globals.Contains(defined.Info.Name));
              _globals.Add(defined.Info.Name);
              break;
            case VariableType.Local:
              for (int i = _registerInfos.Count; i < _baseRegister + defined.Info.Id; i++) {
                _registerInfos.Add(null);
              }
              _registerInfos.Add(new RegisterInfo(defined.Info.Name));
              break;
          }
          break;
        case Notification.VariableDeleted deleted:
          for (int i = _registerInfos.Count - 1; i >= _baseRegister + deleted.StartId; i--) {
            if (!(_registerInfos[i] is null)) {
              Notify(new Event.VariableDeleted(_registerInfos[i].Name, VariableType.Local,
                                               _chunk.Ranges[_pc]));
            }
          }
          return;
      }
      if (!(notification is Notification.VariableDefined) ||
          VisualizerCenter.HasVisualizer<Event.VariableDefined>()) {
        notification.Notify(this, (uint id) => { return ValueOfRK(id); }, instr.A,
                            _chunk.Ranges[_pc]);
      }
    }

    // Gets the register value or constant value according to rkPos. Returns a readonly reference to
    // avoid copying.
    private ref readonly VMValue ValueOfRK(uint rkPos) {
      if (rkPos < Chunk.MaxRegisterCount) {
        return ref _stack[_baseRegister + rkPos];
      }
      return ref _chunk.ValueOfConstId(rkPos);
    }
  }
}
