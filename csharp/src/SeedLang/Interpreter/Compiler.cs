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
  // The compiler to convert an AST tree to bytecode.
  internal class Compiler : StatementWalker {
    private VisualizerCenter _visualizerCenter;
    private RunMode _runMode;
    private ExprCompiler _exprCompiler;

    private VariableResolver _variableResolver;
    private NestedFuncStack _nestedFuncStack;
    private NestedJumpStack _nestedJumpStack;
    private NestedLoopStack _nestedLoopStack;

    // The range of the statement that is just compiled.
    private TextRange _rangeOfPrevStatement = null;

    // The chunk on the top of the function stack.
    private Chunk _chunk;
    // The constant cache on the top of the function stack.
    private ConstantCache _constantCache;

    internal Function Compile(Statement program, GlobalEnvironment env,
                              VisualizerCenter visualizerCenter, RunMode runMode) {
      _visualizerCenter = visualizerCenter;
      _runMode = runMode;
      _variableResolver = new VariableResolver(env);
      _nestedFuncStack = new NestedFuncStack();
      _nestedJumpStack = new NestedJumpStack();
      _nestedLoopStack = new NestedLoopStack();
      _exprCompiler = new ExprCompiler(visualizerCenter, _variableResolver, _nestedJumpStack);
      // Starts to parse the main function in the global scope.
      _nestedFuncStack.PushFunc("main");
      CacheTopFunction();
      Visit(program);
      EmitDefaultReturn();
      return _nestedFuncStack.PopFunc();
    }

    protected override void VisitAssignment(AssignmentStatement assignment) {
      _rangeOfPrevStatement = assignment.Range;
      if (assignment.Exprs.Length == 1) {
        Unpack(assignment.Targets, assignment.Exprs[0], assignment.Range);
      } else {
        Pack(assignment.Targets, assignment.Exprs, assignment.Range);
      }
    }

    protected override void VisitBlock(BlockStatement block) {
      _rangeOfPrevStatement = block.Range;
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void VisitBreak(BreakStatement @break) {
      _chunk.Emit(Opcode.JMP, 0, 0, @break.Range);
      _nestedLoopStack.AddBreakJump(GetCurrentCodePos());
    }

    // TODO: implement continue statements.
    protected override void VisitContinue(ContinueStatement @continue) {
    }

    protected override void VisitExpression(ExpressionStatement expr) {
      _rangeOfPrevStatement = expr.Range;
      _variableResolver.BeginExpressionScope();
      _exprCompiler.RegisterForSubExpr = _variableResolver.AllocateRegister();
      switch (_runMode) {
        case RunMode.Interactive:
          Expression eval = Expression.Identifier(NativeFunctions.PrintVal, expr.Range);
          _exprCompiler.Visit(Expression.Call(eval, new Expression[] { expr.Expr }, expr.Range));
          break;
        case RunMode.Script:
          _exprCompiler.Visit(expr.Expr);
          break;
      }
      _variableResolver.EndExpressionScope();
    }

    protected override void VisitForIn(ForInStatement forIn) {
      _nestedLoopStack.PushLoopFrame();
      _rangeOfPrevStatement = forIn.Range;
      VariableResolver.VariableInfo loopVar = DefineVariableIfNeeded(forIn.Id.Name);

      _variableResolver.BeginBlockScope();
      if (!(GetRegisterId(forIn.Expr) is uint sequence)) {
        sequence = _variableResolver.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = sequence;
        _exprCompiler.Visit(forIn.Expr);
      }
      uint index = _variableResolver.AllocateRegister();
      _chunk.Emit(Opcode.LOADK, index, _constantCache.IdOfConstant(0), forIn.Range);
      uint limit = _variableResolver.AllocateRegister();
      _chunk.Emit(Opcode.LEN, limit, sequence, 0, forIn.Range);
      uint step = _variableResolver.AllocateRegister();
      _chunk.Emit(Opcode.LOADK, step, _constantCache.IdOfConstant(1), forIn.Range);
      _chunk.Emit(Opcode.FORPREP, index, 0, forIn.Range);
      int bodyStart = _chunk.Bytecode.Count;
      switch (loopVar.Type) {
        case VariableResolver.VariableType.Global:
          _variableResolver.BeginExpressionScope();
          uint targetId = _variableResolver.AllocateRegister();
          _chunk.Emit(Opcode.GETELEM, targetId, sequence, index, forIn.Range);
          _chunk.Emit(Opcode.SETGLOB, targetId, loopVar.Id, forIn.Range);
          EmitAssignNotification(forIn.Id.Name, VariableType.Global, targetId, forIn.Id.Range);
          _variableResolver.EndExpressionScope();
          break;
        case VariableResolver.VariableType.Local:
          _chunk.Emit(Opcode.GETELEM, loopVar.Id, sequence, index, forIn.Range);
          EmitAssignNotification(forIn.Id.Name, VariableType.Local, loopVar.Id, forIn.Id.Range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
      Visit(forIn.Body);
      PatchJumpToCurrentPos(bodyStart - 1);
      _chunk.Emit(Opcode.FORLOOP, index, bodyStart - (_chunk.Bytecode.Count + 1), forIn.Range);
      _variableResolver.EndBlockScope();
      PatchJumps(_nestedLoopStack.BreaksJumps);
      _nestedLoopStack.PopLoopFrame();
    }

    protected override void VisitFuncDef(FuncDefStatement funcDef) {
      _rangeOfPrevStatement = funcDef.Range;
      VariableResolver.VariableInfo info = DefineVariableIfNeeded(funcDef.Name);
      PushFunc(funcDef.Name);
      foreach (string parameterName in funcDef.Parameters) {
        _variableResolver.DefineVariable(parameterName);
      }
      Visit(funcDef.Body);
      EmitDefaultReturn();

      Function func = PopFunc();
      uint funcId = _constantCache.IdOfConstant(func);
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          _variableResolver.BeginExpressionScope();
          uint registerId = _variableResolver.AllocateRegister();
          _chunk.Emit(Opcode.LOADK, registerId, funcId, funcDef.Range);
          _chunk.Emit(Opcode.SETGLOB, registerId, info.Id, funcDef.Range);
          _variableResolver.EndExpressionScope();
          break;
        case VariableResolver.VariableType.Local:
          _chunk.Emit(Opcode.LOADK, info.Id, funcId, funcDef.Range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    protected override void VisitIf(IfStatement @if) {
      _rangeOfPrevStatement = @if.Range;
      _nestedJumpStack.PushFrame();
      VisitTest(@if.Test);
      PatchJumps(_nestedJumpStack.TrueJumps);
      Visit(@if.ThenBody);
      if (!(@if.ElseBody is null)) {
        _chunk.Emit(Opcode.JMP, 0, 0, @if.Range);
        int jumpEndPos = GetCurrentCodePos();
        PatchJumps(_nestedJumpStack.FalseJumps);
        Visit(@if.ElseBody);
        PatchJumpToCurrentPos(jumpEndPos);
      } else {
        PatchJumps(_nestedJumpStack.FalseJumps);
      }
      _nestedJumpStack.PopFrame();
    }

    protected override void VisitPass(PassStatement pass) {
      _rangeOfPrevStatement = pass.Range;
    }

    protected override void VisitReturn(ReturnStatement @return) {
      _rangeOfPrevStatement = @return.Range;
      if (@return.Exprs.Length == 0) {
        _chunk.Emit(Opcode.RETURN, 0, 0, 0, @return.Range);
      } else if (@return.Exprs.Length == 1) {
        if (!(GetRegisterId(@return.Exprs[0]) is uint result)) {
          _variableResolver.BeginExpressionScope();
          result = _variableResolver.AllocateRegister();
          _exprCompiler.RegisterForSubExpr = result;
          _exprCompiler.Visit(@return.Exprs[0]);
          _variableResolver.EndExpressionScope();
        }
        _chunk.Emit(Opcode.RETURN, result, 1, 0, @return.Range);
      } else {
        _variableResolver.BeginExpressionScope();
        uint listRegister = _variableResolver.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = listRegister;
        _exprCompiler.Visit(Expression.Tuple(@return.Exprs, @return.Range));
        _chunk.Emit(Opcode.RETURN, listRegister, 1, 0, @return.Range);
        _variableResolver.EndExpressionScope();
      }
    }

    protected override void VisitWhile(WhileStatement @while) {
      _nestedLoopStack.PushLoopFrame();
      _rangeOfPrevStatement = @while.Range;
      _nestedJumpStack.PushFrame();
      int start = GetCurrentCodePos();
      VisitTest(@while.Test);
      Visit(@while.Body);
      _chunk.Emit(Opcode.JMP, 0, start - GetCurrentCodePos() - 1, @while.Range);
      PatchJumps(_nestedJumpStack.FalseJumps);
      _nestedJumpStack.PopFrame();
      PatchJumps(_nestedLoopStack.BreaksJumps);
      _nestedLoopStack.PopLoopFrame();
    }

    protected override void VisitVTag(VTagStatement vTag) {
      _rangeOfPrevStatement = vTag.Range;
      EmitVTagEnteredNotification(CreateVTagEnteredInfo(vTag), vTag.Range);
      foreach (Statement statement in vTag.Statements) {
        Visit(statement);
      }
      EmitVTagExitedNotification(CreateVTagExitedInfo(vTag), vTag.Range);
    }

    private static Event.VTagEntered.VTagInfo[] CreateVTagEnteredInfo(VTagStatement vTag) {
      return Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
        var argTexts = Array.ConvertAll(vTagInfo.Args, args => args.Text);
        return new Event.VTagEntered.VTagInfo(vTagInfo.Name, argTexts);
      });
    }

    private Notification.VTagExited.VTagInfo[] CreateVTagExitedInfo(VTagStatement vTag) {
      _variableResolver.BeginBlockScope();
      var vTagInfos = Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
        var valueIds = new uint[vTagInfo.Args.Length];
        for (int j = 0; j < vTagInfo.Args.Length; j++) {
          if (GetRegisterOrConstantId(vTagInfo.Args[j].Expr) is uint id) {
            valueIds[j] = id;
          } else {
            valueIds[j] = _variableResolver.AllocateRegister();
            _exprCompiler.RegisterForSubExpr = valueIds[j];
            _exprCompiler.Visit(vTagInfo.Args[j].Expr);
          }
        }
        return new Notification.VTagExited.VTagInfo(vTagInfo.Name, valueIds);
      });
      _variableResolver.EndBlockScope();
      return vTagInfos;
    }

    private void VisitTest(Expression test) {
      if (test is ComparisonExpression || test is BooleanExpression) {
        _exprCompiler.Visit(test);
      } else {
        if (GetRegisterId(test) is uint registerId) {
          _chunk.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
        } else {
          _variableResolver.BeginExpressionScope();
          registerId = _variableResolver.AllocateRegister();
          _exprCompiler.RegisterForSubExpr = registerId;
          _exprCompiler.Visit(test);
          _chunk.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
          _variableResolver.EndExpressionScope();
        }
        _chunk.Emit(Opcode.JMP, 0, 0, test.Range);
        int jump = GetCurrentCodePos();
        _nestedJumpStack.FalseJumps.Add(jump);
      }
    }

    private void Pack(Expression[] targets, Expression[] exprs, TextRange range) {
      if (targets.Length == 1) {
        Assign(targets[0], Expression.Tuple(exprs, range), range);
      } else if (targets.Length == exprs.Length) {
        AssignMultipleTargets(targets, exprs, range);
      } else {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", range,
                                      Message.RuntimeErrorIncorrectUnpackCount);
      }
    }

    private void Unpack(Expression[] targets, Expression expr, TextRange range) {
      if (targets.Length == 1) {
        Assign(targets[0], expr, range);
      } else {
        // If the length of targets is less than the one of the unpacked value, SeedPython will
        // unpack part of the value. And if the length of the targets is greater than the one of the
        // unpacked value, an index out of range exception will be thrown.
        // The behavior is different from the original Python. Python will throw an incorrect unpack
        // count exception for both situations.
        // TODO: Add a build-in function to check the length of the unpacked values.
        foreach (Expression target in targets) {
          if (target is IdentifierExpression id) {
            DefineVariableIfNeeded(id.Name);
          }
        }
        _variableResolver.BeginExpressionScope();
        uint listId = VisitExpressionForRegisterId(expr);
        uint valueId = _variableResolver.AllocateRegister();
        for (int i = 0; i < targets.Length; i++) {
          _variableResolver.BeginExpressionScope();
          uint constId = _constantCache.IdOfConstant(i);
          uint indexId = _variableResolver.AllocateRegister();
          _chunk.Emit(Opcode.LOADK, indexId, constId, range);
          _chunk.Emit(Opcode.GETELEM, valueId, listId, indexId, range);
          Assign(targets[i], valueId, range);
          _variableResolver.EndExpressionScope();
        }
        _variableResolver.EndExpressionScope();
      }
    }

    private void Assign(Expression target, Expression expr, TextRange range) {
      if (target is IdentifierExpression id) {
        DefineVariableIfNeeded(id.Name);
      }
      _variableResolver.BeginExpressionScope();
      uint valueId = VisitExpressionForRKId(expr);
      Assign(target, valueId, range);
      _variableResolver.EndExpressionScope();
    }

    private void AssignMultipleTargets(Expression[] targets, Expression[] exprs, TextRange range) {
      for (int i = 0; i < targets.Length; i++) {
        if (targets[i] is IdentifierExpression id) {
          DefineVariableIfNeeded(id.Name);
        }
      }
      _variableResolver.BeginExpressionScope();
      var exprIds = new uint[targets.Length];
      for (int i = 0; i < targets.Length; i++) {
        exprIds[i] = VisitExpressionForRKId(exprs[i]);
      }
      for (int i = 0; i < targets.Length; i++) {
        Assign(targets[i], exprIds[i], range);
      }
      _variableResolver.EndExpressionScope();
    }

    private void Assign(Expression expr, uint valueId, TextRange range) {
      switch (expr) {
        case IdentifierExpression id:
          Assign(id, valueId, range);
          break;
        case SubscriptExpression subscript:
          uint listId = VisitExpressionForRegisterId(subscript.Expr);
          uint indexId = VisitExpressionForRKId(subscript.Index);
          _chunk.Emit(Opcode.SETELEM, listId, indexId, valueId, range);
          break;
      }
    }

    private void Assign(IdentifierExpression id, uint valueId, TextRange range) {
      VariableResolver.VariableInfo info = _variableResolver.FindVariable(id.Name).Value;
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          uint tempRegister = valueId;
          if (Chunk.IsConstId(valueId)) {
            tempRegister = _variableResolver.AllocateRegister();
            _chunk.Emit(Opcode.LOADK, tempRegister, valueId, range);
          }
          _chunk.Emit(Opcode.SETGLOB, tempRegister, info.Id, range);
          EmitAssignNotification(id.Name, VariableType.Global, tempRegister, range);
          break;
        case VariableResolver.VariableType.Local:
          if (Chunk.IsConstId(valueId)) {
            _chunk.Emit(Opcode.LOADK, info.Id, valueId, range);
          } else {
            _chunk.Emit(Opcode.MOVE, info.Id, valueId, 0, range);
          }
          EmitAssignNotification(id.Name, VariableType.Local, valueId, range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    private VariableResolver.VariableInfo DefineVariableIfNeeded(string name) {
      if (_variableResolver.FindVariable(name) is VariableResolver.VariableInfo info) {
        return info;
      }
      return _variableResolver.DefineVariable(name);
    }

    private int GetCurrentCodePos() {
      return _chunk.Bytecode.Count - 1;
    }

    private void PatchJumps(List<int> jumps) {
      foreach (int jump in jumps) {
        PatchJumpToCurrentPos(jump);
      }
      jumps.Clear();
    }

    private void PatchJumpToCurrentPos(int jump) {
      _chunk.PatchSBXAt(jump, _chunk.Bytecode.Count - jump - 1);
    }

    private void PushFunc(string name) {
      _nestedFuncStack.PushFunc(name);
      CacheTopFunction();
      _variableResolver.BeginFunctionScope();
    }

    private Function PopFunc() {
      _variableResolver.EndFunctionScope();
      Function func = _nestedFuncStack.PopFunc();
      CacheTopFunction();
      return func;
    }

    private void CacheTopFunction() {
      _chunk = _nestedFuncStack.CurrentChunk();
      _constantCache = _nestedFuncStack.CurrentConstantCache();
      _exprCompiler.SetChunk(_chunk);
      _exprCompiler.SetConstantCache(_constantCache);
    }

    private uint VisitExpressionForRegisterId(Expression expr) {
      if (!(GetRegisterId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = exprId;
        _exprCompiler.Visit(expr);
      }
      return exprId;
    }

    private uint VisitExpressionForRKId(Expression expr) {
      if (!(GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = exprId;
        _exprCompiler.Visit(expr);
      }
      return exprId;
    }

    private uint? GetRegisterOrConstantId(Expression expr) {
      if (GetRegisterId(expr) is uint registerId) {
        return registerId;
      } else if (GetConstantId(expr) is uint constantId) {
        return constantId;
      }
      return null;
    }

    private uint? GetRegisterId(Expression expr) {
      if (expr is IdentifierExpression identifier &&
          _variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info &&
          info.Type == VariableResolver.VariableType.Local) {
        return info.Id;
      }
      return null;
    }

    private uint? GetConstantId(Expression expr) {
      switch (expr) {
        case NumberConstantExpression number:
          return _constantCache.IdOfConstant(number.Value);
        case StringConstantExpression str:
          return _constantCache.IdOfConstant(str.Value);
        default:
          return null;
      }
    }

    private void EmitDefaultReturn() {
      var range = _rangeOfPrevStatement is null ? new TextRange(1, 0, 1, 0) : _rangeOfPrevStatement;
      _chunk.Emit(Opcode.RETURN, 0, 0, 0, range);
    }

    private void EmitAssignNotification(string name, VariableType type, uint valueId,
                                        TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.Assignment>()) {
        var notification = new Notification.Assignment(name, type, valueId, range);
        _chunk.Emit(Opcode.VISNOTIFY, 0, _chunk.AddNotification(notification), range);
      }
    }

    private void EmitVTagEnteredNotification(Event.VTagEntered.VTagInfo[] vTagInfos,
                                             TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.VTagEntered>()) {
        var notification = new Notification.VTagEntered(vTagInfos, range);
        _chunk.Emit(Opcode.VISNOTIFY, 0, _chunk.AddNotification(notification), range);
      }
    }

    private void EmitVTagExitedNotification(Notification.VTagExited.VTagInfo[] vTagInfos,
                                            TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.VTagExited>()) {
        var notification = new Notification.VTagExited(vTagInfos, range);
        _chunk.Emit(Opcode.VISNOTIFY, 0, _chunk.AddNotification(notification), range);
      }
    }
  }
}
