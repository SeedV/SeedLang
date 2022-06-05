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
using System.Linq;
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
      if (!VisualizerCenter.IsVariableTrackingEnabled) {
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
      if (!VisualizerCenter.IsVariableTrackingEnabled) {
        locals = new List<IVM.VariableInfo>();
        return false;
      }
      var localList = new List<IVM.VariableInfo>();
      for (int i = (int)_baseRegister; i < _registerInfos.Count; i++) {
        if (_registerInfos[i].IsLocal) {
          localList.Add(new IVM.VariableInfo(_registerInfos[i].Name, new Value(_stack[i])));
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

    internal void HandleAssignment(Notification.Assignment notification) {
      Notify(new Event.Assignment(notification.Name, notification.Type,
                                  new Value(ValueOfRK(notification.ValueId)), _chunk.Ranges[_pc]));
    }

    internal void HandleBinary(Notification.Binary binary) {
      Notify(new Event.Binary(new Value(ValueOfRK(binary.LeftId)), binary.Op,
                              new Value(ValueOfRK(binary.RightId)),
                              new Value(ValueOfRK(binary.ResultId)), _chunk.Ranges[_pc]));
    }

    internal void HandleElementLoaded(Notification.ElementLoaded elementLoaded) {
      var targetId = (int)elementLoaded.TargetId;
      var containerId = (int)elementLoaded.ContainerId;
      if (!_registerInfos[targetId].IsLocal) {
        var key = new Value(ValueOfRK(elementLoaded.KeyId));
        if (_registerInfos[containerId].IsLocal) {
          var keys = new List<Value> { key };
          var info = new RegisterInfo(_registerInfos[containerId].Name, VariableType.Local, keys);
          SetRegisterInfo(info, elementLoaded.TargetId);
        } else if (_registerInfos[containerId].IsReference) {
          var keys = _registerInfos[containerId].Keys.ToList();
          keys.Add(key);
          var info = new RegisterInfo(_registerInfos[containerId].Name, VariableType.Local, keys);
          SetRegisterInfo(info, elementLoaded.TargetId);
        }
      }
    }

    internal void HandleFunction(Notification.Function function) {
      Instruction instr = _chunk.Bytecode[_pc];
      Debug.Assert(Enum.IsDefined(typeof(Notification.Function.Status), instr.A));
      switch ((Notification.Function.Status)instr.A) {
        case Notification.Function.Status.Called:
          var args = new Value[function.ArgLength];
          uint argStartId = function.FuncId + 1;
          for (uint i = 0; i < function.ArgLength; i++) {
            args[i] = new Value(ValueOfRK(argStartId + i));
          }
          Notify(new Event.FuncCalled(function.Name, args, _chunk.Ranges[_pc]));
          break;
        case Notification.Function.Status.Returned:
          Notify(new Event.FuncReturned(function.Name, new Value(ValueOfRK(function.FuncId)),
                                        _chunk.Ranges[_pc]));
          break;
      }
    }

    internal void HandleGlobalLoaded(Notification.GlobalLoaded notification) {
      var info = new RegisterInfo(notification.Name, VariableType.Global, new List<Value>());
      SetRegisterInfo(info, notification.TargetId);
    }

    internal void HandleSingleStep(Notification.SingleStep _) {
      Notify(new Event.SingleStep(_chunk.Ranges[_pc]));
    }

    internal void HandleSubscriptAssignment(Notification.SubscriptAssignment assignment) {
      Notify(new Event.SubscriptAssignment(assignment.Name, assignment.Type,
                                           new Value(ValueOfRK(assignment.KeyId)),
                                           new Value(ValueOfRK(assignment.ValueId)),
                                           _chunk.Ranges[_pc]));
    }

    internal void HandleUnary(Notification.Unary unary) {
      Notify(new Event.Unary(unary.Op, new Value(ValueOfRK(unary.ValueId)),
                             new Value(ValueOfRK(unary.ResultId)), _chunk.Ranges[_pc]));
    }

    internal void HandleVariableDefined(Notification.VariableDefined vdn) {
      switch (vdn.Info.Type) {
        case VariableType.Global:
          Debug.Assert(!_globals.Contains(vdn.Info.Name));
          _globals.Add(vdn.Info.Name);
          break;
        case VariableType.Local:
          SetRegisterInfo(new RegisterInfo(vdn.Info.Name), vdn.Info.Id);
          break;
      }
      if (VisualizerCenter.HasVisualizer<Event.VariableDefined>()) {
        Notify(new Event.VariableDefined(vdn.Info.Name, vdn.Info.Type, _chunk.Ranges[_pc]));
      }
    }

    internal void HandleVariableDeleted(Notification.VariableDeleted vdn) {
      for (int i = _registerInfos.Count - 1; i >= _baseRegister + vdn.StartId; i--) {
        if (_registerInfos[i].IsLocal) {
          if (VisualizerCenter.HasVisualizer<Event.VariableDeleted>()) {
            var range = _chunk.Ranges[_pc];
            Notify(new Event.VariableDeleted(_registerInfos[i].Name, VariableType.Local, range));
          }
          _registerInfos[i] = new RegisterInfo();
        }
      }
    }

    internal void HandleVTag(Notification.VTag vTag) {
      var vTags = Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
        var values = Array.ConvertAll(vTagInfo.ValueIds, valueId =>
            valueId.HasValue ? new Value(ValueOfRK(valueId.Value)) : new Value());
        return new VTagInfo(vTagInfo.Name, vTagInfo.Args, values);
      });
      Instruction instr = _chunk.Bytecode[_pc];
      var range = _chunk.Ranges[_pc];
      Debug.Assert(Enum.IsDefined(typeof(Notification.VTag.Status), instr.A));
      switch ((Notification.VTag.Status)instr.A) {
        case Notification.VTag.Status.Entered:
          Notify(new Event.VTagEntered(vTags, range));
          break;
        case Notification.VTag.Status.Exited:
          Notify(new Event.VTagExited(vTags, range));
          break;
      }
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
              _chunk.Notifications[(int)instr.Bx].Accept(this);
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
      _stack[_baseRegister + instr.A] = _stack[_baseRegister + instr.B][ValueOfRK(instr.C)];
    }

    private void SetElement(Instruction instr) {
      _stack[_baseRegister + instr.A][ValueOfRK(instr.B)] = ValueOfRK(instr.C);
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

    private void SetRegisterInfo(RegisterInfo info, uint registerId) {
      int index = (int)(_baseRegister + registerId);
      if (index < _registerInfos.Count) {
        _registerInfos[index] = info;
      } else {
        for (int i = _registerInfos.Count; i < index; i++) {
          _registerInfos.Add(new RegisterInfo());
        }
        _registerInfos.Add(info);
      }
    }

    private void Notify<Event>(Event e) {
      var vmProxy = new VMProxy(this);
      VisualizerCenter.Notify(e, vmProxy);
      vmProxy.Invalid();
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
