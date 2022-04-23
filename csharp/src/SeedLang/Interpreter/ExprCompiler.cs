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
    private readonly VisualizerCenter _visualizerCenter;
    private readonly VariableResolver _variableResolver;
    private readonly NestedJumpStack _nestedJumpStack;

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

    // The next boolean operator. A true condition check instruction is emitted if the next boolean
    // operator is "And", otherwise a false condition instruction check is emitted.
    private BooleanOperator _nextBooleanOp;

    // The chunk on the top of the function stack.
    private Chunk _chunk;
    // The constant cache on the top of the function stack.
    private ConstantCache _constantCache;

    internal ExprCompiler(VisualizerCenter vc, VariableResolver vr, NestedJumpStack njs) {
      _visualizerCenter = vc;
      _variableResolver = vr;
      _nestedJumpStack = njs;
    }

    internal void SetChunk(Chunk chunk) {
      _chunk = chunk;
    }

    internal void SetConstantCache(ConstantCache cache) {
      _constantCache = cache;
    }

    protected override void VisitBinary(BinaryExpression binary) {
      _variableResolver.BeginExpressionScope();
      uint register = RegisterForSubExpr;
      uint left = VisitExpressionForRKId(binary.Left);
      uint right = VisitExpressionForRKId(binary.Right);
      _chunk.Emit(OpcodeOfBinaryOperator(binary.Op), register, left, right, binary.Range);
      EmitBinaryNotification(left, binary.Op, right, register, binary.Range);
      _variableResolver.EndExpressionScope();
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
                PatchJumps(_nestedJumpStack.TrueJumps);
                break;
              case BooleanOperator.Or:
                PatchJumps(_nestedJumpStack.FalseJumps);
                break;
            }
          }
        }
      }, boolean.Range);
    }

    protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant) {
      _chunk.Emit(Opcode.LOADBOOL, RegisterForSubExpr, booleanConstant.Value ? 1u : 0, 0,
                  booleanConstant.Range);
    }

    protected override void VisitCall(CallExpression call) {
      _variableResolver.BeginExpressionScope();
      // TODO: should call.Func always be IdentifierExpression?
      if (call.Func is IdentifierExpression identifier) {
        if (_variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info) {
          uint resultRegister = RegisterForSubExpr;
          bool needRegister = resultRegister != _variableResolver.LastRegister;
          uint funcRegister = needRegister ? _variableResolver.AllocateRegister() : resultRegister;
          switch (info.Type) {
            case VariableResolver.VariableType.Global:
              _chunk.Emit(Opcode.GETGLOB, funcRegister, info.Id, identifier.Range);
              break;
            case VariableResolver.VariableType.Local:
              _chunk.Emit(Opcode.MOVE, funcRegister, info.Id, 0, identifier.Range);
              break;
            case VariableResolver.VariableType.Upvalue:
              // TODO: handle upvalues.
              break;
          }
          foreach (Expression expr in call.Arguments) {
            RegisterForSubExpr = _variableResolver.AllocateRegister();
            Visit(expr);
          }
          EmitCall(identifier.Name, funcRegister, (uint)call.Arguments.Length, call.Range);
          if (needRegister) {
            _chunk.Emit(Opcode.MOVE, resultRegister, funcRegister, 0, call.Range);
          }
        } else {
          // TODO: throw a variable not defined runtime error.
        }
      }
      _variableResolver.EndExpressionScope();
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
      _variableResolver.BeginExpressionScope();
      uint target = RegisterForSubExpr;
      uint? first = null;
      foreach (var item in dict.Items) {
        uint register = _variableResolver.AllocateRegister();
        if (!first.HasValue) {
          first = register;
        }
        RegisterForSubExpr = register;
        Visit(item.Key);
        RegisterForSubExpr = _variableResolver.AllocateRegister();
        Visit(item.Value);
      }
      _chunk.Emit(Opcode.NEWDICT, target, first ?? 0, (uint)dict.Items.Length * 2, dict.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void VisitIdentifier(IdentifierExpression identifier) {
      if (_variableResolver.FindVariable(identifier.Name) is VariableResolver.VariableInfo info) {
        switch (info.Type) {
          case VariableResolver.VariableType.Global:
            _chunk.Emit(Opcode.GETGLOB, RegisterForSubExpr, info.Id, identifier.Range);
            break;
          case VariableResolver.VariableType.Local:
            _chunk.Emit(Opcode.MOVE, RegisterForSubExpr, info.Id, 0, identifier.Range);
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
      _chunk.Emit(Opcode.LOADNIL, RegisterForSubExpr, 1, 0, nilConstant.Range);
    }

    protected override void VisitNumberConstant(NumberConstantExpression numberConstant) {
      uint id = _constantCache.IdOfConstant(numberConstant.Value);
      _chunk.Emit(Opcode.LOADK, RegisterForSubExpr, id, numberConstant.Range);
    }

    protected override void VisitStringConstant(StringConstantExpression stringConstant) {
      uint id = _constantCache.IdOfConstant(stringConstant.Value);
      _chunk.Emit(Opcode.LOADK, RegisterForSubExpr, id, stringConstant.Range);
    }

    protected override void VisitSubscript(SubscriptExpression subscript) {
      _variableResolver.BeginExpressionScope();
      uint targetId = RegisterForSubExpr;
      uint listId = VisitExpressionForRegisterId(subscript.Expr);
      uint indexId = VisitExpressionForRKId(subscript.Index);
      _chunk.Emit(Opcode.GETELEM, targetId, listId, indexId, subscript.Range);
      _variableResolver.EndExpressionScope();
    }

    protected override void VisitTuple(TupleExpression tuple) {
      CreateTupleOrList(Opcode.NEWTUPLE, tuple.Exprs, tuple.Range);
    }

    protected override void VisitUnary(UnaryExpression unary) {
      _variableResolver.BeginExpressionScope();
      uint register = RegisterForSubExpr;
      uint exprId = VisitExpressionForRKId(unary.Expr);
      _chunk.Emit(Opcode.UNM, register, exprId, 0, unary.Range);
      EmitUnaryNotification(unary.Op, exprId, register, unary.Range);
      _variableResolver.EndExpressionScope();
    }

    private void VisitBooleanOrComparisonExpression(Action action, TextRange range) {
      // Generates LOADBOOL opcodes if _registerForSubExprStorage is not null, which means the
      // boolean or comparison expression is a sub-expression of other expressions, otherwise it is
      // part of the test condition of if or while statements.
      uint? register = null;
      if (!(_registerForSubExprStorage is null)) {
        register = RegisterForSubExpr;
        _nestedJumpStack.PushFrame();
      }
      action();
      if (register.HasValue) {
        PatchJumps(_nestedJumpStack.TrueJumps);
        // Loads True into the register, and increases PC.
        _chunk.Emit(Opcode.LOADBOOL, (uint)register, 1, 1, range);
        PatchJumps(_nestedJumpStack.FalseJumps);
        // Loads False into the register.
        _chunk.Emit(Opcode.LOADBOOL, (uint)register, 0, 0, range);
        _nestedJumpStack.PopFrame();
      }
    }

    private void VisitSingleComparison(Expression left, ComparisonOperator op, Expression right,
                                       TextRange range) {
      _variableResolver.BeginExpressionScope();
      uint leftRegister = VisitExpressionForRKId(left);
      uint rightRegister = VisitExpressionForRKId(right);
      (Opcode opcode, bool checkFlag) = OpcodeAndCheckFlagOfComparisonOperator(op);
      if (_nextBooleanOp == BooleanOperator.Or) {
        checkFlag = !checkFlag;
      }
      _chunk.Emit(opcode, checkFlag ? 1u : 0u, leftRegister, rightRegister, range);
      _chunk.Emit(Opcode.JMP, 0, 0, range);
      int jump = GetCurrentCodePos();
      switch (_nextBooleanOp) {
        case BooleanOperator.And:
          _nestedJumpStack.FalseJumps.Add(jump);
          break;
        case BooleanOperator.Or:
          _nestedJumpStack.TrueJumps.Add(jump);
          break;
      }
      _variableResolver.EndExpressionScope();
    }

    private void CreateTupleOrList(Opcode opcode, IReadOnlyList<Expression> exprs,
                                   TextRange range) {
      _variableResolver.BeginExpressionScope();
      uint target = RegisterForSubExpr;
      uint? first = null;
      foreach (var expr in exprs) {
        uint register = _variableResolver.AllocateRegister();
        if (!first.HasValue) {
          first = register;
        }
        RegisterForSubExpr = register;
        Visit(expr);
      }
      _chunk.Emit(opcode, target, first ?? 0, (uint)exprs.Count, range);
      _variableResolver.EndExpressionScope();
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

    private uint VisitExpressionForRegisterId(Expression expr) {
      if (!(GetRegisterId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateRegister();
        RegisterForSubExpr = exprId;
        Visit(expr);
      }
      return exprId;
    }

    private uint VisitExpressionForRKId(Expression expr) {
      if (!(GetRegisterOrConstantId(expr) is uint exprId)) {
        exprId = _variableResolver.AllocateRegister();
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

    // Emits a CALL instruction. A VISNOTIFY instruction is also emitted if there are visualizers
    // for the FuncCalled event.
    private void EmitCall(string name, uint funcRegister, uint argLength, TextRange range) {
      bool isNormalFunc = !NativeFunctions.IsInternalFunction(name);
      bool notifyCalled = isNormalFunc && _visualizerCenter.HasVisualizer<Event.FuncCalled>();
      bool notifyReturned = isNormalFunc && _visualizerCenter.HasVisualizer<Event.FuncReturned>();
      uint nId = 0;
      if (notifyCalled || notifyReturned) {
        var notification = new Notification.Function(name, funcRegister, argLength, range);
        nId = _chunk.AddNotification(notification);
      }
      if (notifyCalled) {
        _chunk.Emit(Opcode.VISNOTIFY, (uint)Notification.Function.Status.Called, nId, range);
      }
      _chunk.Emit(Opcode.CALL, funcRegister, argLength, 0, range);
      if (notifyReturned) {
        _chunk.Emit(Opcode.VISNOTIFY, (uint)Notification.Function.Status.Returned, nId, range);
      }
    }

    private void EmitBinaryNotification(uint leftId, BinaryOperator op, uint rightId, uint resultId,
                                        TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.Binary>()) {
        var notification = new Notification.Binary(leftId, op, rightId, resultId, range);
        _chunk.Emit(Opcode.VISNOTIFY, 0, _chunk.AddNotification(notification), range);
      }
    }

    private void EmitUnaryNotification(UnaryOperator op, uint valueId, uint resultId,
                                       TextRange range) {
      if (_visualizerCenter.HasVisualizer<Event.Unary>()) {
        var notification = new Notification.Unary(op, valueId, resultId, range);
        _chunk.Emit(Opcode.VISNOTIFY, 0, _chunk.AddNotification(notification), range);
      }
    }

    private static Opcode OpcodeOfBinaryOperator(BinaryOperator op) {
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

    private static (Opcode, bool) OpcodeAndCheckFlagOfComparisonOperator(ComparisonOperator op) {
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
  }
}
