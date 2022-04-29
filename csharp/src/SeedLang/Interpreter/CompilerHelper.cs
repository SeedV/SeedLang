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

namespace SeedLang.Interpreter {
  internal class CompilerHelper {
    // The chunk on the top of the function stack.
    public Chunk Chunk { get; set; }
    // The constant cache on the top of the function stack.
    public ConstantCache ConstantCache { get; set; }

    public uint LastRegister => _variableResolver.LastRegister;

    // TODO: refactor the NestedJumpStack and NestedLoopStack classes to extrat the similarities.
    public NestedJumpStack NestedJumpStack { get; } = new NestedJumpStack();

    private readonly VisualizerCenter _visualizerCenter;

    private readonly VariableResolver _variableResolver;

    // The source code line number (1-based) of the previous bytecode.
    private int _sourceLineOfPrevBytecode = 0;

    internal CompilerHelper(VisualizerCenter visualizerCenter, GlobalEnvironment env) {
      _variableResolver = new VariableResolver(env);
      _visualizerCenter = visualizerCenter;
    }

    internal void BeginBlockScope() {
      _variableResolver.BeginBlockScope();
    }

    internal void EndBlockScope() {
      _variableResolver.EndBlockScope();
    }

    internal void BeginFunctionScope() {
      _variableResolver.BeginFunctionScope();
    }

    internal void EndFunctionScope() {
      _variableResolver.EndFunctionScope();
    }

    internal void BeginExpressionScope() {
      _variableResolver.BeginExpressionScope();
    }

    internal void EndExpressionScope() {
      _variableResolver.EndExpressionScope();
    }

    internal VariableResolver.VariableInfo DefineVariable(string name) {
      return _variableResolver.DefineVariable(name);
    }

    internal VariableResolver.VariableInfo? FindVariable(string name) {
      return _variableResolver.FindVariable(name);
    }

    internal uint AllocateRegister() {
      return _variableResolver.AllocateRegister();
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
          _variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info &&
          info.Type == VariableResolver.VariableType.Local) {
        return info.Id;
      }
      return null;
    }

    internal uint? GetConstantId(Expression expr) {
      switch (expr) {
        case NumberConstantExpression number:
          return ConstantCache.IdOfConstant(number.Value);
        case StringConstantExpression str:
          return ConstantCache.IdOfConstant(str.Value);
        default:
          return null;
      }
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
      bool notifyCalled = isNormalFunc && _visualizerCenter.HasVisualizer<Event.FuncCalled>();
      bool notifyReturned = isNormalFunc && _visualizerCenter.HasVisualizer<Event.FuncReturned>();
      uint nId = 0;
      if (notifyCalled || notifyReturned) {
        var notification = new Notification.Function(name, funcRegister, argLength);
        nId = Chunk.AddNotification(notification);
      }
      if (notifyCalled) {
        Emit(Opcode.VISNOTIFY, (uint)Notification.Function.Status.Called, nId, range);
      }
      Emit(Opcode.CALL, funcRegister, argLength, 0, range);
      if (notifyReturned) {
        Emit(Opcode.VISNOTIFY, (uint)Notification.Function.Status.Returned, nId, range);
      }
    }

