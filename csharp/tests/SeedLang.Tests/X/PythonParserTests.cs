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

using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.X.Tests {
  public class PythonParserTests {
    private readonly DiagnosticCollection _collection = new DiagnosticCollection();
    private readonly PythonParser _parser = new PythonParser();

    [Theory]
    [InlineData("eval 1 + 2 * 3 - 4", true)]
    [InlineData("eval 1 +", false)]
    public void TestValidateEvalStatement(string input, bool result) {
      Assert.Equal(result, _parser.Validate(input, "", ParseRule.Statement, _collection));
    }

    [Theory]
    [InlineData("id = 1", "id = 1\n")]
    public void TestParseAssignmentStatement(string input, string expected) {
      Assert.True(_parser.TryParse(input, "", ParseRule.Statement, _collection, out AstNode node));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
    }

    [Fact]
    public void TestParseEvalStatementWithRange() {
      Assert.True(_parser.TryParse("eval (1 + 2) * (( 3 - -4 ))", "", ParseRule.Statement,
                  _collection, out AstNode node));
      Assert.Empty(_collection.Diagnostics);
      Assert.NotNull(node);
      Assert.Equal(new TextRange(1, 0, 1, 26), node.Range);
      var expr = (node as EvalStatement).Expr as BinaryExpression;
      Assert.Equal(new TextRange(1, 5, 1, 26), expr.Range);
      var left = expr.Left as BinaryExpression;
      Assert.Equal(new TextRange(1, 5, 1, 11), left.Range);
      var right = expr.Right as BinaryExpression;
      Assert.Equal(new TextRange(1, 15, 1, 26), right.Range);
      var leftNumber = right.Left as NumberConstantExpression;
      Assert.Equal(new TextRange(1, 18, 1, 18), leftNumber.Range);
      var unary = right.Right as UnaryExpression;
      Assert.Equal(new TextRange(1, 22, 1, 23), unary.Range);
      var rightNumber = unary.Expr as NumberConstantExpression;
      Assert.Equal(new TextRange(1, 23, 1, 23), rightNumber.Range);
      Assert.Equal("eval ((1 + 2) * (3 - (- 4)))\n", node.ToString());
    }

    [Theory]
    [InlineData("eval 1 + 2 * 3 - 4", "eval ((1 + (2 * 3)) - 4)\n")]
    public void TestParseEvalStatement(string input, string expected) {
      Assert.True(_parser.TryParse(input, "", ParseRule.Statement, _collection, out AstNode node));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
    }

    // TODO: add test cases for other syntax errors after grammar is more complex.
    [Theory]
    [InlineData("1", "SyntaxErrorInputMismatch '1' {'break', 'continue', 'eval', IDENTIFIER}")]
    [InlineData("eval1", @"SyntaxErrorInputMismatch '<EOF>' '='")]
    [InlineData("eval 1.2 =", @"SyntaxErrorUnwantedToken '=' <EOF>")]
    public void TestParseSingleSyntaxError(string input, string localizedMessage) {
      Assert.False(_parser.TryParse(input, "", ParseRule.Statement, _collection, out AstNode node));
      Assert.Null(node);
      Assert.Single(_collection.Diagnostics);
      Assert.Equal(SystemReporters.SeedX, _collection.Diagnostics[0].Reporter);
      Assert.Equal(Severity.Fatal, _collection.Diagnostics[0].Severity);
      Assert.Equal(localizedMessage, _collection.Diagnostics[0].LocalizedMessage);
    }
  }
}
