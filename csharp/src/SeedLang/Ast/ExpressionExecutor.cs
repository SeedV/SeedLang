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
using System.Collections.Immutable;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;

namespace SeedLang.Ast {
  // The class to execute expression AST nodes.
  internal class ExpressionExecutor : ExpressionWalker<ExpressionExecutor.Result> {
    internal class Result {
      public VMValue Value;
    }

    private readonly IEnvironment _env;

    internal ExpressionExecutor(IEnvironment env) {
      _env = env;
    }

    protected override void VisitBinary(BinaryExpression binary, Result result) {
      var leftResult = new Result();
      Visit(binary.Left, leftResult);
      var rightResult = new Result();
      Visit(binary.Right, rightResult);
      result.Value = binary.Op switch {
        BinaryOperator.Add => ValueHelper.Add(leftResult.Value, rightResult.Value),
        BinaryOperator.Subtract => ValueHelper.Subtract(leftResult.Value, rightResult.Value),
        BinaryOperator.Multiply => ValueHelper.Multiply(leftResult.Value, rightResult.Value),
        BinaryOperator.Divide => ValueHelper.Divide(leftResult.Value, rightResult.Value),
        BinaryOperator.FloorDivide => ValueHelper.FloorDivide(leftResult.Value, rightResult.Value),
        BinaryOperator.Power => ValueHelper.Power(leftResult.Value, rightResult.Value),
        BinaryOperator.Modulo => ValueHelper.Modulo(leftResult.Value, rightResult.Value),
        _ => throw new NotImplementedException($"Unsupported binary operator: {binary.Op}."),
      };
    }

    protected override void VisitBoolean(BooleanExpression boolean, Result result) {
      result.Value = new VMValue(boolean.Op == BooleanOperator.And);
      foreach (Expression expr in boolean.Exprs) {
        var exprResult = new Result();
        Visit(expr, exprResult);
        if (exprResult.Value != result.Value) {
          result.Value = exprResult.Value;
          break;
        }
      }
    }

    protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant,
                                                 Result result) {
      result.Value = new VMValue(booleanConstant.Value);
    }

    protected override void VisitCall(CallExpression call, Result result) {
      // Doesn't support function call expressions during AST trees execution.
      throw new DiagnosticException(SystemReporters.SeedAst, Severity.Error, "", null,
                                    Message.UnsupportedEvalSyntax);
    }

    protected override void VisitComparison(ComparisonExpression comparison, Result result) {
      var firstResult = new Result();
      Visit(comparison.First, firstResult);
      for (int i = 0; i < comparison.Exprs.Length; i++) {
        var exprResult = new Result();
        Visit(comparison.Exprs[i], exprResult);
        bool boolResult = comparison.Ops[i] switch {
          ComparisonOperator.Less => ValueHelper.Less(firstResult.Value, exprResult.Value),
          ComparisonOperator.Greater => !ValueHelper.LessEqual(firstResult.Value, exprResult.Value),
          ComparisonOperator.LessEqual =>
              ValueHelper.LessEqual(firstResult.Value, exprResult.Value),
          ComparisonOperator.GreaterEqual => !ValueHelper.Less(firstResult.Value, exprResult.Value),
          ComparisonOperator.EqEqual => firstResult.Value == exprResult.Value,
          ComparisonOperator.NotEqual => firstResult.Value != exprResult.Value,
          ComparisonOperator.In => ValueHelper.Contains(firstResult.Value, exprResult.Value),
          _ => throw new NotImplementedException(
              $"Unsupported comparison operator {comparison.Ops[i]}."),
        };
        if (!boolResult) {
          result.Value = new VMValue(false);
          return;
        }
        firstResult = exprResult;
      }
      result.Value = new VMValue(true);
    }

    protected override void VisitDict(DictExpression dict, Result result) {
      var rawDict = new Dictionary<VMValue, VMValue>(dict.KeyValues.Length);
      foreach (var keyValue in dict.KeyValues) {
        var keyResult = new Result();
        Visit(keyValue.Key, keyResult);
        var valueResult = new Result();
        Visit(keyValue.Value, valueResult);
        rawDict[keyResult.Value] = valueResult.Value;
      }
      result.Value = new VMValue(rawDict);
    }

    protected override void VisitIdentifier(IdentifierExpression identifier, Result result) {
      if (!_env.TryGetValueOfVariable(identifier.Name, out result.Value)) {
        throw new DiagnosticException(SystemReporters.SeedAst, Severity.Error, "", null,
                                      Message.RuntimeErrorVariableNotDefined);
      }
    }

    protected override void VisitList(ListExpression list, Result result) {
      var rawList = new List<VMValue>(list.Exprs.Length);
      foreach (Expression expr in list.Exprs) {
        Visit(expr, result);
        rawList.Add(result.Value);
      }
      result.Value = new VMValue(rawList);
    }

    protected override void VisitNilConstant(NilConstantExpression nilConstant, Result result) {
      result.Value = new VMValue();
    }

    protected override void VisitNumberConstant(NumberConstantExpression numberConstant,
                                                Result result) {
      result.Value = new VMValue(numberConstant.Value);
    }

    protected override void VisitSlice(SliceExpression slice, Result result) {
      var start = new Result();
      Visit(slice.Start, start);
      var stop = new Result();
      Visit(slice.Stop, stop);
      var step = new Result();
      Visit(slice.Step, step);
      result.Value = new VMValue(new Slice(start.Value, stop.Value, step.Value));
    }

    protected override void VisitStringConstant(StringConstantExpression stringConstant,
                                                Result result) {
      result.Value = new VMValue(stringConstant.Value);
    }

    protected override void VisitSubscript(SubscriptExpression subscript, Result result) {
      var container = new Result();
      Visit(subscript.Container, container);
      var key = new Result();
      Visit(subscript.Key, key);
      result.Value = container.Value[key.Value];
    }

    protected override void VisitTuple(TupleExpression tuple, Result result) {
      var builder = ImmutableArray.CreateBuilder<VMValue>(tuple.Exprs.Length);
      foreach (Expression expr in tuple.Exprs) {
        Visit(expr, result);
        builder.Add(result.Value);
      }
      result.Value = new VMValue(builder.MoveToImmutable());
    }

    protected override void VisitUnary(UnaryExpression unary, Result result) {
      Visit(unary.Expr, result);
      result.Value = unary.Op switch {
        UnaryOperator.Negative => new VMValue(-result.Value.AsNumber()),
        UnaryOperator.Not => new VMValue(!result.Value.AsBoolean()),
        UnaryOperator.Positive => result.Value,
        _ => throw new NotImplementedException($"Unsupported unary operator: {unary.Op}."),
      };
    }
  }
}
