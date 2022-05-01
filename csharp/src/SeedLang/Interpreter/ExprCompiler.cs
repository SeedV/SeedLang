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

namespace SeedLang.Interpreter {
  // The compiler to convert an AST tree to bytecode.
  internal class ExprCompiler : ExpressionWalker {
    // The register allocated for the result of sub-expressions. The getter resets the storage to
    // null after getting the value to make sure the result register is set before visiting each
    // sub-expression.
    public uint RegisterForSubExpr {
      get {
        Debug.Assert(_registerForSubExprStorage.HasValue,
                     "The result register must be set before visiting sub-expressions.");
        uint value = _registerForSubExprStorage.Value;
        _registerForSubExprStorage = null;
        return value;
      }
      set {
        _registerForSubExprStorage = value;
      }
    }
    private uint? _registerForSubExprStorage;

    private readonly CompilerHelper _helper;

    // The next boolean operator. A true condition check instruction is emitted if the next boolean
    // operator is "And", otherwise a false condition instruction check is emitted.
    private BooleanOperator _nextBooleanOp;

    internal ExprCompiler(CompilerHelper helper) {
      _helper = helper;
    }

    protected override void VisitBinary(BinaryExpression binary) {
      _helper.BeginExpressionScope();
      uint register = RegisterForSubExpr;
      uint left = VisitExpressionForRKId(binary.Left);
      uint right = VisitExpressionForRKId(binary.Right);
      _helper.Emit(CompilerHelper.OpcodeOfBinaryOperator(binary.Op), register, left, right,
                   binary.Range);
      _helper.EmitBinaryNotification(left, binary.Op, right, register, binary.Range);
      _helper.EndExpressionScope();
    }

    protected override void VisitBoolean(BooleanExpression boolean) {
      VisitBooleanOrComparisonExpression(() => {
        BooleanOperator nextBooleanOp = _nextBooleanOp;
        for (int i = 0; i < boolean.Exprs.Length; i++) {
          _nextBooleanOp = i < boolean.Exprs.Length - 1 ? boolean.Op : nextBooleanOp;
          Visit(boolean.Exprs[i]);
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
        }
      }, boolean.Range);
    }

    protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant) {
      _helper.Emit(Opcode.LOADBOOL, RegisterForSubExpr, booleanConstant.Value ? 1u : 0, 0,
                   booleanConstant.Range);
    }

    protected override void VisitCall(CallExpression call) {
      _helper.BeginExpressionScope();
      // TODO: should call.Func always be IdentifierExpression?
      if (call.Func is IdentifierExpression identifier) {
        if (_helper.FindVariable(identifier.Name) is VariableResolver.VariableInfo info) {
          uint resultRegister = RegisterForSubExpr;
          bool needRegister = resultRegister != _helper.LastRegister;
          uint funcRegister = needRegister ? _helper.AllocateRegister() : resultRegister;
          switch (info.Type) {
            case VariableResolver.VariableType.Global:
              _helper.Emit(Opcode.GETGLOB, funcRegister, info.Id, identifier.Range);
              break;
            case VariableResolver.VariableType.Local:
              _helper.Emit(Opcode.MOVE, funcRegister, info.Id, 0, identifier.Range);
              break;
            case VariableResolver.VariableType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          foreach (Expression expr in call.Arguments) {
            RegisterForSubExpr = _helper.AllocateRegister();
            Visit(expr);
          }
          _helper.EmitCall(identifier.Name, funcRegister, (uint)call.Arguments.Length, call.Range);
          if (needRegister) {
            _helper.Emit(Opcode.MOVE, resultRegister, funcRegister, 0, call.Range);
          }
        } else {
          // TODO: throw a variable not defined runtime error.
        }
      }
      _helper.EndExpressionScope();
    }

    protected override void VisitComparison(ComparisonExpression comparison) {
      Debug.Assert(comparison.Ops.Length > 0 && comparison.Exprs.Length > 0);
      VisitBooleanOrComparisonExpression(() => {
        BooleanOperator nextBooleanOp = _nextBooleanOp;
        Expression left = comparison.First;
        for (int i = 0; i < comparison.Exprs.Length; i++) {
          _nextBooleanOp = i < comparison.Exprs.Length - 1 ? BooleanOperator.And : nextBooleanOp;
          VisitSingleComparison(left, comparison.Ops[i], comparison.Exprs[i], comparison.Range);
          left = comparison.Exprs[i];
        }
      }, comparison.Range);
    }

    protected override void VisitDict(DictExpression dict) {
      _helper.BeginExpressionScope();
      uint target = RegisterForSubExpr;
      uint? first = null;
      foreach (var item in dict.Items) {
        uint register = _helper.AllocateRegister();
        if (!first.HasValue) {
          first = register;
        }
        RegisterForSubExpr = register;
        Visit(item.Key);
        RegisterForSubExpr = _helper.AllocateRegister();
        Visit(item.Value);
      }
      _helper.Emit(Opcode.NEWDICT, target, first ?? 0, (uint)dict.Items.Length * 2,
                   dict.Range);
      _helper.EndExpressionScope();
    }

    protected override void VisitIdentifier(IdentifierExpression identifier) {
      if (_helper.FindVariable(identifier.Name) is VariableResolver.VariableInfo info) {
        switch (info.Type) {
          case VariableResolver.VariableType.Global:
            _helper.Emit(Opcode.GETGLOB, RegisterForSubExpr, info.Id, identifier.Range);
            break;
          case VariableResolver.VariableType.Local:
            _helper.Emit(Opcode.MOVE, RegisterForSubExpr, info.Id, 0, identifier.Range);
            break;
          case VariableResolver.VariableType.Upvalue:
            // TODO: handle upvalues.
            break;
        }
      } else {
        throw new DiagnosticException(SystemReporters.SeedInterpreter, Severity.Fatal, "",
                                      identifier.Range, Message.RuntimeErrorVariableNotDefined);
      }
    }

    protected override void VisitList(ListExpression list) {
      CreateTupleOrList(Opcode.NEWLIST, list.Exprs, list.Range);
    }

    protected override void VisitNilConstant(NilConstantExpression nilConstant) {
      _helper.Emit(Opcode.LOADNIL, RegisterForSubExpr, 1, 0, nilConstant.Range);
    }

    protected override void VisitNumberConstant(NumberConstantExpression numberConstant) {
      uint id = _helper.ConstantCache.IdOfConstant(numberConstant.Value);
      _helper.Emit(Opcode.LOADK, RegisterForSubExpr, id, numberConstant.Range);
    }

    protected override void VisitStringConstant(StringConstantExpression stringConstant) {
      uint id = _helper.ConstantCache.IdOfConstant(stringConstant.Value);
      _helper.Emit(Opcode.LOADK, RegisterForSubExpr, id, stringConstant.Range);
    }

    protected override void VisitSubscript(SubscriptExpression subscript) {
      _helper.BeginExpressionScope();
      uint targetId = RegisterForSubExpr;
      uint listId = VisitExpressionForRegisterId(subscript.Expr);
      uint indexId = VisitExpressionForRKId(subscript.Index);
      _helper.Emit(Opcode.GETELEM, targetId, listId, indexId, subscript.Range);
      _helper.EndExpressionScope();
    }

    protected override void VisitTuple(TupleExpression tuple) {
      CreateTupleOrList(Opcode.NEWTUPLE, tuple.Exprs, tuple.Range);
    }

    protected override void VisitUnary(UnaryExpression unary) {
      _helper.BeginExpressionScope();
      uint register = RegisterForSubExpr;
      uint exprId = VisitExpressionForRKId(unary.Expr);
      _helper.Emit(Opcode.UNM, register, exprId, 0, unary.Range);
      _helper.EmitUnaryNotification(unary.Op, exprId, register, unary.Range);
      _helper.EndExpressionScope();
    }

    private void VisitBooleanOrComparisonExpression(Action action, TextRange range) {
      // Generates LOADBOOL opcodes if _registerForSubExprStorage is not null, which means the
      // boolean or comparison expression is a sub-expression of other expressions, otherwise it is
      // part of the test condition of if or while statements.
      uint? register = null;
      if (!(_registerForSubExprStorage is null)) {
        register = RegisterForSubExpr;
        _helper.ExprJumpStack.PushFrame();
      }
      action();
      if (register.HasValue) {
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.TrueJumps);
        // Loads True into the register, and increases PC.
        _helper.Emit(Opcode.LOADBOOL, (uint)register, 1, 1, range);
        _helper.PatchJumpsToCurrentPos(_helper.ExprJumpStack.FalseJumps);
        // Loads False into the register.
        _helper.Emit(Opcode.LOADBOOL, (uint)register, 0, 0, range);
        _helper.ExprJumpStack.PopFrame();
      }
    }

