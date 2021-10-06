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

using System.Collections.Generic;
using System.Linq;
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
    [InlineData("id = 1",

                "[Ln 1, Col 0 - Ln 1, Col 5] AssignmentStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] IdentifierExpression (id)\n" +
                "  [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (1)",

                "Variable [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("eval 1 + 2 * 3 - 40",

                "[Ln 1, Col 0 - Ln 1, Col 18] EvalStatement\n" +
                "  [Ln 1, Col 5 - Ln 1, Col 18] BinaryExpression (-)\n" +
                "    [Ln 1, Col 5 - Ln 1, Col 13] BinaryExpression (+)\n" +
                "      [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 9 - Ln 1, Col 13] BinaryExpression (*)\n" +
                "        [Ln 1, Col 9 - Ln 1, Col 9] NumberConstantExpression (2)\n" +
                "        [Ln 1, Col 13 - Ln 1, Col 13] NumberConstantExpression (3)\n" +
                "    [Ln 1, Col 17 - Ln 1, Col 18] NumberConstantExpression (40)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Number [Ln 1, Col 9 - Ln 1, Col 9]," +
                "Operator [Ln 1, Col 11 - Ln 1, Col 11]," +
                "Number [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Operator [Ln 1, Col 15 - Ln 1, Col 15]," +
                "Number [Ln 1, Col 17 - Ln 1, Col 18]")]

    [InlineData("eval (1 + (2)) - (x) - -3",

                "[Ln 1, Col 0 - Ln 1, Col 24] EvalStatement\n" +
                "  [Ln 1, Col 5 - Ln 1, Col 24] BinaryExpression (-)\n" +
                "    [Ln 1, Col 5 - Ln 1, Col 19] BinaryExpression (-)\n" +
                "      [Ln 1, Col 5 - Ln 1, Col 13] BinaryExpression (+)\n" +
                "        [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (1)\n" +
                "        [Ln 1, Col 10 - Ln 1, Col 12] NumberConstantExpression (2)\n" +
                "      [Ln 1, Col 17 - Ln 1, Col 19] IdentifierExpression (x)\n" +
                "    [Ln 1, Col 23 - Ln 1, Col 24] UnaryExpression (-)\n" +
                "      [Ln 1, Col 24 - Ln 1, Col 24] NumberConstantExpression (3)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 3]," +
                "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Number [Ln 1, Col 11 - Ln 1, Col 11]," +
                "Symbol [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Symbol [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Operator [Ln 1, Col 15 - Ln 1, Col 15]," +
                "Symbol [Ln 1, Col 17 - Ln 1, Col 17]," +
                "Variable [Ln 1, Col 18 - Ln 1, Col 18]," +
                "Symbol [Ln 1, Col 19 - Ln 1, Col 19]," +
                "Operator [Ln 1, Col 21 - Ln 1, Col 21]," +
                "Operator [Ln 1, Col 23 - Ln 1, Col 23]," +
                "Number [Ln 1, Col 24 - Ln 1, Col 24]")]

    [InlineData("eval (1 + 2) * (( 3 - -4 ))",

                "[Ln 1, Col 0 - Ln 1, Col 26] EvalStatement\n" +
                "  [Ln 1, Col 5 - Ln 1, Col 26] BinaryExpression (*)\n" +
                "    [Ln 1, Col 5 - Ln 1, Col 11] BinaryExpression (+)\n" +
                "      [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (2)\n" +
                "    [Ln 1, Col 15 - Ln 1, Col 26] BinaryExpression (-)\n" +
                "      [Ln 1, Col 18 - Ln 1, Col 18] NumberConstantExpression (3)\n" +
                "      [Ln 1, Col 22 - Ln 1, Col 23] UnaryExpression (-)\n" +
                "        [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (4)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 3]," +
                "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Number [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Symbol [Ln 1, Col 11 - Ln 1, Col 11]," +
                "Operator [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Symbol [Ln 1, Col 15 - Ln 1, Col 15]," +
                "Symbol [Ln 1, Col 16 - Ln 1, Col 16]," +
                "Number [Ln 1, Col 18 - Ln 1, Col 18]," +
                "Operator [Ln 1, Col 20 - Ln 1, Col 20]," +
                "Operator [Ln 1, Col 22 - Ln 1, Col 22]," +
                "Number [Ln 1, Col 23 - Ln 1, Col 23]," +
                "Symbol [Ln 1, Col 25 - Ln 1, Col 25]," +
                "Symbol [Ln 1, Col 26 - Ln 1, Col 26]")]
    public void TestPythonParser(string input, string expectedAst, string expectedTokens) {
      Assert.True(_parser.Parse(input, "", ParseRule.Statement, _collection, out AstNode node,
                                out IReadOnlyList<SyntaxToken> tokens));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expectedAst, node.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens.Select(token => token.ToString())));
    }

    // TODO: add test cases for other syntax errors after grammar is more complex.
    [Theory]
    [InlineData("1",
                "SyntaxErrorInputMismatch '1' {'break', 'continue', 'eval', IDENTIFIER}",
                "Number [Ln 1, Col 0 - Ln 1, Col 0]")]

    [InlineData("eval1",
                "SyntaxErrorInputMismatch '<EOF>' '='",
                "Variable [Ln 1, Col 0 - Ln 1, Col 4]")]

    [InlineData("eval 1.2 =",
                "SyntaxErrorUnwantedToken '=' <EOF>",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 7]," +
                "Operator [Ln 1, Col 9 - Ln 1, Col 9]")]

    [InlineData("eval 1 +",
                "SyntaxErrorInputMismatch '<EOF>' {'-', IDENTIFIER, NUMBER, '('}",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 7 - Ln 1, Col 7]")]
    public void TestParseSingleSyntaxError(string input, string errorMessage,
                                           string expectedTokens) {
      Assert.False(_parser.Parse(input, "", ParseRule.Statement, _collection, out AstNode node,
                                out IReadOnlyList<SyntaxToken> tokens));
      Assert.Null(node);
      Assert.Single(_collection.Diagnostics);
      Assert.Equal(SystemReporters.SeedX, _collection.Diagnostics[0].Reporter);
      Assert.Equal(Severity.Fatal, _collection.Diagnostics[0].Severity);
      Assert.Equal(errorMessage, _collection.Diagnostics[0].LocalizedMessage);
      Assert.Equal(expectedTokens, string.Join(",", tokens.Select(token => token.ToString())));
    }
  }
}
