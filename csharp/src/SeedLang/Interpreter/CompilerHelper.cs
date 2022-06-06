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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  // A helper class to encapsulate common functions of Compiler and ExprCompiler classes.
  internal class CompilerHelper {
    public uint LastRegister => _variableResolver.LastRegister;

    // The stack to collect positions of nested true and false jump bytecode for comparison and
    // boolean expressions.
    public ExprJumpStack ExprJumpStack { get; } = new ExprJumpStack();

    // The chunk on the top of the function stack.
    public Chunk Chunk => _nestedFuncStack.CurrentChunk();

    // The chunk cache on the top of the function stack.
    public ChunkCache Cache => _nestedFuncStack.CurrentChunkCache();

    private readonly VisualizerCenter _visualizerCenter;

    private readonly VariableResolver _variableResolver;

    private readonly NestedFuncStack _nestedFuncStack = new NestedFuncStack();

    // The flag to suspend emitting of notifications.
    private bool _suspendNotificationEmitting = false;

    internal CompilerHelper(VisualizerCenter visualizerCenter, GlobalEnvironment env) {
      _variableResolver = new VariableResolver(env);
      _visualizerCenter = visualizerCenter;
    }

    internal void PushMainFunc() {
      _nestedFuncStack.PushFunc("main");
    }

    internal Function PopMainFunc() {
      return _nestedFuncStack.PopFunc();
    }

    internal void PushFunc(string name) {
      _nestedFuncStack.PushFunc(name);
      _variableResolver.BeginFuncScope(name);
    }

    internal Function PopFunc() {
      _variableResolver.EndFuncScope();
      return _nestedFuncStack.PopFunc();
    }

    internal void BeginExprScope() {
      _variableResolver.BeginExprScope();
    }

    internal void EndExprScope() {
      _variableResolver.EndExprScope();
    }

    internal VariableInfo DefineVariable(string name, TextRange range) {
      VariableInfo info = _variableResolver.DefineVariable(name);
      EmitVariableDefinedNotification(info, range);
      return info;
    }

    internal VariableInfo FindVariable(string name) {
      return _variableResolver.FindVariable(name);
    }

    internal uint DefineTempVariable() {
      return _variableResolver.DefineTempVariable();
    }

    internal uint? GetRegisterOrConstantId(Expression expr) {
      if (GetRegisterId(expr) is uint registerId) {
        return registerId;
      } else if (GetConstantId(expr) is uint constantId) {
        return constantId;
      }
      return null;
    }

    internal uint? GetRegisterId(Expression expr) {
      if (expr is IdentifierExpression identifier &&
          _variableResolver.FindVariable(identifier.Name) is VariableInfo info &&
          info.Type == VariableType.Local) {
        return info.Id;
      }
      return null;
    }

    internal uint? GetConstantId(Expression expr) {
      return expr switch {
        NumberConstantExpression number => Cache.IdOfConstant(number.Value),
        StringConstantExpression str => Cache.IdOfConstant(str.Value),
        _ => null,
      };
    }

    internal void PatchJumpsToPos(List<int> jumps, int pos) {
      foreach (int jump in jumps) {
        PatchJumpToPos(jump, pos);
      }
      jumps.Clear();
    }

    internal void PatchJumpsToCurrentPos(List<int> jumps) {
      foreach (int jump in jumps) {
        PatchJumpToCurrentPos(jump);
      }
      jumps.Clear();
    }

    internal void PatchJumpToPos(int jump, int pos) {
      Chunk.PatchSBXAt(jump, pos - jump - 1);
    }

    internal void PatchJumpToCurrentPos(int jump) {
      PatchJumpToPos(jump, Chunk.Bytecode.Count);
    }

    // Emits a CALL instruction. A VISNOTIFY instruction is also emitted if there are visualizers
    // for the FuncCalled event.
    internal void EmitCall(string name, uint funcRegister, uint argLength, TextRange range) {
      bool isNormalFunc = !NativeFunctions.IsInternalFunction(name);
      bool notifyCalled = !_suspendNotificationEmitting && isNormalFunc &&
                          _visualizerCenter.HasVisualizer<Event.FuncCalled>();
      if (notifyCalled) {
        var nId = Cache.IdOfNotification(new Notification.Function(name, funcRegister, argLength));
        Emit(Opcode.VISNOTIFY, (uint)Notification.Function.Status.Called, nId, range);
      }
      Emit(Opcode.CALL, funcRegister, argLength, 0, range);
      bool notifyReturned = !_suspendNotificationEmitting && isNormalFunc &&
                            _visualizerCenter.HasVisualizer<Event.FuncReturned>();
      if (notifyReturned) {
        var nId = Cache.IdOfNotification(new Notification.Function(name, funcRegister, argLength));
        Emit(Opcode.VISNOTIFY, (uint)Notification.Function.Status.Returned, nId, range);
      }
      bool notifyVariableDeleted = !_suspendNotificationEmitting &&
                                   !NativeFunctions.IsNativeFunc(name) &&
                                   _visualizerCenter.IsVariableTrackingEnabled;
      if (notifyVariableDeleted) {
        var nId = Cache.IdOfNotification(new Notification.VariableDeleted(0));
        Emit(Opcode.VISNOTIFY, 0, nId, range);
      }
    }

    internal void EmitAssignNotification(string name, VariableType type, uint valueId,
                                         TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.HasVisualizer<Event.Assignment>()) {
        var n = new Notification.Assignment(name, type, valueId);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitBinaryNotification(uint leftId, BinaryOperator op, uint rightId,
                                         uint resultId, TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.HasVisualizer<Event.Binary>()) {
        var n = new Notification.Binary(leftId, op, rightId, resultId);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitGetElementNotification(uint targetId, uint containerId, uint keyId,
                                             TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.IsVariableTrackingEnabled) {
        var n = new Notification.ElementLoaded(targetId, containerId, keyId);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitGetGlobalNotification(uint targetId, string name, TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.IsVariableTrackingEnabled) {
        var n = new Notification.GlobalLoaded(targetId, name);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitSubscriptAssignNotification(uint containerId, uint keyId, uint valueId,
                                                  TextRange range) {
      if (!_suspendNotificationEmitting &&
          _visualizerCenter.HasVisualizer<Event.SubscriptAssignment>()) {
        var n = new Notification.SubscriptAssignment(containerId, keyId, valueId);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitUnaryNotification(UnaryOperator op, uint valueId, uint resultId,
                                        TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.HasVisualizer<Event.Unary>()) {
        var n = new Notification.Unary(op, valueId, resultId);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitVariableDefinedNotification(VariableInfo info, TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.IsVariableTrackingEnabled) {
        var n = new Notification.VariableDefined(info);
        Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), range);
      }
    }

    internal void EmitVTagEnteredNotification(VTagStatement vTag, ExprCompiler exprCompiler) {
      if (!_suspendNotificationEmitting && _visualizerCenter.HasVisualizer<Event.VTagEntered>()) {
        _suspendNotificationEmitting = true;
        Notification.VTagInfo[] vTagInfos = CollectVTagInfo(vTag, exprCompiler);
        var n = new Notification.VTag(vTagInfos);
        Emit(Opcode.VISNOTIFY, (uint)Notification.VTag.Status.Entered, Cache.IdOfNotification(n),
             vTag.Range);
        _suspendNotificationEmitting = false;
      }
    }

    internal void EmitVTagExitedNotification(VTagStatement vTag, ExprCompiler exprCompiler) {
      if (!_suspendNotificationEmitting && _visualizerCenter.HasVisualizer<Event.VTagExited>()) {
        _suspendNotificationEmitting = true;
        Notification.VTagInfo[] vTagInfos = CollectVTagInfo(vTag, exprCompiler);
        var n = new Notification.VTag(vTagInfos);
        Emit(Opcode.VISNOTIFY, (uint)Notification.VTag.Status.Exited, Cache.IdOfNotification(n),
             vTag.Range);
        _suspendNotificationEmitting = false;
      }
    }

    // Emits an ABC instruction to the chunk. Also emits a single step notification before each line
    // if there are visualizers for this event.
    internal void Emit(Opcode opcode, uint a, uint b, uint c, TextRange range) {
      TryEmitSingleStepNotification(range);
      Chunk.Emit(opcode, a, b, c, range);
    }

    // Emits an ABx instruction to the chunk. Also emits a single step notification before each line
    // if there are visualizers for this event.
    internal void Emit(Opcode opcode, uint a, uint bx, TextRange range) {
      TryEmitSingleStepNotification(range);
      Chunk.Emit(opcode, a, bx, range);
    }

    // Emits a SBx instruction to the chunk. Also emits a single step notification before each line
    // if there are visualizers for this event.
    internal void Emit(Opcode opcode, uint a, int sbx, TextRange range) {
      TryEmitSingleStepNotification(range);
      Chunk.Emit(opcode, a, sbx, range);
    }

    internal static Opcode OpcodeOfBinaryOperator(BinaryOperator op) {
      return op switch {
        BinaryOperator.Add => Opcode.ADD,
        BinaryOperator.Subtract => Opcode.SUB,
        BinaryOperator.Multiply => Opcode.MUL,
        BinaryOperator.Divide => Opcode.DIV,
        BinaryOperator.FloorDivide => Opcode.FLOORDIV,
        BinaryOperator.Power => Opcode.POW,
        BinaryOperator.Modulo => Opcode.MOD,
        _ => throw new NotImplementedException($"Operator {op} not implemented."),
      };
    }

    internal static (Opcode, bool) OpcodeAndCheckFlagOfComparisonOperator(ComparisonOperator op) {
      return op switch {
        ComparisonOperator.Less => (Opcode.LT, true),
        ComparisonOperator.Greater => (Opcode.LE, false),
        ComparisonOperator.LessEqual => (Opcode.LE, true),
        ComparisonOperator.GreaterEqual => (Opcode.LT, false),
        ComparisonOperator.EqEqual => (Opcode.EQ, true),
        ComparisonOperator.NotEqual => (Opcode.EQ, false),
        ComparisonOperator.In => (Opcode.IN, true),
        _ => throw new NotImplementedException($"Operator {op} not implemented."),
      };
    }

    private void TryEmitSingleStepNotification(TextRange range) {
      if (!_suspendNotificationEmitting && _visualizerCenter.HasVisualizer<Event.SingleStep>()) {
        if (range.Start.Line != _nestedFuncStack.CurrentSourceLineOfPrevBytecode()) {
          // Creates the text range to indicate the start of a single step source line.
          var eventRange = new TextRange(range.Start.Line, 0, range.Start.Line, 0);
          var n = new Notification.SingleStep();
          Chunk.Emit(Opcode.VISNOTIFY, 0, Cache.IdOfNotification(n), eventRange);
          _nestedFuncStack.SetCurrentSourceLineOfPrevBytecode(range.Start.Line);
        }
      }
    }

    private Notification.VTagInfo[] CollectVTagInfo(VTagStatement vTag, ExprCompiler exprCompiler) {
      BeginExprScope();
      var vTagInfos = Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
        var args = Array.ConvertAll(vTagInfo.Args, arg => arg.Text);
        var valueIds = new uint?[vTagInfo.Args.Length];
        for (int j = 0; j < vTagInfo.Args.Length; j++) {
          try {
            if (GetRegisterOrConstantId(vTagInfo.Args[j].Expr) is uint id) {
              valueIds[j] = id;
            } else {
              valueIds[j] = DefineTempVariable();
              exprCompiler.RegisterForSubExpr = valueIds[j].Value;
              exprCompiler.Visit(vTagInfo.Args[j].Expr);
            }
          } catch (DiagnosticException ex) {
            if (ex.Diagnostic.MessageId == Message.RuntimeErrorVariableNotDefined) {
              // Ignores variable not defined exception for variables in VTags and resets
              // RegisterForSubExpr.
              // TODO: The state of RegisterForSubExpr is error-prone. Consider using an IR to
              // optimize expression compilation algorithms.
              _ = exprCompiler.RegisterForSubExpr;
              valueIds[j] = null;
            } else {
              throw ex;
            }
          }
        }
        return new Notification.VTagInfo(vTagInfo.Name, args, valueIds);
      });
      EndExprScope();
      return vTagInfos;
    }
  }
}
