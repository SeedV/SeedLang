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
using System.Collections.Immutable;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  internal interface IEnvironment {
    bool GetValueOfVariable(string name, out VMValue value);
  }

  internal class ExpressionExecutor : ExpressionWalker<ExpressionExecutor.Result> {
    internal class Result {
      public VMValue Value;
    }

    private readonly IEnvironment _env;

    internal ExpressionExecutor(IEnvironment env) {
      _env = env;
    }

    protected override void VisitBinary(BinaryExpression binary, Result result) {
      var left = new Result();
      Visit(binary.Left, left);
      var right = new Result();
      Visit(binary.Right, right);
      result.Value = binary.Op switch {
        BinaryOperator.Add => ValueHelper.Add(left.Value, right.Value),
        BinaryOperator.Subtract => ValueHelper.Subtract(left.Value, right.Value),
        BinaryOperator.Multiply => ValueHelper.Multiply(left.Value, right.Value),
        BinaryOperator.Divide => ValueHelper.Divide(left.Value, right.Value),
        BinaryOperator.FloorDivide => ValueHelper.FloorDivide(left.Value, right.Value),
        BinaryOperator.Power => ValueHelper.Power(left.Value, right.Value),
        BinaryOperator.Modulo => ValueHelper.Modulo(left.Value, right.Value),
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
      throw new NotImplementedException();
    }

    protected override void VisitComparison(ComparisonExpression comparison, Result result) {
      throw new NotImplementedException();
    }

    protected override void VisitDict(DictExpression dict, Result result) {
      throw new NotImplementedException();
    }

    protected override void VisitIdentifier(IdentifierExpression identifier, Result result) {
      _env.GetValueOfVariable(identifier.Name, out result.Value);
    }

    protected override void VisitList(ListExpression list, Result result) {
      throw new NotImplementedException();
    }

    protected override void VisitNilConstant(NilConstantExpression nilConstant, Result result) {
      throw new NotImplementedException();
    }

    protected override void VisitNumberConstant(NumberConstantExpression numberConstant,
                                                Result result) {
      result.Value = new VMValue(numberConstant.Value);
    }

    protected override void VisitSlice(SliceExpression slice, Result result) {
      throw new NotImplementedException();
    }

    protected override void VisitStringConstant(StringConstantExpression stringConstant,
                                                Result result) {
      throw new NotImplementedException();
    }

    protected override void VisitSubscript(SubscriptExpression subscript, Result result) {
      throw new NotImplementedException();
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
