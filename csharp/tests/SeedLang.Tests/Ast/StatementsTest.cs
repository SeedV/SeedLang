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
  public class StatementsTests {
    internal class TestData : TheoryData<AstNode, string> {
      private static TextRange _textRange => new TextRange(0, 1, 2, 3);

      private static BlockRange _blockRange => new BlockRange(new BlockPosition("id"));

      public TestData() {
        AddAssignmentStatement();
        AddExpressionStatement();
      }

      private void AddAssignmentStatement() {
        var identifier = Expression.Identifier("id", _textRange);
        var expr = Expression.NumberConstant(1, _textRange);
        var assignment = Statement.Assignment(identifier, expr, _textRange);
        var expectedOutput = $"{_textRange} AssignmentStatement\n" +
                             $"  {_textRange} IdentifierExpression (id)\n" +
                             $"  {_textRange} NumberConstantExpression (1)";
        Add(assignment, expectedOutput);
      }

      private void AddExpressionStatement() {
        var one = Expression.NumberConstant(1, _textRange);
        var two = Expression.NumberConstant(2, _textRange);
        var three = Expression.NumberConstant(3, _textRange);
        var left = Expression.Binary(one, BinaryOperator.Add, two, _textRange);
        var binary = Expression.Binary(left, BinaryOperator.Multiply, three, _textRange);
        var expr = Statement.Expression(binary, _textRange);
        var expectedOutput = $"{_textRange} ExpressionStatement\n" +
                             $"  {_textRange} BinaryExpression (*)\n" +
                             $"    {_textRange} BinaryExpression (+)\n" +
                             $"      {_textRange} NumberConstantExpression (1)\n" +
                             $"      {_textRange} NumberConstantExpression (2)\n" +
                             $"    {_textRange} NumberConstantExpression (3)";
        Add(expr, expectedOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestAstNodes(AstNode node, string expectedOutput) {
      Assert.Equal(expectedOutput, node.ToString());
    }
  }
}
