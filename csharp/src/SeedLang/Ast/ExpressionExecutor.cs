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

using SeedLang.Runtime;

namespace SeedLang.Ast {
  internal interface IVariables {
    bool GetValueOf(string name, out VMValue value);
  }

  internal class ExpressionExecutor : ExpressionWalker<ExpressionExecutor.Result> {
    internal class Result {
      public VMValue Value;
    }

    private readonly IVariables _variables;

    internal ExpressionExecutor(IVariables variables) {
      _variables = variables;
    }

    protected override void VisitBinary(BinaryExpression binary, Result result) {
      var left = new Result();
      Visit(binary.Left, left);
      var right = new Result();
      Visit(binary.Right, right);
      result.Value = ValueHelper.Add(left.Value, right.Value);
    }

    protected override void VisitBoolean(BooleanExpression boolean, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant,
                                                 Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitCall(CallExpression call, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitComparison(ComparisonExpression comparison, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitDict(DictExpression dict, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitIdentifier(IdentifierExpression identifier, Result result) {
      _variables.GetValueOf(identifier.Name, out result.Value);
    }

    protected override void VisitList(ListExpression list, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitNilConstant(NilConstantExpression nilConstant, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitNumberConstant(NumberConstantExpression numberConstant,
                                                Result result) {
      result.Value = new VMValue(numberConstant.Value);
    }

    protected override void VisitSlice(SliceExpression slice, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitStringConstant(StringConstantExpression stringConstant,
                                                Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitSubscript(SubscriptExpression subscript, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitTuple(TupleExpression tuple, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitUnary(UnaryExpression unary, Result result) {
      throw new System.NotImplementedException();
    }
  }
}
