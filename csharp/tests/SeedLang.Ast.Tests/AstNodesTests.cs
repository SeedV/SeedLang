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

using Xunit;

namespace SeedLang.Ast.Tests {
  public class AstNodesTests {
    [Fact]
    public void TestNumberConstant() {
      double value = 1.5;
      var number = Expression.Number(value);
      Assert.Equal(value, number.Value);
      Assert.Equal(value.ToString(), number.ToString());
    }

    [Fact]
    public void TestStringConstant() {
      string strValue = "test string";
      var str = Expression.String(strValue);
      Assert.Equal(strValue, str.Value);
      Assert.Equal(strValue, str.ToString());
    }

    [Fact]
    public void TestBinaryExpression() {
      var left = Expression.Number(1);
      var right = Expression.Number(2);
      var binary = Expression.Binary(left, BinaryOperator.Add, right);
      Assert.Equal("(1 + 2)", binary.ToString());
    }

    [Fact]
    public void TestEvalStatement() {
      var one = Expression.Number(1);
      var two = Expression.Number(2);
      var three = Expression.Number(3);
      var left = Expression.Binary(one, BinaryOperator.Add, two);
      var binary = Expression.Binary(left, BinaryOperator.Multiply, three);
      var eval = Statement.Eval(binary);
      Assert.Equal("eval ((1 + 2) * 3)\n", eval.ToString());
    }
  }
}
