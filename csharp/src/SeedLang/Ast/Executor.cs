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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // An executor class to execute a program represented by an AST tree.
  internal sealed class Executor : AstWalker {
    private readonly Sys _sys = new Sys();
    private RunMode _runMode;
    // The visualizer center to observe AST execution events and dispatch them to the registered
    // visualizers.
    private readonly VisualizerCenter _visualizerCenter;
    // The environment to store global and local variables.
    private readonly ScopedEnvironment _env = new ScopedEnvironment();
    // The result of current executed expression.
    private Value _expressionResult;

    internal Executor(VisualizerCenter visualizerCenter = null) {
      foreach (var func in NativeFunctions.Funcs) {
        _env.SetVariable(func.Name, new Value(func));
      }
      _visualizerCenter = visualizerCenter ?? new VisualizerCenter();
    }

    internal void RedirectStdout(TextWriter stdout) {
      _sys.Stdout = stdout;
    }

    // Executes the given AST tree.
    internal void Run(AstNode node, RunMode runMode) {
      _runMode = runMode;
      Visit(node);
    }

    // Calls an AST function with given arguments that locate in the "args" array starting from
    // "offset". The number of arguments is "length".
    internal Value Call(FuncDefStatement funcDef, Value[] args, int offset, int length) {
      if (funcDef.Parameters.Length != length) {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", null,
                                      Message.RuntimeErrorIncorrectArgsCount);
      }
      _env.EnterScope();
      for (int i = 0; i < funcDef.Parameters.Length; i++) {
        _env.SetVariable(funcDef.Parameters[i], args[offset + i]);
      }
      var result = new Value();
      try {
        Visit(funcDef.Body);
      } catch (ReturnException returnException) {
        result = returnException.Result;
      }
      _env.ExitScope();
      return result;
    }

    protected override void Visit(BinaryExpression binary) {
      Visit(binary.Left);
      Value left = _expressionResult;
      Visit(binary.Right);
      Value right = _expressionResult;
      try {
        switch (binary.Op) {
          case BinaryOperator.Add:
            _expressionResult = ValueHelper.Add(left, right);
            break;
          case BinaryOperator.Subtract:
            _expressionResult = ValueHelper.Subtract(left, right);
            break;
          case BinaryOperator.Multiply:
            _expressionResult = ValueHelper.Multiply(left, right);
            break;
          case BinaryOperator.Divide:
            _expressionResult = ValueHelper.Divide(left, right);
            break;
          case BinaryOperator.FloorDivide:
            _expressionResult = ValueHelper.FloorDivide(left, right);
            break;
          case BinaryOperator.Power:
            _expressionResult = ValueHelper.Power(left, right);
            break;
          case BinaryOperator.Modulo:
            _expressionResult = ValueHelper.Modulo(left, right);
            break;
          default:
            throw new System.NotImplementedException($"Unsupported binary operator: {binary.Op}");
        }
      } catch (DiagnosticException ex) {
        // Throws a new diagnostic exception with more information.
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, binary.Range,
                                      ex.Diagnostic.MessageId);
      }
      if (!_visualizerCenter.BinaryPublisher.IsEmpty()) {
        var be = new BinaryEvent(new ValueWrapper(left), binary.Op, new ValueWrapper(right),
                                 new ValueWrapper(_expressionResult), binary.Range);
        _visualizerCenter.BinaryPublisher.Notify(be);
      }
    }

    protected override void Visit(BooleanExpression boolean) {
      var values = new Value[boolean.Exprs.Length];
      bool shortCircuitCondition = boolean.Op == BooleanOperator.Or;
      for (int i = 0; i < boolean.Exprs.Length; ++i) {
        Visit(boolean.Exprs[i]);
        values[i] = _expressionResult;
        if (_expressionResult.AsBoolean() == shortCircuitCondition) {
          break;
        }
      }
      if (!_visualizerCenter.BooleanPublisher.IsEmpty()) {
        var vs = System.Array.ConvertAll(values, value => new ValueWrapper(value));
        var be = new BooleanEvent(boolean.Op, vs, new ValueWrapper(_expressionResult),
                                  boolean.Range);
        _visualizerCenter.BooleanPublisher.Notify(be);
      }
    }

    protected override void Visit(ComparisonExpression comparison) {
      Visit(comparison.First);
      Value first = _expressionResult;
      var values = new Value[comparison.Exprs.Length];
      bool currentResult = true;
      for (int i = 0; i < comparison.Ops.Length; ++i) {
        Visit(comparison.Exprs[i]);
        values[i] = _expressionResult;
        Value left = i > 0 ? values[i - 1] : first;
        switch (comparison.Ops[i]) {
          case ComparisonOperator.Less:
            currentResult = ValueHelper.Less(left, values[i]);
            break;
          case ComparisonOperator.Greater:
            currentResult = !ValueHelper.LessEqual(left, values[i]);
            break;
          case ComparisonOperator.LessEqual:
            currentResult = ValueHelper.LessEqual(left, values[i]);
            break;
          case ComparisonOperator.GreaterEqual:
            currentResult = !ValueHelper.Less(left, values[i]);
            break;
          case ComparisonOperator.EqEqual:
            currentResult = left.Equals(values[i]);
            break;
          case ComparisonOperator.NotEqual:
            currentResult = !left.Equals(values[i]);
            break;
          case ComparisonOperator.In:
            currentResult = ValueHelper.Contains(values[i], left);
            break;
          default:
            throw new System.NotImplementedException(
                $"Unsupported comparison operator: {comparison.Ops[i]}");
        }
        if (!currentResult) {
          break;
        }
      }
      _expressionResult = new Value(currentResult);
      if (!_visualizerCenter.ComparisonPublisher.IsEmpty()) {
        var vs = System.Array.ConvertAll(values, value => new ValueWrapper(value));
        var ce = new ComparisonEvent(new ValueWrapper(first), comparison.Ops, vs,
                                     new ValueWrapper(_expressionResult), comparison.Range);
        _visualizerCenter.ComparisonPublisher.Notify(ce);
      }
    }

    protected override void Visit(UnaryExpression unary) {
      Visit(unary.Expr);
      Value value = _expressionResult;
      if (unary.Op == UnaryOperator.Negative) {
        _expressionResult = new Value(value.AsNumber() * -1);
      } else if (unary.Op == UnaryOperator.Not) {
        _expressionResult = new Value(!value.AsBoolean());
      }
      if (!_visualizerCenter.UnaryPublisher.IsEmpty()) {
        var ue = new UnaryEvent(unary.Op, new ValueWrapper(value),
                                new ValueWrapper(_expressionResult), unary.Range);
        _visualizerCenter.UnaryPublisher.Notify(ue);
      }
    }

    protected override void Visit(IdentifierExpression identifier) {
      _expressionResult = _env.GetVariable(identifier.Name);
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      _expressionResult = new Value(booleanConstant.Value);
    }

    protected override void Visit(NilConstantExpression nilConstant) {
      _expressionResult = new Value();
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      try {
        _expressionResult = new Value(numberConstant.Value);
      } catch (DiagnosticException ex) {
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, numberConstant.Range,
                                      ex.Diagnostic.MessageId);
      }
    }

    protected override void Visit(StringConstantExpression stringConstant) {
      _expressionResult = new Value(stringConstant.Value);
    }

    protected override void Visit(DictExpression dict) {
      var initialValues = new Dictionary<Value, Value>();
      foreach (var item in dict.Items) {
        Visit(item.Key);
        Value key = _expressionResult;
        Visit(item.Value);
        initialValues[key] = _expressionResult;
      }
      _expressionResult = new Value(initialValues);
    }

    protected override void Visit(ListExpression list) {
      var initialValues = new List<Value>(list.Exprs.Length);
      foreach (Expression expr in list.Exprs) {
        Visit(expr);
        initialValues.Add(_expressionResult);
      }
      _expressionResult = new Value(initialValues);
    }

    protected override void Visit(TupleExpression list) {
      var builder = ImmutableArray.CreateBuilder<Value>(list.Exprs.Length);
      for (int i = 0; i < list.Exprs.Length; i++) {
        Visit(list.Exprs[i]);
        builder.Add(_expressionResult);
      }
      _expressionResult = new Value(builder.MoveToImmutable());
    }

    protected override void Visit(SubscriptExpression subscript) {
      Visit(subscript.Expr);
      Value list = _expressionResult;
      Visit(subscript.Index);
      try {
        _expressionResult = list[_expressionResult];
      } catch (DiagnosticException ex) {
        throw new DiagnosticException(SystemReporters.SeedAst, ex.Diagnostic.Severity,
                                      ex.Diagnostic.Module, subscript.Range,
                                      ex.Diagnostic.MessageId);
      }
    }

    protected override void Visit(CallExpression call) {
      Visit(call.Func);
      Value funcValue = _expressionResult;
      var values = new Value[call.Arguments.Length];
      for (int i = 0; i < call.Arguments.Length; i++) {
        Visit(call.Arguments[i]);
        values[i] = _expressionResult;
      }
      switch (funcValue.AsFunction()) {
        case HeapObject.NativeFunction nativeFunc:
          _expressionResult = nativeFunc.Call(values, 0, values.Length, _sys);
          break;
        case Function func:
          _expressionResult = Call(func.FuncDef, values, 0, values.Length);
          break;
      }
    }

    protected override void Visit(AssignmentStatement assignment) {
      if (assignment.Exprs.Length == 1) {
        Unpack(assignment.Targets, assignment.Exprs[0], assignment.Range);
      } else {
        Pack(assignment.Targets, assignment.Exprs, assignment.Range);
      }
    }

    protected override void Visit(BlockStatement block) {
      foreach (Statement statement in block.Statements) {
        Visit(statement);
      }
    }

    protected override void Visit(ExpressionStatement expr) {
      switch (_runMode) {
        case RunMode.Interactive:
          Expression eval = Expression.Identifier(NativeFunctions.PrintVal, expr.Range);
          Visit(Expression.Call(eval, new Expression[] { expr.Expr }, expr.Range));
          break;
        case RunMode.Script:
          Visit(expr.Expr);
          break;
      }
    }

    protected override void Visit(ForInStatement forIn) {
      Visit(forIn.Expr);
      Value sequence = _expressionResult;
      for (int i = 0; i < sequence.Length; i++) {
        _env.SetVariable(forIn.Id.Name, sequence[new Value(i)]);
        Visit(forIn.Body);
      }
    }

    protected override void Visit(FuncDefStatement funcDef) {
      _env.SetVariable(funcDef.Name, new Value(new Function(funcDef)));
    }

    protected override void Visit(IfStatement @if) {
      Visit(@if.Test);
      if (_expressionResult.AsBoolean()) {
        Visit(@if.ThenBody);
      } else {
        if (!(@if.ElseBody is null)) {
          Visit(@if.ElseBody);
        }
      }
    }

    protected override void Visit(PassStatement pass) {
    }

    protected override void Visit(ReturnStatement @return) {
      // Throws a return exception carried with the result value to break current execution flow and
      // return from current function.
      if (@return.Exprs.Length == 0) {
        throw new ReturnException(new Value());
      } else if (@return.Exprs.Length == 1) {
        Visit(@return.Exprs[0]);
        throw new ReturnException(_expressionResult);
      } else {
        // The result value is a tuple that holds all the return values if there are multiple
        // return values.
        var values = new Value[@return.Exprs.Length];
        for (int i = 0; i < @return.Exprs.Length; i++) {
          Visit(@return.Exprs[i]);
          values[i] = _expressionResult;
        }
        throw new ReturnException(new Value(values));
      }
    }

    protected override void Visit(WhileStatement @while) {
      while (true) {
        Visit(@while.Test);
        if (_expressionResult.AsBoolean()) {
          Visit(@while.Body);
        } else {
          break;
        }
      }
    }

    protected override void Visit(VTagStatement vTag) { }

    private void Pack(Expression[] targets, Expression[] exprs, Range range) {
      var builder = ImmutableArray.CreateBuilder<Value>(exprs.Length);
      for (int i = 0; i < exprs.Length; i++) {
        Visit(exprs[i]);
        builder.Add(_expressionResult);
      }
      ImmutableArray<Value> values = builder.MoveToImmutable();
      if (targets.Length == 1) {
        Assign(targets[0], new Value(values), range);
      } else if (targets.Length == values.Length) {
        for (int i = 0; i < targets.Length; i++) {
          Assign(targets[i], values[i], range);
        }
      } else {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", range,
                                      Message.RuntimeErrorIncorrectUnpackCount);
      }
    }

    private void Unpack(Expression[] targets, Expression expr, Range range) {
      Visit(expr);
      Value value = _expressionResult;
      if (targets.Length == 1) {
        Assign(targets[0], value, range);
      } else if (targets.Length == value.Length) {
        for (int i = 0; i < targets.Length; i++) {
          Assign(targets[i], value[new Value(i)], range);
        }
      } else {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", range,
                                      Message.RuntimeErrorIncorrectUnpackCount);
      }
    }

    private void Assign(Expression target, Value value, Range range) {
      switch (target) {
        case IdentifierExpression identifier:
          _env.SetVariable(identifier.Name, value);
          var type = _env.InGlobalScope ? VariableType.Global : VariableType.Local;
          var ae = new AssignmentEvent(identifier.Name, type, new ValueWrapper(value), range);
          _visualizerCenter.AssignmentPublisher.Notify(ae);
          break;
        case SubscriptExpression subscript:
          Visit(subscript.Expr);
          Value list = _expressionResult;
          Visit(subscript.Index);
          list[_expressionResult] = value;
          // TODO: send an assignment event to visualizers.
          break;
      }
    }
  }
}