    private void VisitSingleComparison(Expression left, ComparisonOperator op, Expression right,
                                       TextRange range) {
      _helper.BeginExpressionScope();
      uint leftRegister = VisitExpressionForRKId(left);
      uint rightRegister = VisitExpressionForRKId(right);
      (Opcode opcode, bool checkFlag) = CompilerHelper.OpcodeAndCheckFlagOfComparisonOperator(op);
      if (_nextBooleanOp == BooleanOperator.Or) {
        checkFlag = !checkFlag;
      }
      _helper.Emit(opcode, checkFlag ? 1u : 0u, leftRegister, rightRegister, range);
      _helper.Emit(Opcode.JMP, 0, 0, range);
      switch (_nextBooleanOp) {
        case BooleanOperator.And:
          _helper.ExprJumpStack.AddFalseJump(_helper.Chunk.LatestCodePos);
          break;
        case BooleanOperator.Or:
          _helper.ExprJumpStack.AddTrueJump(_helper.Chunk.LatestCodePos);
          break;
      }
      _helper.EndExpressionScope();
    }

    private void CreateTupleOrList(Opcode opcode, IReadOnlyList<Expression> exprs,
                                   TextRange range) {
      _helper.BeginExpressionScope();
      uint target = RegisterForSubExpr;
      uint? first = null;
      foreach (var expr in exprs) {
        uint register = _helper.AllocateRegister();
        if (!first.HasValue) {
          first = register;
        }
        RegisterForSubExpr = register;
        Visit(expr);
      }
      _helper.Emit(opcode, target, first ?? 0, (uint)exprs.Count, range);
      _helper.EndExpressionScope();
    }

    private uint VisitExpressionForRegisterId(Expression expr) {
      if (!(GetRegisterId(expr) is uint exprId)) {
        exprId = _helper.AllocateRegister();
        RegisterForSubExpr = exprId;
        Visit(expr);
      }
      return exprId;
    }

    private uint VisitExpressionForRKId(Expression expr) {
      if (!(GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _helper.AllocateRegister();
        RegisterForSubExpr = exprId;
        Visit(expr);
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
          _helper.FindVariable(identifier.Name) is VariableResolver.VariableInfo info &&
          info.Type == VariableResolver.VariableType.Local) {
        return info.Id;
      }
      return null;
    }

    private uint? GetConstantId(Expression expr) {
      switch (expr) {
        case NumberConstantExpression number:
          return _helper.ConstantCache.IdOfConstant(number.Value);
        case StringConstantExpression str:
          return _helper.ConstantCache.IdOfConstant(str.Value);
        default:
          return null;
      }
    }
  }
}
