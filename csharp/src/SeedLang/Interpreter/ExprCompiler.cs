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
using System.Diagnostics;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class ExprCompiler : ExpressionWalker<ExprCompiler.Context> {
    // The context class to encapsulate the target register id and next boolean operator for
    // expression compilation functions.
    internal class Context {
      public bool HasTargetRegister => _targetRegister.HasValue;
      public uint TargetRegister {
        get {
          return (uint)_targetRegister;
        }
        set {
          _targetRegister = value;
        }
      }
      // The next boolean operator. A true condition check instruction is emitted if the next
      // boolean operator is "And", otherwise a false condition instruction check is emitted.
      public BooleanOperator NextBooleanOp;

      private uint? _targetRegister;
    }

    private readonly CompilerHelper _helper;

    internal ExprCompiler(CompilerHelper helper) {
      _helper = helper;
    }

    // Visits an expression and returns the register id of the result.
    internal uint VisitExpressionForRegisterId(Expression expr) {
      if (!(_helper.GetRegisterId(expr) is uint exprId)) {
        exprId = _helper.DefineTempVariable(expr.Range);
        Visit(expr, new Context { TargetRegister = exprId });
      }
      return exprId;
    }

    // Visits an expression and returns the register and constant id of the result.
    internal uint VisitExpressionForRKId(Expression expr) {
      if (!(_helper.GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _helper.DefineTempVariable(expr.Range);
        Visit(expr, new Context { TargetRegister = exprId });
      }
      return exprId;
    }

    // Visits a non-comparison or non-boolean expression and evaluates the boolean result of it.
    internal void VisitExpressionWithBooleanResult(Expression expr, BooleanOperator nextBooleanOp) {
      if (_helper.GetRegisterId(expr) is uint targetRegister) {
        _helper.Emit(Opcode.TEST, targetRegister, 0, 1, expr.Range);
      } else {
        _helper.BeginExprScope();
        targetRegister = _helper.DefineTempVariable(expr.Range);
        Visit(expr, new Context { TargetRegister = targetRegister });
        _helper.Emit(Opcode.TEST, targetRegister, 0, 1, expr.Range);
        _helper.EndExprScope();
      }
      EmitJump(nextBooleanOp, expr.Range);
    }

    protected override void VisitBinary(BinaryExpression binary, Context context) {
      _helper.BeginExprScope();
      uint left = VisitExpressionForRKId(binary.Left);
      uint right = VisitExpressionForRKId(binary.Right);
      _helper.Emit(CompilerHelper.OpcodeOfBinaryOperator(binary.Op), context.TargetRegister, left,
                   right, binary.Range);
      _helper.EmitBinaryNotification(left, binary.Op, right, context.TargetRegister, binary.Range);
      _helper.EndExprScope();
    }

    protected override void VisitBoolean(BooleanExpression boolean, Context context) {
      VisitBooleanOrComparisonExpression(() => {
        for (int i = 0; i < boolean.Exprs.Length; i++) {
          var nextBooleanOp = i < boolean.Exprs.Length - 1 ? boolean.Op : context.NextBooleanOp;
          if (boolean.Exprs[i] is ComparisonExpression comparison) {
            Visit(comparison, new Context { NextBooleanOp = nextBooleanOp, });
            if (i < boolean.Exprs.Length - 1) {
              switch (boolean.Op) {
                case BooleanOperator.And:
                  _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.TrueJumps);
                  break;
                case BooleanOperator.Or:
                  _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
                  break;
              }
            }
          } else {
            VisitExpressionWithBooleanResult(boolean.Exprs[i], nextBooleanOp);
          }
        }
      }, boolean.Range, context);
    }

    protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant,
                                                 Context context) {
      _helper.Emit(Opcode.LOADBOOL, context.TargetRegister, booleanConstant.Value ? 1u : 0, 0,
                   booleanConstant.Range);
    }

    protected override void VisitCall(CallExpression call, Context context) {
      _helper.BeginExprScope();
      // TODO: should call.Func always be IdentifierExpression?
      if (call.Func is IdentifierExpression identifier) {
        if (_helper.FindVariable(identifier.Name) is VariableInfo info) {
          bool needRegister = context.TargetRegister != _helper.LastRegister;
          uint funcRegister = needRegister ? _helper.DefineTempVariable(identifier.Range) :
                                             context.TargetRegister;
          switch (info.Type) {
            case VariableType.Global:
              _helper.Emit(Opcode.GETGLOB, funcRegister, info.Id, identifier.Range);
              break;
            case VariableType.Local:
              _helper.Emit(Opcode.MOVE, funcRegister, info.Id, 0, identifier.Range);
              break;
            case VariableType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          foreach (Expression expr in call.Arguments) {
            Visit(expr, new Context { TargetRegister = _helper.DefineTempVariable(expr.Range) });
          }
          _helper.EmitCall(identifier.Name, funcRegister, (uint)call.Arguments.Length, call.Range);
          if (needRegister) {
            _helper.Emit(Opcode.MOVE, context.TargetRegister, funcRegister, 0, call.Range);
          }
        } else {
          throw new DiagnosticException(SystemReporters.SeedInterpreter, Severity.Error, "",
                                        call.Range, Message.RuntimeErrorVariableNotDefined);
        }
      }
      _helper.EndExprScope();
    }

    protected override void VisitComparison(ComparisonExpression comparison, Context context) {
      Debug.Assert(comparison.Ops.Length > 0 && comparison.Exprs.Length > 0);
      VisitBooleanOrComparisonExpression(() => {
        Expression left = comparison.First;
        for (int i = 0; i < comparison.Exprs.Length; i++) {
          var nextBooleanOp = i < comparison.Exprs.Length - 1 ? BooleanOperator.And :
                                                                context.NextBooleanOp;
          VisitSingleComparison(left, comparison.Ops[i], comparison.Exprs[i], comparison.Range,
                                nextBooleanOp);
          left = comparison.Exprs[i];
        }
      }, comparison.Range, context);
    }

    protected override void VisitDict(DictExpression dict, Context context) {
      _helper.BeginExprScope();
      uint? first = null;
      foreach (var item in dict.KeyValues) {
        uint register = _helper.DefineTempVariable(dict.Range);
        if (!first.HasValue) {
          first = register;
        }
        Visit(item.Key, new Context { TargetRegister = register });
        Visit(item.Value, new Context { TargetRegister = _helper.DefineTempVariable(dict.Range) });
      }
      _helper.Emit(Opcode.NEWDICT, context.TargetRegister, first ?? 0,
                   (uint)dict.KeyValues.Length * 2, dict.Range);
      _helper.EndExprScope();
    }

    protected override void VisitIdentifier(IdentifierExpression identifier, Context context) {
      if (_helper.FindVariable(identifier.Name) is VariableInfo info) {
        switch (info.Type) {
          case VariableType.Global:
            _helper.Emit(Opcode.GETGLOB, context.TargetRegister, info.Id, identifier.Range);
            _helper.EmitGetGlobalNotification(context.TargetRegister, info.Name, identifier.Range);
            break;
          case VariableType.Local:
            _helper.Emit(Opcode.MOVE, context.TargetRegister, info.Id, 0, identifier.Range);
            break;
          case VariableType.Upvalue:
            // TODO: handle upvalues.
            break;
        }
      } else {
        throw new DiagnosticException(SystemReporters.SeedInterpreter, Severity.Fatal, "",
                                      identifier.Range, Message.RuntimeErrorVariableNotDefined);
      }
    }

    protected override void VisitList(ListExpression list, Context context) {
      CreateTupleOrList(Opcode.NEWLIST, list.Exprs, list.Range, context.TargetRegister);
    }

    protected override void VisitNilConstant(NilConstantExpression nilConstant, Context context) {
      _helper.Emit(Opcode.LOADNIL, context.TargetRegister, 1, 0, nilConstant.Range);
    }

    protected override void VisitNumberConstant(NumberConstantExpression numberConstant,
                                                Context context) {
      uint id = _helper.Cache.IdOfConstant(numberConstant.Value);
      _helper.Emit(Opcode.LOADK, context.TargetRegister, id, numberConstant.Range);
    }

    protected override void VisitSlice(SliceExpression slice, Context context) {
      Expression sliceFunc = Expression.Identifier(NativeFunctions.Slice, slice.Range);
      Visit(Expression.Call(sliceFunc, new Expression[] {
        slice.Start ?? Expression.NilConstant(slice.Range),
        slice.Stop ?? Expression.NilConstant(slice.Range),
        slice.Step ?? Expression.NilConstant(slice.Range),
      }, slice.Range), context);
    }

    protected override void VisitStringConstant(StringConstantExpression stringConstant,
                                                Context context) {
      uint id = _helper.Cache.IdOfConstant(stringConstant.Value);
      _helper.Emit(Opcode.LOADK, context.TargetRegister, id, stringConstant.Range);
    }

    protected override void VisitSubscript(SubscriptExpression subscript, Context context) {
      _helper.BeginExprScope();
      uint containerId = VisitExpressionForRegisterId(subscript.Container);
      uint keyId = VisitExpressionForRKId(subscript.Key);
      _helper.Emit(Opcode.GETELEM, context.TargetRegister, containerId, keyId, subscript.Range);
      _helper.EmitGetElementNotification(context.TargetRegister, containerId, keyId,
                                         subscript.Range);
      _helper.EndExprScope();
    }

    protected override void VisitTuple(TupleExpression tuple, Context context) {
      CreateTupleOrList(Opcode.NEWTUPLE, tuple.Exprs, tuple.Range, context.TargetRegister);
    }

    protected override void VisitUnary(UnaryExpression unary, Context context) {
      _helper.BeginExprScope();
      uint exprId = VisitExpressionForRKId(unary.Expr);
      _helper.Emit(Opcode.UNM, context.TargetRegister, exprId, 0, unary.Range);
      _helper.EndExprScope();
    }

    private void VisitBooleanOrComparisonExpression(Action action, TextRange range,
                                                    Context context) {
      // Generates LOADBOOL opcodes if _registerForSubExprStorage is not null, which means the
      // boolean or comparison expression is a sub-expression of other expressions, otherwise it is
      // part of the test condition of if or while statements.
      if (context.HasTargetRegister) {
        _helper.ExprJumpStack.PushFrame();
      }
      action();
      if (context.HasTargetRegister) {
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.TrueJumps);
        // Loads True into the register, and increases PC.
        _helper.Emit(Opcode.LOADBOOL, context.TargetRegister, 1, 1, range);
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
        // Loads False into the register.
        _helper.Emit(Opcode.LOADBOOL, context.TargetRegister, 0, 0, range);
        _helper.ExprJumpStack.PopFrame();
      }
    }

    private void VisitSingleComparison(Expression left, ComparisonOperator op, Expression right,
                                       TextRange range, BooleanOperator nextBooleanOp) {
      _helper.BeginExprScope();
      uint leftRegister = VisitExpressionForRKId(left);
      uint rightRegister = VisitExpressionForRKId(right);
      (Opcode opcode, bool checkFlag) = CompilerHelper.OpcodeAndCheckFlagOfComparisonOperator(op);
      if (nextBooleanOp == BooleanOperator.Or) {
        checkFlag = !checkFlag;
      }
      // Emits a comparison notification for each single comparison before the actual comparison.
      // Two notifications have to be emitted in two different execution paths if emitting after the
      // actual comparison.
      _helper.EmitComparisonNotification(leftRegister, op, rightRegister, range);
      _helper.Emit(opcode, checkFlag ? 1u : 0u, leftRegister, rightRegister, range);
      EmitJump(nextBooleanOp, range);
      _helper.EndExprScope();
    }

    private void CreateTupleOrList(Opcode opcode, IReadOnlyList<Expression> exprs, TextRange range,
                                   uint targetRegister) {
      _helper.BeginExprScope();
      uint? first = null;
      foreach (var expr in exprs) {
        uint register = _helper.DefineTempVariable(range);
        if (!first.HasValue) {
          first = register;
        }
        Visit(expr, new Context { TargetRegister = register });
      }
      _helper.Emit(opcode, targetRegister, first ?? 0, (uint)exprs.Count, range);
      _helper.EndExprScope();
    }

    private void EmitJump(BooleanOperator nextBooleanOp, TextRange range) {
      _helper.Emit(Opcode.JMP, 0, 0, range);
      switch (nextBooleanOp) {
        case BooleanOperator.And:
          _helper.ExprJumpStack.AddFalseJump(_helper.Chunk.LatestCodePos);
          break;
        case BooleanOperator.Or:
          _helper.ExprJumpStack.AddTrueJump(_helper.Chunk.LatestCodePos);
          break;
      }
    }
  }
}
