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
  internal class Compiler : StatementWalker<NestedLoopStack> {
    private RunMode _runMode;
    private CompilerHelper _helper;
    private ExprCompiler _exprCompiler;

    // The range of the statement that is just compiled.
    private TextRange _rangeOfPrevStatement = null;

    internal Function Compile(Statement program, Module module, VisualizerCenter visualizerCenter,
                              RunMode runMode) {
      _runMode = runMode;
      _helper = new CompilerHelper(visualizerCenter, module);
      _exprCompiler = new ExprCompiler(_helper);
      _helper.PushMainFunc();
      Visit(program, new NestedLoopStack());
      _helper.Chunk.Emit(Opcode.HALT, (uint)HaltReason.Terminated, 0, 0,
                         _rangeOfPrevStatement ?? new TextRange(1, 0, 1, 0));
      return _helper.PopMainFunc();
    }

    protected override void Enter(Statement statement, NestedLoopStack _) {
      _rangeOfPrevStatement = statement.Range;
    }

    protected override void VisitAssignment(AssignmentStatement assignment, NestedLoopStack _) {
      foreach (Expression[] targets in assignment.Targets) {
        foreach (Expression target in targets) {
          if (target is IdentifierExpression id) {
            DefineVariableIfNeeded(id.Name, id.Range);
          }
        }
      }
      _helper.BeginExprScope();
      if (assignment.Values.Length == 1) {
        Unpack(assignment.Targets, assignment.Values[0], assignment.Range);
      } else {
        Pack(assignment.Targets, assignment.Values, assignment.Range);
      }
      _helper.EndExprScope(assignment.Range);
    }

    protected override void VisitBlock(BlockStatement block, NestedLoopStack loopStack) {
      foreach (Statement statement in block.Statements) {
        Visit(statement, loopStack);
      }
    }

    protected override void VisitBreak(BreakStatement @break, NestedLoopStack loopStack) {
      _helper.Emit(Opcode.JMP, 0, 0, @break.Range);
      loopStack.AddBreakJump(_helper.Chunk.LatestCodePos, @break.Range);
    }

    // TODO: implement continue statements.
    protected override void VisitContinue(ContinueStatement @continue, NestedLoopStack loopStack) {
      _helper.Emit(Opcode.JMP, 0, 0, @continue.Range);
      loopStack.AddContinueJump(_helper.Chunk.LatestCodePos, @continue.Range);
    }

    protected override void VisitExpression(ExpressionStatement expr, NestedLoopStack _) {
      _helper.BeginExprScope();
      switch (_runMode) {
        case RunMode.Interactive:
          Expression eval = Expression.Identifier(BuiltinFunctions.PrintVal, expr.Range);
          _exprCompiler.Visit(Expression.Call(eval, new Expression[] { expr.Expr }, expr.Range),
                              new ExprCompiler.Context {
                                TargetRegister = _helper.DefineTempVariable(),
                              });
          break;
        case RunMode.Script:
          _exprCompiler.Visit(expr.Expr, new ExprCompiler.Context {
            TargetRegister = _helper.DefineTempVariable(),
          });
          break;
      }
      _helper.EndExprScope(expr.Range);
    }

    protected override void VisitForIn(ForInStatement forIn, NestedLoopStack loopStack) {
      loopStack.PushFrame();
      VariableInfo loopVar = DefineVariableIfNeeded(forIn.Id.Name, forIn.Id.Range);

      if (!(_helper.GetRegisterId(forIn.Expr) is uint sequence)) {
        sequence = _helper.DefineTempVariable();
        _exprCompiler.Visit(forIn.Expr, new ExprCompiler.Context { TargetRegister = sequence });
      }
      uint index = _helper.DefineTempVariable();
      _helper.Emit(Opcode.LOADK, index, _helper.Cache.IdOfConstant(0), forIn.Range);
      uint limit = _helper.DefineTempVariable();
      _helper.Emit(Opcode.LEN, limit, sequence, 0, forIn.Range);
      uint step = _helper.DefineTempVariable();
      _helper.Emit(Opcode.LOADK, step, _helper.Cache.IdOfConstant(1), forIn.Range);
      _helper.Emit(Opcode.FORPREP, index, 0, forIn.Range);
      int bodyStart = _helper.Chunk.Bytecode.Count;
      switch (loopVar.Type) {
        case VariableType.Global:
          _helper.BeginExprScope();
          uint targetId = _helper.DefineTempVariable();
          _helper.Emit(Opcode.GETELEM, targetId, sequence, index, forIn.Range);
          _helper.Emit(Opcode.SETGLOB, targetId, loopVar.Id, forIn.Range);
          _helper.EmitAssignNotification(loopVar.Name, VariableType.Global, targetId,
                                         forIn.Id.Range);
          _helper.EndExprScope(forIn.Id.Range);
          break;
        case VariableType.Local:
          _helper.Emit(Opcode.GETELEM, loopVar.Id, sequence, index, forIn.Range);
          _helper.EmitAssignNotification(loopVar.Name, VariableType.Local, loopVar.Id,
                                         forIn.Id.Range);
          break;
        case VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
      Visit(forIn.Body, loopStack);
      // Patches all continue jumps to the FORLOOP instruction.
      _helper.PatchJumpsToCurrentPos(loopStack.ContinueJumps);
      _helper.Emit(Opcode.FORLOOP, index, 0, forIn.Range);
      // Patches the jump position of the FORLOOP instruction to the start point of the body.
      _helper.PatchJumpToPos(_helper.Chunk.LatestCodePos, bodyStart);
      // Patches the jump position of the FORPREP instruction to the latest FORLOOP.
      _helper.PatchJumpToPos(bodyStart - 1, _helper.Chunk.LatestCodePos);
      _helper.PatchJumpsToCurrentPos(loopStack.BreakJumps);
      loopStack.PopFrame();
    }

    protected override void VisitFuncDef(FuncDefStatement funcDef, NestedLoopStack loopStack) {
      VariableInfo info = DefineVariableIfNeeded(funcDef.Name, funcDef.Range);
      _helper.PushFunc(funcDef.Name);
      foreach (IdentifierExpression parameter in funcDef.Parameters) {
        _helper.DefineVariable(parameter.Name, parameter.Range);
      }
      Visit(funcDef.Body, loopStack);
      _helper.Emit(Opcode.RETURN, 0, 0, 0, _rangeOfPrevStatement ?? new TextRange(1, 0, 1, 0));

      Function func = _helper.PopFunc();
      uint funcId = _helper.Cache.IdOfConstant(func);
      switch (info.Type) {
        case VariableType.Global:
          _helper.BeginExprScope();
          uint registerId = _helper.DefineTempVariable();
          _helper.Emit(Opcode.LOADK, registerId, funcId, funcDef.Range);
          _helper.Emit(Opcode.SETGLOB, registerId, info.Id, funcDef.Range);
          _helper.EndExprScope(funcDef.Range);
          break;
        case VariableType.Local:
          _helper.Emit(Opcode.LOADK, info.Id, funcId, funcDef.Range);
          break;
        case VariableType.Upvalue:
          // TODO: handle upvalues.
          break;
      }
    }

    protected override void VisitIf(IfStatement @if, NestedLoopStack loopStack) {
      _helper.ExprJumpStack.PushFrame();
      VisitTest(@if.Test);
      _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.TrueJumps);
      Visit(@if.ThenBody, loopStack);
      if (!(@if.ElseBody is null)) {
        _helper.Emit(Opcode.JMP, 0, 0, @if.Range);
        int jumpEndPos = _helper.Chunk.LatestCodePos;
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
        Visit(@if.ElseBody, loopStack);
        _helper.PatchJumpToCurrentPos(jumpEndPos);
      } else {
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
      }
      _helper.ExprJumpStack.PopFrame();
    }

    protected override void VisitImport(ImportStatement import, NestedLoopStack _) {
      throw new System.NotImplementedException();
    }

    protected override void VisitPass(PassStatement pass, NestedLoopStack _) { }

    protected override void VisitReturn(ReturnStatement @return, NestedLoopStack _) {
      if (@return.Exprs.Length == 0) {
        _helper.Emit(Opcode.RETURN, 0, 0, 0, @return.Range);
      } else if (@return.Exprs.Length == 1) {
        _helper.BeginExprScope();
        if (!(_helper.GetRegisterId(@return.Exprs[0]) is uint result)) {
          result = _helper.DefineTempVariable();
          _exprCompiler.Visit(@return.Exprs[0], new ExprCompiler.Context {
            TargetRegister = result,
          });
        }
        _helper.Emit(Opcode.RETURN, result, 1, 0, @return.Range);
        _helper.EndExprScope(@return.Range);
      } else {
        _helper.BeginExprScope();
        uint targetRegister = _helper.DefineTempVariable();
        _exprCompiler.Visit(Expression.Tuple(@return.Exprs, @return.Range),
                            new ExprCompiler.Context { TargetRegister = targetRegister });
        _helper.Emit(Opcode.RETURN, targetRegister, 1, 0, @return.Range);
        _helper.EndExprScope(@return.Range);
      }
    }

    protected override void VisitWhile(WhileStatement @while, NestedLoopStack loopStack) {
      loopStack.PushFrame();
      _helper.ExprJumpStack.PushFrame();
      int start = _helper.Chunk.Bytecode.Count;
      VisitTest(@while.Test);
      Visit(@while.Body, loopStack);
      // Doesn't emit single step notifications for this jump instruction, because it's at the same
      // line with the while statement. The single step notification in the first instruction of
      // the while statement will trigger correct single step events.
      _helper.Chunk.Emit(Opcode.JMP, 0, 0, @while.Range);
      _helper.PatchJumpToPos(_helper.Chunk.LatestCodePos, start);
      _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
      _helper.PatchJumpsToCurrentPos(loopStack.BreakJumps);
      _helper.PatchJumpsToPos(loopStack.ContinueJumps, start);
      _helper.ExprJumpStack.PopFrame();
      loopStack.PopFrame();
    }

    protected override void VisitVTag(VTagStatement vTag, NestedLoopStack loopStack) {
      _helper.EmitVTagEnteredNotification(vTag, _exprCompiler);
      foreach (Statement statement in vTag.Statements) {
        Visit(statement, loopStack);
      }
      _helper.EmitVTagExitedNotification(vTag, _exprCompiler);
    }

    private void VisitTest(Expression test) {
      if (test is ComparisonExpression || test is BooleanExpression) {
        _exprCompiler.Visit(test, new ExprCompiler.Context());
      } else {
        _exprCompiler.VisitExpressionWithBooleanResult(test, BooleanOperator.And);
      }
    }

    private void Pack(Expression[][] chainedTargets, Expression[] values, TextRange range) {
      foreach (Expression[] targets in chainedTargets) {
        if (targets.Length == 1) {
          var tuple = Expression.Tuple(values, range);
          Assign(targets[0], null, _exprCompiler.VisitExpressionForRegisterId(tuple), range);
        } else {
          MultipleAssign(targets, values, range);
        }
      }
    }

    private void Unpack(Expression[][] chainedTargets, Expression value, TextRange range) {
      if (chainedTargets.Length == 1 && chainedTargets[0].Length == 1) {
        Assign(chainedTargets[0][0], value, 0, range);
      } else {
        uint registerId = _exprCompiler.VisitExpressionForRegisterId(value);
        foreach (Expression[] targets in chainedTargets) {
          if (targets.Length == 1) {
            Assign(targets[0], null, registerId, range);
          } else {
            UnpackTuple(targets, registerId, range);
          }
        }
      }
    }

    // Unpacks a tuple and assigns to multiple targets.
    //
    // If the length of targets is less than the one of the unpacked value, SeedPython will unpack
    // part of the value. And if the length of the targets is greater than the one of the unpacked
    // value, an index out of range exception will be thrown.
    // The behavior is different from the original Python. Python will throw an incorrect unpack
    // count exception for both situations.
    // TODO: Add a build-in function to check the length of the unpacked values.
    private void UnpackTuple(Expression[] targets, uint tupleId, TextRange range) {
      uint elemId = _helper.DefineTempVariable();
      for (int i = 0; i < targets.Length; i++) {
        _helper.BeginExprScope();
        uint constId = _helper.Cache.IdOfConstant(i);
        uint indexId = _helper.DefineTempVariable();
        _helper.Emit(Opcode.LOADK, indexId, constId, range);
        _helper.Emit(Opcode.GETELEM, elemId, tupleId, indexId, range);
        Assign(targets[i], null, elemId, range);
        _helper.EndExprScope(range);
      }
    }

    private void MultipleAssign(Expression[] targets, Expression[] values, TextRange range) {
      if (targets.Length == values.Length) {
        var valueIds = new uint[values.Length];
        for (int i = 0; i < values.Length; i++) {
          valueIds[i] = _exprCompiler.VisitExpressionForRegisterId(values[i]);
        }
        for (int i = 0; i < targets.Length; i++) {
          Assign(targets[i], null, valueIds[i], range);
        }
      } else {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", range,
                                      Message.RuntimeErrorIncorrectUnpackCount);
      }
    }

    // Assigns the value of an expression or a register (when the expression is null) to the target.
    // The implementation can be optimized to reduce a MOVE instruction if the expression is
    // provided.
    private void Assign(Expression target, Expression value, uint registerId, TextRange range) {
      switch (target) {
        case IdentifierExpression id:
          VariableInfo info = _helper.FindVariable(id.Name);
          switch (info.Type) {
            case VariableType.Global:
              if (!(value is null)) {
                registerId = _exprCompiler.VisitExpressionForRegisterId(value);
              }
              _helper.Emit(Opcode.SETGLOB, registerId, info.Id, range);
              _helper.EmitAssignNotification(info.Name, VariableType.Global, registerId, range);
              break;
            case VariableType.Local:
              if (!(value is null)) {
                _exprCompiler.Visit(value, new ExprCompiler.Context { TargetRegister = info.Id });
                registerId = info.Id;
              } else {
                _helper.Emit(Opcode.MOVE, info.Id, registerId, 0, range);
              }
              _helper.EmitAssignNotification(info.Name, VariableType.Local, registerId, range);
              break;
            case VariableType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          break;
        case SubscriptExpression subscript:
          if (!(value is null)) {
            registerId = _exprCompiler.VisitExpressionForRKId(value);
          }
          uint containerId = _exprCompiler.VisitExpressionForRegisterId(subscript.Container);
          uint keyId = _exprCompiler.VisitExpressionForRKId(subscript.Key);
          _helper.Emit(Opcode.SETELEM, containerId, keyId, registerId, range);
          _helper.EmitSubscriptAssignNotification(containerId, keyId, registerId, range);
          break;
      }
    }

    private VariableInfo DefineVariableIfNeeded(string name, TextRange range) {
      if (_helper.FindVariable(name) is VariableInfo info) {
        return info;
      }
      return _helper.DefineVariable(name, range);
    }
  }
}
