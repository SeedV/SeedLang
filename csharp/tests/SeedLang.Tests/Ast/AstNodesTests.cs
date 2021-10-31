// Copyright 2021 The Aha001 Team.
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
  public class AstNodesTests {
    internal class TestData : TheoryData<AstNode, string> {
      public TestData() {
        AddBinaryExpression();
        AddIdentifierExpression();
        AddNumberConstantExpression();
        AddStringConstantExpression();
        AddUnaryExpression();

        AddAssignmentStatement();
        AddExpressionStatement();
      }

      private void AddBinaryExpression() {
        var left = Expression.Number(1, NewTextRange());
        var right = Expression.Number(2, NewTextRange());
        var binary = Expression.Binary(left, BinaryOperator.Add, right, NewTextRange());
        var expectedOutput = $"{NewTextRange()} BinaryExpression (+)\n" +
                             $"  {NewTextRange()} NumberConstantExpression (1)\n" +
                             $"  {NewTextRange()} NumberConstantExpression (2)";
        Add(binary, expectedOutput);
      }

      private void AddIdentifierExpression() {
        var name = "test name";
        var identifier = Expression.Identifier(name, NewTextRange());
        var expectedOutput = $"{NewTextRange()} IdentifierExpression ({name})";
        Add(identifier, expectedOutput);
      }

      private void AddNumberConstantExpression() {
        double value = 1.5;
        var number = Expression.Number(value, NewBlockRange());
        var expectedOutput = $"{NewBlockRange()} NumberConstantExpression ({value})";
        Add(number, expectedOutput);
      }

      private void AddStringConstantExpression() {
        string strValue = "test string";
        var str = Expression.String(strValue, NewTextRange());
        var expectedOutput = $"{NewTextRange()} StringConstantExpression ({strValue})";
        Add(str, expectedOutput);
      }

      private void AddUnaryExpression() {
        var number = Expression.Number(1, NewTextRange());
        var unary = Expression.Unary(UnaryOperator.Negative, number, NewTextRange());
        var expectedOutput = $"{NewTextRange()} UnaryExpression (-)\n" +
                             $"  {NewTextRange()} NumberConstantExpression (1)";
        Add(unary, expectedOutput);
      }

      private void AddAssignmentStatement() {
        var identifier = Expression.Identifier("id", NewTextRange());
        var expr = Expression.Number(1, NewTextRange());
        var assignment = Statement.Assignment(identifier, expr, NewTextRange());
        var expectedOutput = $"{NewTextRange()} AssignmentStatement\n" +
                             $"  {NewTextRange()} IdentifierExpression (id)\n" +
                             $"  {NewTextRange()} NumberConstantExpression (1)";
        Add(assignment, expectedOutput);
      }

      private void AddExpressionStatement() {
        var one = Expression.Number(1, NewTextRange());
        var two = Expression.Number(2, NewTextRange());
        var three = Expression.Number(3, NewTextRange());
        var left = Expression.Binary(one, BinaryOperator.Add, two, NewTextRange());
        var binary = Expression.Binary(left, BinaryOperator.Multiply, three, NewTextRange());
        var eval = Statement.Expression(binary, NewTextRange());
        var expectedOutput = $"{NewTextRange()} ExpressionStatement\n" +
                             $"  {NewTextRange()} BinaryExpression (*)\n" +
                             $"    {NewTextRange()} BinaryExpression (+)\n" +
                             $"      {NewTextRange()} NumberConstantExpression (1)\n" +
                             $"      {NewTextRange()} NumberConstantExpression (2)\n" +
                             $"    {NewTextRange()} NumberConstantExpression (3)";
        Add(eval, expectedOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestAstNodes(AstNode node, string expectedOutput) {
      Assert.Equal(expectedOutput, node.ToString());
    }

    private static TextRange NewTextRange() {
      return new TextRange(0, 1, 2, 3);
    }

    private static BlockRange NewBlockRange() {
      return new BlockRange(new BlockPosition("id"));
    }
  }
}