    internal void EmitAssignNotification(string name, VariableType type, uint valueId,
                                        TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.Assignment>()) {
        var notification = new Notification.Assignment(name, type, valueId);
        Emit(Opcode.VISNOTIFY, 0, Chunk.AddNotification(notification), range);
      }
    }

    internal void EmitBinaryNotification(uint leftId, BinaryOperator op, uint rightId,
                                         uint resultId, TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.Binary>()) {
        var notification = new Notification.Binary(leftId, op, rightId, resultId);
        Emit(Opcode.VISNOTIFY, 0, Chunk.AddNotification(notification), range);
      }
    }

    internal void EmitUnaryNotification(UnaryOperator op, uint valueId, uint resultId,
                                        TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.Unary>()) {
        var notification = new Notification.Unary(op, valueId, resultId);
        Emit(Opcode.VISNOTIFY, 0, Chunk.AddNotification(notification), range);
      }
    }

    internal void EmitVTagEnteredNotification(VTagStatement vTag) {
      if (_visualizerCenter.HasVisualizer<Event.VTagEntered>()) {
        Event.VTagEntered.VTagInfo[] vTagInfos = Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
          var argTexts = Array.ConvertAll(vTagInfo.Args, args => args.Text);
          return new Event.VTagEntered.VTagInfo(vTagInfo.Name, argTexts);
        });
        var notification = new Notification.VTagEntered(vTagInfos);
        Emit(Opcode.VISNOTIFY, 0, Chunk.AddNotification(notification), vTag.Range);
      }
    }

    internal void EmitVTagExitedNotification(VTagStatement vTag, ExprCompiler exprCompiler) {
      if (_visualizerCenter.HasVisualizer<Event.VTagExited>()) {
        BeginBlockScope();
        var vTagInfos = Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
          var valueIds = new uint[vTagInfo.Args.Length];
          for (int j = 0; j < vTagInfo.Args.Length; j++) {
            if (GetRegisterOrConstantId(vTagInfo.Args[j].Expr) is uint id) {
              valueIds[j] = id;
            } else {
              valueIds[j] = AllocateRegister();
              exprCompiler.RegisterForSubExpr = valueIds[j];
              exprCompiler.Visit(vTagInfo.Args[j].Expr);
            }
          }
          return new Notification.VTagExited.VTagInfo(vTagInfo.Name, valueIds);
        });
        EndBlockScope();
        var notification = new Notification.VTagExited(vTagInfos);
        Emit(Opcode.VISNOTIFY, 0, Chunk.AddNotification(notification), vTag.Range);
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
      switch (op) {
        case BinaryOperator.Add:
          return Opcode.ADD;
        case BinaryOperator.Subtract:
          return Opcode.SUB;
        case BinaryOperator.Multiply:
          return Opcode.MUL;
        case BinaryOperator.Divide:
          return Opcode.DIV;
        case BinaryOperator.FloorDivide:
          return Opcode.FLOORDIV;
        case BinaryOperator.Power:
          return Opcode.POW;
        case BinaryOperator.Modulo:
          return Opcode.MOD;
        default:
          throw new NotImplementedException($"Operator {op} not implemented.");
      }
    }

    internal static (Opcode, bool) OpcodeAndCheckFlagOfComparisonOperator(ComparisonOperator op) {
      switch (op) {
        case ComparisonOperator.Less:
          return (Opcode.LT, true);
        case ComparisonOperator.Greater:
          return (Opcode.LE, false);
        case ComparisonOperator.LessEqual:
          return (Opcode.LE, true);
        case ComparisonOperator.GreaterEqual:
          return (Opcode.LT, false);
        case ComparisonOperator.EqEqual:
          return (Opcode.EQ, true);
        case ComparisonOperator.NotEqual:
          return (Opcode.EQ, false);
        case ComparisonOperator.In:
          return (Opcode.IN, true);
        default:
          throw new NotImplementedException($"Operator {op} not implemented.");
      }
    }

    private static bool CanFollowSingleStep(Opcode opcode) {
      switch (opcode) {
        case Opcode.LOADBOOL:
        case Opcode.EQ:
        case Opcode.LT:
        case Opcode.LE:
        case Opcode.IN:
        case Opcode.TEST:
        case Opcode.TESTSET:
          return false;
        default:
          return true;
      }
    }

    private bool CanAddSingleStep() {
      return Chunk.Bytecode.Count == 0 ||
             CanFollowSingleStep(Chunk.Bytecode[Chunk.LatestCodePos].Opcode);
    }

    private void TryEmitSingleStepNotification(TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.SingleStep>()) {
        if (range.Start.Line != _sourceLineOfPrevBytecode && CanAddSingleStep()) {
          // Creates the text range to indicate the start of a single step source line.
          var eventRange = new TextRange(range.Start.Line, 0, range.Start.Line, 0);
          Chunk.Emit(Opcode.VISNOTIFY, 0, Chunk.IdOfSingleStepNotification(), eventRange);
          _sourceLineOfPrevBytecode = range.Start.Line;
        }
      }
    }
  }
}
