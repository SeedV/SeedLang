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
    private static TextRange _testTextRange => new TextRange(0, 1, 2, 3);
    private static BlockRange _testBlockRange => new BlockRange(new BlockPosition("Test Id"));

    [Fact]
    public void TestIdentifier() {
      string name = "test name";
      var identifier = Expression.Identifier(name, _testTextRange);
      Assert.Equal(name, identifier.Name);
      Assert.Equal(_testTextRange, identifier.Range);
      Assert.Equal(name, identifier.ToString());
    }

    [Fact]
    public void TestNumberConstant() {
      double value = 1.5;
      var number = Expression.Number(value, _testBlockRange);
      Assert.Equal(value, number.Value);
      Assert.Equal(_testBlockRange, number.Range);
      Assert.Equal(value.ToString(), number.ToString());
    }

    [Fact]
    public void TestStringConstant() {
      string strValue = "test string";
      var str = Expression.String(strValue, _testTextRange);
      Assert.Equal(strValue, str.Value);
      Assert.Equal(_testTextRange, str.Range);
      Assert.Equal(strValue, str.ToString());
    }

    [Fact]
    public void TestBinaryExpression() {
      var left = Expression.Number(1, _testTextRange);
      var right = Expression.Number(2, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _testTextRange);
      Assert.Equal("(1 + 2)", binary.ToString());
    }

    [Fact]
    public void TestUnaryExpression() {
      var number = Expression.Number(1, _testTextRange);
      var unary = Expression.Unary(UnaryOperator.Negative, number, _testTextRange);
      Assert.Equal("(- 1)", unary.ToString());
    }

    [Fact]
    public void TestAssignmentStatement() {
      var identifier = Expression.Identifier("id", null);
      var expr = Expression.Number(1, _testTextRange);
      var assignment = Statement.Assignment(identifier, expr, _testTextRange);
      Assert.Equal("id = 1\n", assignment.ToString());
    }

    [Fact]
    public void TestEvalStatement() {
      var one = Expression.Number(1, _testTextRange);
      var two = Expression.Number(2, _testTextRange);
      var three = Expression.Number(3, _testTextRange);
      var left = Expression.Binary(one, BinaryOperator.Add, two, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Multiply, three, _testTextRange);
      var eval = Statement.Eval(binary, _testTextRange);
      Assert.Equal("eval ((1 + 2) * 3)\n", eval.ToString());
    }
  }
}
