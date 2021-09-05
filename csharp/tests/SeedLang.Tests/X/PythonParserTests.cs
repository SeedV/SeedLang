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
    [InlineData("0", true)]
    [InlineData("0.", true)]
    [InlineData(".0", true)]
    [InlineData(".5", true)]
    [InlineData("1.5", true)]
    [InlineData("1e3", true)]
    [InlineData("1e+20", true)]
    [InlineData("1e-5", true)]
    [InlineData("..1", false)]
    [InlineData("1.2.3", false)]
    [InlineData("1a", false)]
    public void TestValidateNumber(string input, bool result) {
      Assert.Equal(result, _parser.Validate(input, "", ParseRule.Number, _collection));
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("0.", "0")]
    [InlineData(".0", "0")]
    [InlineData(".5", "0.5")]
    [InlineData("1.5", "1.5")]
    [InlineData("1e3", "1000")]
    [InlineData("1e+20", "1E+20")]
    [InlineData("1e-5", "1E-05")]
    public void TestParseNumber(string input, string expected) {
      Assert.True(_parser.TryParse(input, "", ParseRule.Number, _collection, out AstNode node));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
    }

    [Theory]
    [InlineData("1 + 2", "(1 + 2)")]
    [InlineData("1 - 2 * 3", "(1 - (2 * 3))")]
    [InlineData("(1 + 2) / 3", "((1 + 2) / 3)")]
    public void TestParseBinaryExpression(string input, string expected) {
      Assert.True(_parser.TryParse(input, "", ParseRule.Expression, _collection, out AstNode node));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
    }

    [Theory]
    [InlineData("-1 + 2", "((- 1) + 2)")]
    [InlineData("-(1 + 2)", "(- (1 + 2))")]
    [InlineData("2 - - 1", "(2 - (- 1))")]
    public void TestParseUnaryExpression(string input, string expected) {
      Assert.True(_parser.TryParse(input, "", ParseRule.Expression, _collection, out AstNode node));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
    }

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
    [InlineData("1", "SyntaxErrorInputMismatch '1' {'eval', 'break', 'continue', IDENTIFIER}")]
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