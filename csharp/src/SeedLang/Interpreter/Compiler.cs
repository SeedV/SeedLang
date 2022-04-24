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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class Compiler : StatementWalker {
    private RunMode _runMode;
    private CompilerHelper _helper;
    private ExprCompiler _exprCompiler;

    private NestedFuncStack _nestedFuncStack;
    private NestedLoopStack _nestedLoopStack;

    // The range of the statement that is just compiled.
    private TextRange _rangeOfPrevStatement = null;

    internal Function Compile(Statement program, GlobalEnvironment env,
                              VisualizerCenter visualizerCenter, RunMode runMode) {
      _runMode = runMode;
      _helper = new CompilerHelper(visualizerCenter, env);
      _nestedFuncStack = new NestedFuncStack();
      _nestedLoopStack = new NestedLoopStack();
      _exprCompiler = new ExprCompiler(_helper);
      // Starts to parse the main function in the global scope.
      _nestedFuncStack.PushFunc("main");
      CacheTopFunction();
      Visit(program);
      EmitDefaultReturn();
      return _nestedFuncStack.PopFunc();
    }

    protected override void Enter(Statement statement) {
      _rangeOfPrevStatement = statement.Range;
    }

    protected override void VisitAssignment(AssignmentStatement assignment) {
      if (assignment.Exprs.Length == 1) {
        Unpack(assignment.Targets, assignment.Exprs[0], assignment.Range);
      } else {
        Pack(assignment.Targets, assignment.Exprs, assignment.Range);
      }
    }

    protected override void VisitBlock(BlockStatement block) {
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void VisitBreak(BreakStatement @break) {
      _helper.Chunk.Emit(Opcode.JMP, 0, 0, @break.Range);
      _nestedLoopStack.AddBreakJump(_helper.Chunk.LatestCodePos);
    }

    // TODO: implement continue statements.
    protected override void VisitContinue(ContinueStatement @continue) {
    }

    protected override void VisitExpression(ExpressionStatement expr) {
      _helper.BeginExpressionScope();
      _exprCompiler.RegisterForSubExpr = _helper.AllocateRegister();
      switch (_runMode) {
        case RunMode.Interactive:
          Expression eval = Expression.Identifier(NativeFunctions.PrintVal, expr.Range);
          _exprCompiler.Visit(Expression.Call(eval, new Expression[] { expr.Expr }, expr.Range));
          break;
        case RunMode.Script:
          _exprCompiler.Visit(expr.Expr);
          break;
      }
      _helper.EndExpressionScope();
    }

    protected override void VisitForIn(ForInStatement forIn) {
      _nestedLoopStack.PushLoopFrame();
      VariableResolver.VariableInfo loopVar = DefineVariableIfNeeded(forIn.Id.Name);

      _helper.BeginBlockScope();
      if (!(_helper.GetRegisterId(forIn.Expr) is uint sequence)) {
        sequence = _helper.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = sequence;
        _exprCompiler.Visit(forIn.Expr);
      }
      uint index = _helper.AllocateRegister();
      _helper.Chunk.Emit(Opcode.LOADK, index, _helper.ConstantCache.IdOfConstant(0), forIn.Range);
      uint limit = _helper.AllocateRegister();
      _helper.Chunk.Emit(Opcode.LEN, limit, sequence, 0, forIn.Range);
      uint step = _helper.AllocateRegister();
      _helper.Chunk.Emit(Opcode.LOADK, step, _helper.ConstantCache.IdOfConstant(1), forIn.Range);
      _helper.Chunk.Emit(Opcode.FORPREP, index, 0, forIn.Range);
      int bodyStart = _helper.Chunk.Bytecode.Count;
      switch (loopVar.Type) {
        case VariableResolver.VariableType.Global:
          _helper.BeginExpressionScope();
          uint targetId = _helper.AllocateRegister();
          _helper.Chunk.Emit(Opcode.GETELEM, targetId, sequence, index, forIn.Range);
          _helper.Chunk.Emit(Opcode.SETGLOB, targetId, loopVar.Id, forIn.Range);
          _helper.EmitAssignNotification(forIn.Id.Name, VariableType.Global, targetId,
                                         forIn.Id.Range);
          _helper.EndExpressionScope();
          break;
        case VariableResolver.VariableType.Local:
          _helper.Chunk.Emit(Opcode.GETELEM, loopVar.Id, sequence, index, forIn.Range);
          _helper.EmitAssignNotification(forIn.Id.Name, VariableType.Local, loopVar.Id,
                                         forIn.Id.Range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
      Visit(forIn.Body);
      _helper.PatchJumpToCurrentPos(bodyStart - 1);
      _helper.Chunk.Emit(Opcode.FORLOOP, index, bodyStart - (_helper.Chunk.Bytecode.Count + 1),
                         forIn.Range);
      _helper.EndBlockScope();
      _helper.PatchJumpsToCurrentPos(_nestedLoopStack.BreaksJumps);
      _nestedLoopStack.PopLoopFrame();
    }

    protected override void VisitFuncDef(FuncDefStatement funcDef) {
      VariableResolver.VariableInfo info = DefineVariableIfNeeded(funcDef.Name);
      PushFunc(funcDef.Name);
      foreach (string parameterName in funcDef.Parameters) {
        _helper.DefineVariable(parameterName);
      }
      Visit(funcDef.Body);
      EmitDefaultReturn();

      Function func = PopFunc();
      uint funcId = _helper.ConstantCache.IdOfConstant(func);
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          _helper.BeginExpressionScope();
          uint registerId = _helper.AllocateRegister();
          _helper.Chunk.Emit(Opcode.LOADK, registerId, funcId, funcDef.Range);
          _helper.Chunk.Emit(Opcode.SETGLOB, registerId, info.Id, funcDef.Range);
          _helper.EndExpressionScope();
          break;
        case VariableResolver.VariableType.Local:
          _helper.Chunk.Emit(Opcode.LOADK, info.Id, funcId, funcDef.Range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    protected override void VisitIf(IfStatement @if) {
      _helper.NestedJumpStack.PushFrame();
      VisitTest(@if.Test);
      _helper.PatchJumpsToCurrentPos(_helper.NestedJumpStack.TrueJumps);
      Visit(@if.ThenBody);
      if (!(@if.ElseBody is null)) {
        _helper.Chunk.Emit(Opcode.JMP, 0, 0, @if.Range);
        int jumpEndPos = _helper.Chunk.LatestCodePos;
        _helper.PatchJumpsToCurrentPos(_helper.NestedJumpStack.FalseJumps);
        Visit(@if.ElseBody);
        _helper.PatchJumpToCurrentPos(jumpEndPos);
      } else {
        _helper.PatchJumpsToCurrentPos(_helper.NestedJumpStack.FalseJumps);
      }
      _helper.NestedJumpStack.PopFrame();
    }

    protected override void VisitPass(PassStatement pass) { }

    protected override void VisitReturn(ReturnStatement @return) {
      if (@return.Exprs.Length == 0) {
        _helper.Chunk.Emit(Opcode.RETURN, 0, 0, 0, @return.Range);
      } else if (@return.Exprs.Length == 1) {
        if (!(_helper.GetRegisterId(@return.Exprs[0]) is uint result)) {
          _helper.BeginExpressionScope();
          result = _helper.AllocateRegister();
          _exprCompiler.RegisterForSubExpr = result;
          _exprCompiler.Visit(@return.Exprs[0]);
          _helper.EndExpressionScope();
        }
        _helper.Chunk.Emit(Opcode.RETURN, result, 1, 0, @return.Range);
      } else {
        _helper.BeginExpressionScope();
        uint listRegister = _helper.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = listRegister;
        _exprCompiler.Visit(Expression.Tuple(@return.Exprs, @return.Range));
        _helper.Chunk.Emit(Opcode.RETURN, listRegister, 1, 0, @return.Range);
        _helper.EndExpressionScope();
      }
    }

    protected override void VisitWhile(WhileStatement @while) {
      _nestedLoopStack.PushLoopFrame();
      _helper.NestedJumpStack.PushFrame();
      int start = _helper.Chunk.LatestCodePos;
      VisitTest(@while.Test);
      Visit(@while.Body);
      _helper.Chunk.Emit(Opcode.JMP, 0, start - _helper.Chunk.LatestCodePos - 1, @while.Range);
      _helper.PatchJumpsToCurrentPos(_helper.NestedJumpStack.FalseJumps);
      _helper.NestedJumpStack.PopFrame();
      _helper.PatchJumpsToCurrentPos(_nestedLoopStack.BreaksJumps);
      _nestedLoopStack.PopLoopFrame();
    }

    protected override void VisitVTag(VTagStatement vTag) {
      _helper.EmitVTagEnteredNotification(CreateVTagEnteredInfo(vTag), vTag.Range);
      foreach (Statement statement in vTag.Statements) {
        Visit(statement);
      }
      _helper.EmitVTagExitedNotification(CreateVTagExitedInfo(vTag), vTag.Range);
    }

    private static Event.VTagEntered.VTagInfo[] CreateVTagEnteredInfo(VTagStatement vTag) {
      return Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
        var argTexts = Array.ConvertAll(vTagInfo.Args, args => args.Text);
        return new Event.VTagEntered.VTagInfo(vTagInfo.Name, argTexts);
      });
    }

    private Notification.VTagExited.VTagInfo[] CreateVTagExitedInfo(VTagStatement vTag) {
      _helper.BeginBlockScope();
      var vTagInfos = Array.ConvertAll(vTag.VTagInfos, vTagInfo => {
        var valueIds = new uint[vTagInfo.Args.Length];
        for (int j = 0; j < vTagInfo.Args.Length; j++) {
          if (_helper.GetRegisterOrConstantId(vTagInfo.Args[j].Expr) is uint id) {
            valueIds[j] = id;
          } else {
            valueIds[j] = _helper.AllocateRegister();
            _exprCompiler.RegisterForSubExpr = valueIds[j];
            _exprCompiler.Visit(vTagInfo.Args[j].Expr);
          }
        }
        return new Notification.VTagExited.VTagInfo(vTagInfo.Name, valueIds);
      });
      _helper.EndBlockScope();
      return vTagInfos;
    }

    private void VisitTest(Expression test) {
      if (test is ComparisonExpression || test is BooleanExpression) {
        _exprCompiler.Visit(test);
      } else {
        if (_helper.GetRegisterId(test) is uint registerId) {
          _helper.Chunk.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
        } else {
          _helper.BeginExpressionScope();
          registerId = _helper.AllocateRegister();
          _exprCompiler.RegisterForSubExpr = registerId;
          _exprCompiler.Visit(test);
          _helper.Chunk.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
          _helper.EndExpressionScope();
        }
        _helper.Chunk.Emit(Opcode.JMP, 0, 0, test.Range);
        _helper.NestedJumpStack.FalseJumps.Add(_helper.Chunk.LatestCodePos);
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
        _helper.BeginExpressionScope();
        uint listId = VisitExpressionForRegisterId(expr);
        uint valueId = _helper.AllocateRegister();
        for (int i = 0; i < targets.Length; i++) {
          _helper.BeginExpressionScope();
          uint constId = _helper.ConstantCache.IdOfConstant(i);
          uint indexId = _helper.AllocateRegister();
          _helper.Chunk.Emit(Opcode.LOADK, indexId, constId, range);
          _helper.Chunk.Emit(Opcode.GETELEM, valueId, listId, indexId, range);
          Assign(targets[i], valueId, range);
          _helper.EndExpressionScope();
        }
        _helper.EndExpressionScope();
      }
    }

    private void Assign(Expression target, Expression expr, TextRange range) {
      if (target is IdentifierExpression id) {
        DefineVariableIfNeeded(id.Name);
      }
      _helper.BeginExpressionScope();
      uint valueId = VisitExpressionForRKId(expr);
      Assign(target, valueId, range);
      _helper.EndExpressionScope();
    }

    private void AssignMultipleTargets(Expression[] targets, Expression[] exprs, TextRange range) {
      for (int i = 0; i < targets.Length; i++) {
        if (targets[i] is IdentifierExpression id) {
          DefineVariableIfNeeded(id.Name);
        }
      }
      _helper.BeginExpressionScope();
      var exprIds = new uint[targets.Length];
      for (int i = 0; i < targets.Length; i++) {
        exprIds[i] = VisitExpressionForRKId(exprs[i]);
      }
      for (int i = 0; i < targets.Length; i++) {
        Assign(targets[i], exprIds[i], range);
      }
      _helper.EndExpressionScope();
    }

    private void Assign(Expression expr, uint valueId, TextRange range) {
      switch (expr) {
        case IdentifierExpression id:
          Assign(id, valueId, range);
          break;
        case SubscriptExpression subscript:
          uint listId = VisitExpressionForRegisterId(subscript.Expr);
          uint indexId = VisitExpressionForRKId(subscript.Index);
          _helper.Chunk.Emit(Opcode.SETELEM, listId, indexId, valueId, range);
          break;
      }
    }

    private void Assign(IdentifierExpression id, uint valueId, TextRange range) {
      VariableResolver.VariableInfo info = _helper.FindVariable(id.Name).Value;
      switch (info.Type) {
        case VariableResolver.VariableType.Global:
          uint tempRegister = valueId;
          if (Chunk.IsConstId(valueId)) {
            tempRegister = _helper.AllocateRegister();
            _helper.Chunk.Emit(Opcode.LOADK, tempRegister, valueId, range);
          }
          _helper.Chunk.Emit(Opcode.SETGLOB, tempRegister, info.Id, range);
          _helper.EmitAssignNotification(id.Name, VariableType.Global, tempRegister, range);
          break;
        case VariableResolver.VariableType.Local:
          if (Chunk.IsConstId(valueId)) {
            _helper.Chunk.Emit(Opcode.LOADK, info.Id, valueId, range);
          } else {
            _helper.Chunk.Emit(Opcode.MOVE, info.Id, valueId, 0, range);
          }
          _helper.EmitAssignNotification(id.Name, VariableType.Local, valueId, range);
          break;
        case VariableResolver.VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    private VariableResolver.VariableInfo DefineVariableIfNeeded(string name) {
      if (_helper.FindVariable(name) is VariableResolver.VariableInfo info) {
        return info;
      }
      return _helper.DefineVariable(name);
    }

    private void PushFunc(string name) {
      _nestedFuncStack.PushFunc(name);
      CacheTopFunction();
      _helper.BeginFunctionScope();
    }

    private Function PopFunc() {
      _helper.EndFunctionScope();
      Function func = _nestedFuncStack.PopFunc();
      CacheTopFunction();
      return func;
    }

    private void CacheTopFunction() {
      _helper.Chunk = _nestedFuncStack.CurrentChunk();
      _helper.ConstantCache = _nestedFuncStack.CurrentConstantCache();
    }

    private uint VisitExpressionForRegisterId(Expression expr) {
      if (!(_helper.GetRegisterId(expr) is uint exprId)) {
        exprId = _helper.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = exprId;
        _exprCompiler.Visit(expr);
      }
      return exprId;
    }

    private uint VisitExpressionForRKId(Expression expr) {
      if (!(_helper.GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _helper.AllocateRegister();
        _exprCompiler.RegisterForSubExpr = exprId;
        _exprCompiler.Visit(expr);
      }
      return exprId;
    }

    private void EmitDefaultReturn() {
      var range = _rangeOfPrevStatement is null ? new TextRange(1, 0, 1, 0) : _rangeOfPrevStatement;
      _helper.Chunk.Emit(Opcode.RETURN, 0, 0, 0, range);
    }
  }
}
