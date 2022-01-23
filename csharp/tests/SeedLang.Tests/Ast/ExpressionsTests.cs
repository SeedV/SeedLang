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
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExpressionsTests {
    internal class TestData : TheoryData<Expression, string> {
      public TestData() {
        AddBinaryExpression();
        AddBooleanExpression();
        AddComplexBooleanExpression();
        AddComparisonExpression();
        AddUnaryExpression();
        AddIdentifierExpression();
        AddBooleanConstantExpression();
        AddNoneConstantExpression();
        AddNumberConstantExpression();
        AddStringConstantExpression();
        AddListExpression();
        AddSubscriptExpression();
        AddCallExpression();
      }

      private void AddBinaryExpression() {
        var binary = AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                                      AstHelper.NumberConstant(2));
        var expectedOutput = $"{AstHelper.TextRange} BinaryExpression (+)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (1)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (2)";
        Add(binary, expectedOutput);
      }

      private void AddBooleanExpression() {
        var boolean = AstHelper.Boolean(BooleanOperator.And, AstHelper.BooleanConstant(false),
                                        AstHelper.BooleanConstant(true));
        var expectedOutput = $"{AstHelper.TextRange} BooleanExpression (And)\n" +
                             $"  {AstHelper.TextRange} BooleanConstantExpression (False)\n" +
                             $"  {AstHelper.TextRange} BooleanConstantExpression (True)";
        Add(boolean, expectedOutput);
      }

      private void AddComplexBooleanExpression() {
        var boolean = AstHelper.Boolean(
          BooleanOperator.Or,
          AstHelper.Boolean(BooleanOperator.And, AstHelper.BooleanConstant(false),
                            AstHelper.BooleanConstant(true)),
          AstHelper.BooleanConstant(false)
        );
        var expectedOutput = $"{AstHelper.TextRange} BooleanExpression (Or)\n" +
                             $"  {AstHelper.TextRange} BooleanExpression (And)\n" +
                             $"    {AstHelper.TextRange} BooleanConstantExpression (False)\n" +
                             $"    {AstHelper.TextRange} BooleanConstantExpression (True)\n" +
                             $"  {AstHelper.TextRange} BooleanConstantExpression (False)";
        Add(boolean, expectedOutput);
      }

      private void AddComparisonExpression() {
        var comparison = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                              AstHelper.CompOps(ComparisonOperator.Less,
                                                                ComparisonOperator.Greater),
                                              AstHelper.NumberConstant(2),
                                              AstHelper.NumberConstant(3));
        var expectedOutput = $"{AstHelper.TextRange} ComparisonExpression\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (1) (<)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (2) (>)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (3)";
        Add(comparison, expectedOutput);
      }

      private void AddUnaryExpression() {
        var unary = AstHelper.Unary(UnaryOperator.Negative, AstHelper.NumberConstant(1));
        var expectedOutput = $"{AstHelper.TextRange} UnaryExpression (-)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (1)";
        Add(unary, expectedOutput);
      }

      private void AddIdentifierExpression() {
        var name = "test name";
        var identifier = AstHelper.Id(name);
        var expectedOutput = $"{AstHelper.TextRange} IdentifierExpression ({name})";
        Add(identifier, expectedOutput);
      }

      private void AddBooleanConstantExpression() {
        var falseConstant = AstHelper.BooleanConstant(false);
        var expectedFalseOutput = $"{AstHelper.TextRange} BooleanConstantExpression (False)";
        Add(falseConstant, expectedFalseOutput);
        var trueConstant = AstHelper.BooleanConstant(true);
        var expectedTrueOutput = $"{AstHelper.TextRange} BooleanConstantExpression (True)";
        Add(trueConstant, expectedTrueOutput);
      }

      private void AddNoneConstantExpression() {
        var noneConstant = AstHelper.NoneConstant();
        var expectedTrueOutput = $"{AstHelper.TextRange} NoneConstantExpression";
        Add(noneConstant, expectedTrueOutput);
      }

      private void AddNumberConstantExpression() {
        double value = 1.5;
        var numberConstant = AstHelper.NumberConstant(value);
        var expectedOutput = $"{AstHelper.TextRange} NumberConstantExpression ({value})";
        Add(numberConstant, expectedOutput);
      }

      private void AddStringConstantExpression() {
        string strValue = "test string";
        var str = AstHelper.StringConstant(strValue);
        var expectedOutput = $"{AstHelper.TextRange} StringConstantExpression ({strValue})";
        Add(str, expectedOutput);
      }

      private void AddListExpression() {
        var list = AstHelper.List(AstHelper.NumberConstant(1),
                                  AstHelper.NumberConstant(2),
                                  AstHelper.NumberConstant(3));
        var expectedOutput = $"{AstHelper.TextRange} ListExpression\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (1)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (2)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (3)";
        Add(list, expectedOutput);
      }

      private void AddSubscriptExpression() {
        var subscript = AstHelper.Subscript(AstHelper.List(AstHelper.NumberConstant(1),
                                                           AstHelper.NumberConstant(2)),
                                            AstHelper.NumberConstant(1));
        var expectedOutput = $"{AstHelper.TextRange} SubscriptExpression\n" +
                             $"  {AstHelper.TextRange} ListExpression\n" +
                             $"    {AstHelper.TextRange} NumberConstantExpression (1)\n" +
                             $"    {AstHelper.TextRange} NumberConstantExpression (2)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (1)";
        Add(subscript, expectedOutput);
      }

      private void AddCallExpression() {
        string name = "func";
        var call = AstHelper.Call(AstHelper.Id(name), AstHelper.NumberConstant(1), AstHelper.NumberConstant(2));
        var expectedOutput = $"{AstHelper.TextRange} CallExpression\n" +
                             $"  {AstHelper.TextRange} IdentifierExpression ({name})\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (1)\n" +
                             $"  {AstHelper.TextRange} NumberConstantExpression (2)";
        Add(call, expectedOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestExpression(Expression expression, string expectedOutput) {
      Assert.Equal(expectedOutput, expression.ToString());
    }
  }
}
