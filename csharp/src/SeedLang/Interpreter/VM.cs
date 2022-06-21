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

    private readonly Sys _sys = new Sys();
    private readonly Registers _registers = new Registers();
    private readonly VisualizerCenter _visualizerCenter;

    public GlobalEnvironment Env { get; } = new GlobalEnvironment(NativeFunctions.Funcs.Values);

    public bool IsRunning => _state == State.Running;
    public bool IsPaused => _state == State.Paused;
    public bool IsStopped => _state == State.Stopped;

    private State _state = State.Stopped;

    private CallStack _callStack;
    private Chunk _chunk;
    private int _pc;

    // The hash table to store defined global variable names.
    private HashSet<string> _globals;

    internal VM(VisualizerCenter visualizerCenter) {
      _visualizerCenter = visualizerCenter;
    }

    internal void RedirectStdout(TextWriter stdout) {
      _sys.Stdout = stdout;
    }

    internal bool GetGlobals(out IReadOnlyList<IVM.VariableInfo> globals) {
      if (!_visualizerCenter.IsVariableTrackingEnabled) {
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
      if (!_visualizerCenter.IsVariableTrackingEnabled) {
        locals = new List<IVM.VariableInfo>();
        return false;
      }
      locals = _registers.Locals;
      return true;
    }

    internal void Run(Function func) {
      Debug.Assert(!IsRunning, "VM shall not be running.");
      if (!IsStopped) {
        Stop();
      }
      _state = State.Running;

      _callStack = new CallStack();
      _registers.Reset();
      _callStack.PushFunc(func, _registers.Base, 0);
      _chunk = func.Chunk;
      _pc = 0;
      _globals = new HashSet<string>();
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

    internal void HandleAssignment(Notification.Assignment assign) {
      var target = new LValue(new Variable(assign.Name, ToVariableType(assign.Type)));
      _visualizerCenter.Notify(new Event.Assignment(target, MakeRValue(assign.ValueId),
                                                    _chunk.Ranges[_pc]));
    }

    internal void HandleBinary(Notification.Binary binary) {
      var left = MakeRValue(binary.LeftId);
      var right = MakeRValue(binary.RightId);
      _visualizerCenter.Notify(new Event.Binary(left, binary.Op, right,
                                                new Value(ValueOfRK(binary.ResultId)),
                                                _chunk.Ranges[_pc]));
    }

    internal void HandleComparison(Notification.Comparison comparison) {
      var left = MakeRValue(comparison.LeftId);
      var right = MakeRValue(comparison.RightId);
      bool result = comparison.Op switch {
        ComparisonOperator.Less =>
            ValueHelper.Less(left.Value.GetRawValue(), right.Value.GetRawValue()),
        ComparisonOperator.Greater =>
            !ValueHelper.LessEqual(left.Value.GetRawValue(), right.Value.GetRawValue()),
        ComparisonOperator.LessEqual =>
            ValueHelper.LessEqual(left.Value.GetRawValue(), right.Value.GetRawValue()),
        ComparisonOperator.GreaterEqual =>
            !ValueHelper.Less(left.Value.GetRawValue(), right.Value.GetRawValue()),
        ComparisonOperator.EqEqual => left.Value.GetRawValue() == right.Value.GetRawValue(),
        ComparisonOperator.NotEqual => left.Value.GetRawValue() != right.Value.GetRawValue(),
        ComparisonOperator.In =>
            ValueHelper.Contains(right.Value.GetRawValue(), left.Value.GetRawValue()),
        _ => throw new NotImplementedException($"Unsupported comparison operator {comparison.Op}"),
      };
      _visualizerCenter.Notify(new Event.Comparison(left, comparison.Op, right,
                                                    new Value(new VMValue(result)),
                                                    _chunk.Ranges[_pc]));
    }

    internal void HandleElementLoaded(Notification.ElementLoaded elemLoaded) {
      if (!_registers.GetRegisterInfo(elemLoaded.TargetId).IsLocal) {
        var key = new Value(ValueOfRK(elemLoaded.KeyId));
        Registers.RegisterInfo container = _registers.GetRegisterInfo(elemLoaded.ContainerId);
        if (container.IsLocal) {
          _registers.SetRefRegisterInfoAt(elemLoaded.TargetId, container.Name, VariableType.Local,
                                          new List<Value> { key });
        } else if (container.IsReference) {
          var keys = container.Keys.ToList();
          keys.Add(key);
          _registers.SetRefRegisterInfoAt(elemLoaded.TargetId, container.Name,
                                          container.RefVariableType, keys);
        }
      }
    }

    internal void HandleFunction(Notification.Function func) {
      Instruction instr = _chunk.Bytecode[_pc];
      Debug.Assert(Enum.IsDefined(typeof(Notification.Function.Status), instr.A));
      switch ((Notification.Function.Status)instr.A) {
        case Notification.Function.Status.Called:
          var args = new Value[func.ArgLength];
          uint argStartId = func.FuncId + 1;
          for (uint i = 0; i < func.ArgLength; i++) {
            args[i] = new Value(ValueOfRK(argStartId + i));
          }
          _visualizerCenter.Notify(new Event.FuncCalled(func.Name, args, _chunk.Ranges[_pc]));
          break;
        case Notification.Function.Status.Returned:
          _visualizerCenter.Notify(new Event.FuncReturned(func.Name,
                                                          new Value(ValueOfRK(func.FuncId)),
                                                          _chunk.Ranges[_pc]));
          break;
      }
    }

    internal void HandleGlobalLoaded(Notification.GlobalLoaded globalLoaded) {
      _registers.SetRefRegisterInfoAt(globalLoaded.TargetId, globalLoaded.Name, VariableType.Global,
                                      new List<Value>());
    }

    internal void HandleSingleStep(Notification.SingleStep _) {
      _visualizerCenter.Notify(new Event.SingleStep(_chunk.Ranges[_pc]));
    }

    internal void HandleSubscriptAssignment(Notification.SubscriptAssignment assign) {
      Registers.RegisterInfo container = _registers.GetRegisterInfo(assign.ContainerId);
      if (!container.IsTemporary) {
        var keys = container.Keys.ToList();
        keys.Add(new Value(ValueOfRK(assign.KeyId)));
        var type = ToVariableType(container.RefVariableType);
        var target = new LValue(new Variable(container.Name, type), keys);
        _visualizerCenter.Notify(new Event.Assignment(target, MakeRValue(assign.ValueId),
                                                      _chunk.Ranges[_pc]));
      }
    }

    internal void HandleTempRegisterAllocated(Notification.TempRegisterAllocated temp) {
      _registers.SetTempRegisterInfoAt(temp.Id);
    }

    internal void HandleVariableDefined(Notification.VariableDefined variableDefined) {
      bool isFirstTimeDefined = false;
      switch (variableDefined.Info.Type) {
        case VariableType.Global:
          // for i in range(5):
          //   for j in range(5):
          //     ...
          // Global variable j will be defined for several times. Only adds it in the first time.
          if (!_globals.Contains(variableDefined.Info.Name)) {
            isFirstTimeDefined = true;
            _globals.Add(variableDefined.Info.Name);
          }
          break;
        case VariableType.Local:
          if (!_registers.GetRegisterInfo(variableDefined.Info.Id).IsLocal) {
            isFirstTimeDefined = true;
            _registers.SetLocalRegisterInfoAt(variableDefined.Info.Id, variableDefined.Info.Name);
          }
          break;
      }
      if (isFirstTimeDefined && _visualizerCenter.HasVisualizer<Event.VariableDefined>()) {
        _visualizerCenter.Notify(new Event.VariableDefined(variableDefined.Info.Name,
                                                           ToVariableType(variableDefined.Info.Type),
                                                           _chunk.Ranges[_pc]));
      }
    }

    internal void HandleVariableDeleted(Notification.VariableDeleted variableDeleted) {
      _registers.DeleteRegisterInfoFrom(variableDeleted.StartId, localInfo => {
        if (_visualizerCenter.HasVisualizer<Event.VariableDeleted>()) {
          _visualizerCenter.Notify(new Event.VariableDeleted(localInfo.Name,
                                                             Visualization.VariableType.Local,
                                                             _chunk.Ranges[_pc]));
        }
      });
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
          _visualizerCenter.Notify(new Event.VTagEntered(vTags, range));
          break;
        case Notification.VTag.Status.Exited:
          _visualizerCenter.Notify(new Event.VTagExited(vTags, range));
          break;
      }
    }

    private void RunLoop() {
      while (_pc < _chunk.Bytecode.Count) {
        Instruction instr = _chunk.Bytecode[_pc];
        try {
          switch (instr.Opcode) {
            case Opcode.MOVE:
              _registers.SetValueAt(instr.A, _registers.GetValueAt(instr.B));
              break;
            case Opcode.LOADNIL:
              for (uint i = 0; i < instr.B; i++) {
                _registers.SetValueAt(instr.A + i, new VMValue());
              }
              break;
            case Opcode.LOADBOOL:
              _registers.SetValueAt(instr.A, new VMValue(instr.B == 1));
              if (instr.C == 1) {
                _pc++;
              }
              break;
            case Opcode.LOADK:
              _registers.SetValueAt(instr.A, _chunk.ValueOfConstId(instr.Bx));
              break;
            case Opcode.NEWTUPLE:
              var builder = ImmutableArray.CreateBuilder<VMValue>((int)instr.C);
              for (uint i = 0; i < instr.C; i++) {
                builder.Add(_registers.GetValueAt(instr.B + i));
              }
              _registers.SetValueAt(instr.A, new VMValue(builder.MoveToImmutable()));
              break;
            case Opcode.NEWLIST:
              var list = new List<VMValue>((int)instr.C);
              for (uint i = 0; i < instr.C; i++) {
                list.Add(_registers.GetValueAt(instr.B + i));
              }
              _registers.SetValueAt(instr.A, new VMValue(list));
              break;
            case Opcode.NEWDICT:
              int count = (int)instr.C / 2;
              var dict = new Dictionary<VMValue, VMValue>(count);
              for (uint i = 0; i < count; i++) {
                uint keyRegister = instr.B + i * 2;
                dict[_registers.GetValueAt(keyRegister)] = _registers.GetValueAt(keyRegister + 1);
              }
              _registers.SetValueAt(instr.A, new VMValue(dict));
              break;
            case Opcode.GETGLOB:
              _registers.SetValueAt(instr.A, Env.GetVariable(instr.Bx));
              break;
            case Opcode.SETGLOB:
              Env.SetVariable(instr.Bx, _registers.GetValueAt(instr.A));
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
              _registers.SetValueAt(instr.A, new VMValue(-ValueOfRK(instr.B).AsNumber()));
              break;
            case Opcode.LEN:
              _registers.SetValueAt(instr.A, new VMValue(_registers.GetValueAt(instr.B).Length));
              break;
            case Opcode.JMP:
              _pc += instr.SBx;
              break;
            case Opcode.EQ: {
                bool result = ValueOfRK(instr.B) == ValueOfRK(instr.C);
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
              if (_registers.GetValueAt(instr.A).AsBoolean() == (instr.C == 1)) {
                _pc++;
              }
              break;
            case Opcode.TESTSET:
              // TODO: implement the TESTSET opcode.
              break;
            case Opcode.FORPREP: {
                const uint stepOffset = 2;
                VMValue loop = ValueHelper.Subtract(_registers.GetValueAt(instr.A),
                                                    _registers.GetValueAt(instr.A + stepOffset));
                _registers.SetValueAt(instr.A, loop);
                _pc += instr.SBx;
                break;
              }
            case Opcode.FORLOOP: {
                const uint limitOffset = 1;
                const uint stepOffset = 2;
                VMValue loop = ValueHelper.Add(_registers.GetValueAt(instr.A),
                                               _registers.GetValueAt(instr.A + stepOffset));
                _registers.SetValueAt(instr.A, loop);
                if (loop.AsNumber() < _registers.GetValueAt(instr.A + limitOffset).AsNumber()) {
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
      _registers.SetValueAt(instr.A, _registers.GetValueAt(instr.B)[ValueOfRK(instr.C)]);
    }

    private void SetElement(Instruction instr) {
      _registers.GetValueAt(instr.A)[ValueOfRK(instr.B)] = ValueOfRK(instr.C);
    }

    private void HandleBinary(Instruction instr) {
      switch (instr.Opcode) {
        case Opcode.ADD:
          _registers.SetValueAt(instr.A, ValueHelper.Add(ValueOfRK(instr.B), ValueOfRK(instr.C)));
          break;
        case Opcode.SUB:
          _registers.SetValueAt(instr.A, ValueHelper.Subtract(ValueOfRK(instr.B),
                                                              ValueOfRK(instr.C)));
          break;
        case Opcode.MUL:
          _registers.SetValueAt(instr.A, ValueHelper.Multiply(ValueOfRK(instr.B),
                                                              ValueOfRK(instr.C)));
          break;
        case Opcode.DIV:
          _registers.SetValueAt(instr.A, ValueHelper.Divide(ValueOfRK(instr.B),
                                                            ValueOfRK(instr.C)));
          break;
        case Opcode.FLOORDIV:
          _registers.SetValueAt(instr.A, ValueHelper.FloorDivide(ValueOfRK(instr.B),
                                                                 ValueOfRK(instr.C)));
          break;
        case Opcode.POW:
          _registers.SetValueAt(instr.A, ValueHelper.Power(ValueOfRK(instr.B), ValueOfRK(instr.C)));
          break;
        case Opcode.MOD:
          _registers.SetValueAt(instr.A, ValueHelper.Modulo(ValueOfRK(instr.B),
                                                            ValueOfRK(instr.C)));
          break;
      }
    }

    private void CallFunc(Instruction instr) {
      var callee = _registers.GetValueAt(instr.A).AsFunction();
      switch (callee) {
        case NativeFunction nativeFunc:
          VMValue result = nativeFunc.Call(_registers.GetArguments(instr.A, (int)instr.B), _sys);
          _registers.SetValueAt(instr.A, result);
          break;
        case Function func:
          _registers.Base += instr.A + 1;
          _callStack.PushFunc(func, _registers.Base, _pc);
          _chunk = func.Chunk;
          _pc = -1;
          break;
        default:
          throw new NotImplementedException("");
      }
    }

    private void ReturnFromFunc(Instruction instr) {
      // TODO: only support one return value now.
      _registers.SetReturnValue(instr.B > 0 ? _registers.GetValueAt(instr.A) : new VMValue());
      _callStack.PopFunc();
      Debug.Assert(!_callStack.IsEmpty);
      _chunk = _callStack.CurrentChunk();
      _registers.Base = _callStack.CurrentBase();
      _pc = _callStack.CurrentPC();
    }

    private RValue MakeRValue(uint operandId) {
      var value = new Value(ValueOfRK(operandId));
      if (IsRegisterId(operandId)) {
        Registers.RegisterInfo info = _registers.GetRegisterInfo(operandId);
        if (!info.IsTemporary) {
          VariableType type = info.IsLocal ? VariableType.Local : info.RefVariableType;
          var variable = new Variable(info.Name, ToVariableType(type));
          if (info.Keys.Count > 0) {
            return new RValue(variable, info.Keys, value);
          } else {
            return new RValue(variable, value);
          }
        }
      }
      return new RValue(value);
    }

    // Gets the register value or constant value according to rkPos. Returns a readonly reference to
    // avoid copying.
    private ref readonly VMValue ValueOfRK(uint rkId) {
      if (IsRegisterId(rkId)) {
        return ref _registers.GetValueAt(rkId);
      }
      return ref _chunk.ValueOfConstId(rkId);
    }

    private static bool IsRegisterId(uint rkId) {
      return rkId < Chunk.MaxRegisterCount;
    }

    private static Visualization.VariableType ToVariableType(VariableType type) {
      return type switch {
        VariableType.Global => Visualization.VariableType.Global,
        VariableType.Local => Visualization.VariableType.Local,
        _ => throw new NotImplementedException($"Unsupported variable type {type}."),
      };
    }
  }
}
