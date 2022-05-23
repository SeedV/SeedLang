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

using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Visualization;

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
      // Emits the HALT instruction to indicate the ending of the program.
      _helper.Chunk.Emit(Opcode.HALT, 1, 0, 0, _rangeOfPrevStatement ?? new TextRange(1, 0, 1, 0));
      return _nestedFuncStack.PopFunc();
    }

    protected override void Enter(Statement statement) {
      _rangeOfPrevStatement = statement.Range;
    }

    protected override void VisitAssignment(AssignmentStatement assignment) {
      foreach (Expression[] targets in assignment.Targets) {
        foreach (Expression target in targets) {
          if (target is IdentifierExpression id) {
            DefineVariableIfNeeded(id.Name);
          }
        }
      }
      _helper.BeginExprScope();
      if (assignment.Exprs.Length == 1) {
        Unpack(assignment.Targets, assignment.Exprs[0], assignment.Range);
      } else {
        Pack(assignment.Targets, assignment.Exprs, assignment.Range);
      }
      _helper.EndExprScope();
    }

    protected override void VisitBlock(BlockStatement block) {
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void VisitBreak(BreakStatement @break) {
      _helper.Emit(Opcode.JMP, 0, 0, @break.Range);
      _nestedLoopStack.AddBreakJump(_helper.Chunk.LatestCodePos, @break.Range);
    }

    // TODO: implement continue statements.
    protected override void VisitContinue(ContinueStatement @continue) {
      _helper.Emit(Opcode.JMP, 0, 0, @continue.Range);
      _nestedLoopStack.AddContinueJump(_helper.Chunk.LatestCodePos, @continue.Range);
    }

    protected override void VisitExpression(ExpressionStatement expr) {
      _helper.BeginExprScope();
      _exprCompiler.RegisterForSubExpr = _helper.DefineTempVariable();
      switch (_runMode) {
        case RunMode.Interactive:
          Expression eval = Expression.Identifier(NativeFunctions.PrintVal, expr.Range);
          _exprCompiler.Visit(Expression.Call(eval, new Expression[] { expr.Expr }, expr.Range));
          break;
        case RunMode.Script:
          _exprCompiler.Visit(expr.Expr);
          break;
      }
      _helper.EndExprScope();
    }

    protected override void VisitForIn(ForInStatement forIn) {
      _nestedLoopStack.PushFrame();
      RegisterInfo loopVar = DefineVariableIfNeeded(forIn.Id.Name);

      if (!(_helper.GetRegisterId(forIn.Expr) is uint sequence)) {
        sequence = _helper.DefineTempVariable();
        _exprCompiler.RegisterForSubExpr = sequence;
        _exprCompiler.Visit(forIn.Expr);
      }
      uint index = _helper.DefineTempVariable();
      _helper.Emit(Opcode.LOADK, index, _helper.ConstantCache.IdOfConstant(0), forIn.Range);
      uint limit = _helper.DefineTempVariable();
      _helper.Emit(Opcode.LEN, limit, sequence, 0, forIn.Range);
      uint step = _helper.DefineTempVariable();
      _helper.Emit(Opcode.LOADK, step, _helper.ConstantCache.IdOfConstant(1), forIn.Range);
      _helper.Emit(Opcode.FORPREP, index, 0, forIn.Range);
      int bodyStart = _helper.Chunk.Bytecode.Count;
      switch (loopVar.Type) {
        case RegisterType.Global:
          _helper.BeginExprScope();
          uint targetId = _helper.DefineTempVariable();
          _helper.Emit(Opcode.GETELEM, targetId, sequence, index, forIn.Range);
          _helper.Emit(Opcode.SETGLOB, targetId, loopVar.Id, forIn.Range);
          _helper.EmitAssignNotification(loopVar.Name, VariableType.Global, targetId,
                                         forIn.Id.Range);
          _helper.EndExprScope();
          break;
        case RegisterType.Local:
          _helper.Emit(Opcode.GETELEM, loopVar.Id, sequence, index, forIn.Range);
          _helper.EmitAssignNotification(loopVar.Name, VariableType.Local, loopVar.Id,
                                         forIn.Id.Range);
          break;
        case RegisterType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
      Visit(forIn.Body);
      // Patches all continue jumps to the FORLOOP instruction.
      _helper.PatchJumpsToCurrentPos(_nestedLoopStack.ContinueJumps);
      _helper.Emit(Opcode.FORLOOP, index, 0, forIn.Range);
      // Patches the jump position of the FORLOOP instruction to the start point of the body.
      _helper.PatchJumpToPos(_helper.Chunk.LatestCodePos, bodyStart);
      // Patches the jump position of the FORPREP instruction to the latest FORLOOP.
      _helper.PatchJumpToPos(bodyStart - 1, _helper.Chunk.LatestCodePos);
      _helper.PatchJumpsToCurrentPos(_nestedLoopStack.BreakJumps);
      _nestedLoopStack.PopFrame();
    }

    protected override void VisitFuncDef(FuncDefStatement funcDef) {
      RegisterInfo info = DefineVariableIfNeeded(funcDef.Name);
      PushFunc(funcDef.Name);
      foreach (string parameterName in funcDef.Parameters) {
        _helper.DefineVariable(parameterName);
      }
      Visit(funcDef.Body);
      _helper.Emit(Opcode.RETURN, 0, 0, 0, _rangeOfPrevStatement ?? new TextRange(1, 0, 1, 0));

      Function func = PopFunc();
      uint funcId = _helper.ConstantCache.IdOfConstant(func);
      switch (info.Type) {
        case RegisterType.Global:
          _helper.BeginExprScope();
          uint registerId = _helper.DefineTempVariable();
          _helper.Emit(Opcode.LOADK, registerId, funcId, funcDef.Range);
          _helper.Emit(Opcode.SETGLOB, registerId, info.Id, funcDef.Range);
          _helper.EndExprScope();
          break;
        case RegisterType.Local:
          _helper.Emit(Opcode.LOADK, info.Id, funcId, funcDef.Range);
          break;
        case RegisterType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    protected override void VisitIf(IfStatement @if) {
      _helper.ExprJumpStack.PushFrame();
      VisitTest(@if.Test);
      _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.TrueJumps);
      Visit(@if.ThenBody);
      if (!(@if.ElseBody is null)) {
        _helper.Emit(Opcode.JMP, 0, 0, @if.Range);
        int jumpEndPos = _helper.Chunk.LatestCodePos;
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
        Visit(@if.ElseBody);
        _helper.PatchJumpToCurrentPos(jumpEndPos);
      } else {
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
      }
      _helper.ExprJumpStack.PopFrame();
    }

    protected override void VisitPass(PassStatement pass) { }

    protected override void VisitReturn(ReturnStatement @return) {
      if (@return.Exprs.Length == 0) {
        _helper.Emit(Opcode.RETURN, 0, 0, 0, @return.Range);
      } else if (@return.Exprs.Length == 1) {
        if (!(_helper.GetRegisterId(@return.Exprs[0]) is uint result)) {
          _helper.BeginExprScope();
          result = _helper.DefineTempVariable();
          _exprCompiler.RegisterForSubExpr = result;
          _exprCompiler.Visit(@return.Exprs[0]);
          _helper.EndExprScope();
        }
        _helper.Emit(Opcode.RETURN, result, 1, 0, @return.Range);
      } else {
        _helper.BeginExprScope();
        uint listRegister = _helper.DefineTempVariable();
        _exprCompiler.RegisterForSubExpr = listRegister;
        _exprCompiler.Visit(Expression.Tuple(@return.Exprs, @return.Range));
        _helper.Emit(Opcode.RETURN, listRegister, 1, 0, @return.Range);
        _helper.EndExprScope();
      }
    }

    protected override void VisitWhile(WhileStatement @while) {
      _nestedLoopStack.PushFrame();
      _helper.ExprJumpStack.PushFrame();
      int start = _helper.Chunk.Bytecode.Count;
      VisitTest(@while.Test);
      Visit(@while.Body);
      // Doesn't emit single step notifications for this jump instruction, because it's at the same
      // line with the while statement. The single step notification in the first instruction of
      // the while statement will trigger correct single step events.
      _helper.Chunk.Emit(Opcode.JMP, 0, 0, @while.Range);
      _helper.PatchJumpToPos(_helper.Chunk.LatestCodePos, start);
      _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
      _helper.PatchJumpsToCurrentPos(_nestedLoopStack.BreakJumps);
      _helper.PatchJumpsToPos(_nestedLoopStack.ContinueJumps, start);
      _helper.ExprJumpStack.PopFrame();
      _nestedLoopStack.PopFrame();
    }

    protected override void VisitVTag(VTagStatement vTag) {
      _helper.EmitVTagEnteredNotification(vTag, _exprCompiler);
      foreach (Statement statement in vTag.Statements) {
        Visit(statement);
      }
      _helper.EmitVTagExitedNotification(vTag, _exprCompiler);
    }

    private void VisitTest(Expression test) {
      if (test is ComparisonExpression || test is BooleanExpression) {
        _exprCompiler.Visit(test);
      } else {
        if (_helper.GetRegisterId(test) is uint registerId) {
          _helper.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
        } else {
          _helper.BeginExprScope();
          registerId = _helper.DefineTempVariable();
          _exprCompiler.RegisterForSubExpr = registerId;
          _exprCompiler.Visit(test);
          _helper.Emit(Opcode.TEST, registerId, 0, 1, test.Range);
          _helper.EndExprScope();
        }
        _helper.Emit(Opcode.JMP, 0, 0, test.Range);
        _helper.ExprJumpStack.AddFalseJump(_helper.Chunk.LatestCodePos);
      }
    }

    private void Pack(Expression[][] chainedTargets, Expression[] exprs, TextRange range) {
      uint tupleId = VisitExpressionForRegisterId(Expression.Tuple(exprs, range));
      foreach (Expression[] targets in chainedTargets) {
        if (targets.Length == 1) {
          Assign(targets[0], tupleId, range);
        } else if (targets.Length == exprs.Length) {
          UnpackTuple(targets, tupleId, range);
        } else {
          throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", range,
                                        Message.RuntimeErrorIncorrectUnpackCount);
        }
      }
    }

    private void Unpack(Expression[][] chainedTargets, Expression expr, TextRange range) {
      if (chainedTargets.Length == 1 && chainedTargets[0].Length == 1) {
        SimpleAssign(chainedTargets[0][0], expr, range);
      } else {
        uint registerId = VisitExpressionForRegisterId(expr);
        foreach (Expression[] targets in chainedTargets) {
          if (targets.Length == 1) {
            Assign(targets[0], registerId, range);
          } else {
            UnpackTuple(targets, registerId, range);
          }
        }
      }
    }

    // Assigns one value to one target.
    private void SimpleAssign(Expression target, Expression expr, TextRange range) {
      switch (target) {
        case IdentifierExpression id:
          RegisterInfo info = _helper.FindVariable(id.Name);
          switch (info.Type) {
            case RegisterType.Global:
              uint valueId = VisitExpressionForRegisterId(expr);
              _helper.Emit(Opcode.SETGLOB, valueId, info.Id, range);
              _helper.EmitAssignNotification(info.Name, VariableType.Global, valueId, range);
              break;
            case RegisterType.Local:
              _exprCompiler.RegisterForSubExpr = info.Id;
              _exprCompiler.Visit(expr);
              _helper.EmitAssignNotification(info.Name, VariableType.Local, info.Id, range);
              break;
            case RegisterType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          break;
        case SubscriptExpression subscript: {
            uint valueId = VisitExpressionForRKId(expr);
            uint containerId = VisitExpressionForRegisterId(subscript.Container);
            uint keyId = VisitExpressionForRKId(subscript.Key);
            _helper.Emit(Opcode.SETELEM, containerId, keyId, valueId, range);
            _helper.EmitSubscriptAssignNotification(subscript, keyId, valueId, range);
            break;
          }
      }
    }

    // Unpacks a tuple to multiple targets.
    //
    // If the length of targets is less than the one of the unpacked value, SeedPython will unpack
    // part of the value. And if the length of the targets is greater than the one of the unpacked
    // value, an index out of range exception will be thrown.
    // The behavior is different from the original Python. Python will throw an incorrect unpack
    // count exception for both situations.
    // TODO: Add a build-in function to check the length of the unpacked values.
    private void UnpackTuple(Expression[] targets, uint tupleId, TextRange range) {
      _helper.BeginExprScope();
      uint elemId = _helper.DefineTempVariable();
      for (int i = 0; i < targets.Length; i++) {
        _helper.BeginExprScope();
        uint constId = _helper.ConstantCache.IdOfConstant(i);
        uint indexId = _helper.DefineTempVariable();
        _helper.Emit(Opcode.LOADK, indexId, constId, range);
        _helper.Emit(Opcode.GETELEM, elemId, tupleId, indexId, range);
        Assign(targets[i], elemId, range);
        _helper.EndExprScope();
      }
      _helper.EndExprScope();
    }

    private void Assign(Expression target, uint registerId, TextRange range) {
      switch (target) {
        case IdentifierExpression id:
          RegisterInfo info = _helper.FindVariable(id.Name);
          switch (info.Type) {
            case RegisterType.Global:
              _helper.Emit(Opcode.SETGLOB, registerId, info.Id, range);
              _helper.EmitAssignNotification(info.Name, VariableType.Global, registerId, range);
              break;
            case RegisterType.Local:
              _helper.Emit(Opcode.MOVE, info.Id, registerId, 0, range);
              _helper.EmitAssignNotification(info.Name, VariableType.Local, registerId, range);
              break;
            case RegisterType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          break;
        case SubscriptExpression subscript:
          uint containerId = VisitExpressionForRegisterId(subscript.Container);
          uint keyId = VisitExpressionForRKId(subscript.Key);
          _helper.Emit(Opcode.SETELEM, containerId, keyId, registerId, range);
          _helper.EmitSubscriptAssignNotification(subscript, keyId, registerId, range);
          break;
      }
    }

    private RegisterInfo DefineVariableIfNeeded(string name) {
      if (_helper.FindVariable(name) is RegisterInfo info) {
        return info;
      }
      return _helper.DefineVariable(name);
    }

    private void PushFunc(string name) {
      _nestedFuncStack.PushFunc(name);
      CacheTopFunction();
      _helper.BeginFuncScope(name);
    }

    private Function PopFunc() {
      _helper.EndFuncScope();
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
        exprId = _helper.DefineTempVariable();
        _exprCompiler.RegisterForSubExpr = exprId;
        _exprCompiler.Visit(expr);
      }
      return exprId;
    }

    private uint VisitExpressionForRKId(Expression expr) {
      if (!(_helper.GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _helper.DefineTempVariable();
        _exprCompiler.RegisterForSubExpr = exprId;
        _exprCompiler.Visit(expr);
      }
      return exprId;
    }
  }
}
