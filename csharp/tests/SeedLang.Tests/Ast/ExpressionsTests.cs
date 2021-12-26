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

using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExpressionsTests {
    internal class TestData : TheoryData<Expression, string> {
      private static TextRange _textRange => new TextRange(0, 1, 2, 3);

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
        var left = Expression.NumberConstant(1, _textRange);
        var right = Expression.NumberConstant(2, _textRange);
        var binary = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
        var expectedOutput = $"{_textRange} BinaryExpression (+)\n" +
                             $"  {_textRange} NumberConstantExpression (1)\n" +
                             $"  {_textRange} NumberConstantExpression (2)";
        Add(binary, expectedOutput);
      }

      private void AddBooleanExpression() {
        var boolean = Expression.Boolean(BooleanOperator.And, new Expression[] {
          Expression.BooleanConstant(false, _textRange),
          Expression.BooleanConstant(true, _textRange),
        }, _textRange);
        var expectedOutput = $"{_textRange} BooleanExpression (And)\n" +
                             $"  {_textRange} BooleanConstantExpression (False)\n" +
                             $"  {_textRange} BooleanConstantExpression (True)";
        Add(boolean, expectedOutput);
      }

      private void AddComplexBooleanExpression() {
        var left = Expression.Boolean(BooleanOperator.And, new Expression[] {
          Expression.BooleanConstant(false, _textRange),
          Expression.BooleanConstant(true, _textRange),
        }, _textRange);
        var right = Expression.BooleanConstant(false, _textRange);
        var boolean = Expression.Boolean(BooleanOperator.Or, new Expression[] { left, right },
                                         _textRange);
        var expectedOutput = $"{_textRange} BooleanExpression (Or)\n" +
                             $"  {_textRange} BooleanExpression (And)\n" +
                             $"    {_textRange} BooleanConstantExpression (False)\n" +
                             $"    {_textRange} BooleanConstantExpression (True)\n" +
                             $"  {_textRange} BooleanConstantExpression (False)";
        Add(boolean, expectedOutput);
      }

      private void AddComparisonExpression() {
        var first = Expression.NumberConstant(1, _textRange);
        var second = Expression.NumberConstant(2, _textRange);
        var third = Expression.NumberConstant(3, _textRange);
        var ops = new ComparisonOperator[] { ComparisonOperator.Less, ComparisonOperator.Greater };
        var exprs = new Expression[] { second, third };
        var comparison = Expression.Comparison(first, ops, exprs, _textRange);
        var expectedOutput = $"{_textRange} ComparisonExpression\n" +
                             $"  {_textRange} NumberConstantExpression (1) (<)\n" +
                             $"  {_textRange} NumberConstantExpression (2) (>)\n" +
                             $"  {_textRange} NumberConstantExpression (3)";
        Add(comparison, expectedOutput);
      }

      private void AddUnaryExpression() {
        var numberConstant = Expression.NumberConstant(1, _textRange);
        var unary = Expression.Unary(UnaryOperator.Negative, numberConstant, _textRange);
        var expectedOutput = $"{_textRange} UnaryExpression (-)\n" +
                             $"  {_textRange} NumberConstantExpression (1)";
        Add(unary, expectedOutput);
      }

      private void AddIdentifierExpression() {
        var name = "test name";
        var identifier = Expression.Identifier(name, _textRange);
        var expectedOutput = $"{_textRange} IdentifierExpression ({name})";
        Add(identifier, expectedOutput);
      }

      private void AddBooleanConstantExpression() {
        var falseConstant = Expression.BooleanConstant(false, _textRange);
        var expectedFalseOutput = $"{_textRange} BooleanConstantExpression (False)";
        Add(falseConstant, expectedFalseOutput);
        var trueConstant = Expression.BooleanConstant(true, _textRange);
        var expectedTrueOutput = $"{_textRange} BooleanConstantExpression (True)";
        Add(trueConstant, expectedTrueOutput);
      }

      private void AddNoneConstantExpression() {
        var noneConstant = Expression.NoneConstant(_textRange);
        var expectedTrueOutput = $"{_textRange} NoneConstantExpression";
        Add(noneConstant, expectedTrueOutput);
      }

      private void AddNumberConstantExpression() {
        double value = 1.5;
        var numberConstant = Expression.NumberConstant(value, _textRange);
        var expectedOutput = $"{_textRange} NumberConstantExpression ({value})";
        Add(numberConstant, expectedOutput);
      }

      private void AddStringConstantExpression() {
        string strValue = "test string";
        var str = Expression.StringConstant(strValue, _textRange);
        var expectedOutput = $"{_textRange} StringConstantExpression ({strValue})";
        Add(str, expectedOutput);
      }

      private void AddListExpression() {
        var one = Expression.NumberConstant(1, _textRange);
        var two = Expression.NumberConstant(2, _textRange);
        var three = Expression.NumberConstant(3, _textRange);
        var list = Expression.List(new Expression[] { one, two, three }, _textRange);
        var expectedOutput = $"{_textRange} ListExpression\n" +
                             $"  {_textRange} NumberConstantExpression (1)\n" +
                             $"  {_textRange} NumberConstantExpression (2)\n" +
                             $"  {_textRange} NumberConstantExpression (3)";
        Add(list, expectedOutput);
      }

      private void AddSubscriptExpression() {
        var one = Expression.NumberConstant(1, _textRange);
        var two = Expression.NumberConstant(2, _textRange);
        var list = Expression.List(new Expression[] { one, two }, _textRange);
        var subscript = Expression.Subscript(list, one, _textRange);
        var expectedOutput = $"{_textRange} SubscriptExpression\n" +
                             $"  {_textRange} ListExpression\n" +
                             $"    {_textRange} NumberConstantExpression (1)\n" +
                             $"    {_textRange} NumberConstantExpression (2)\n" +
                             $"  {_textRange} NumberConstantExpression (1)";
        Add(subscript, expectedOutput);
      }

      private void AddCallExpression() {
        var one = Expression.NumberConstant(1, _textRange);
        var two = Expression.NumberConstant(2, _textRange);
        var name = Expression.Identifier("func", _textRange);
        var call = Expression.Call(name, new Expression[] { one, two }, _textRange);
        var expectedOutput = $"{_textRange} CallExpression\n" +
                             $"  {_textRange} IdentifierExpression (func)\n" +
                             $"  {_textRange} NumberConstantExpression (1)\n" +
                             $"  {_textRange} NumberConstantExpression (2)";
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
