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
    [InlineData("1 + 2 * 3 - 4\n", true)]
    [InlineData("1 +", false)]
    public void TestValidateExpressionStatement(string input, bool result) {
      Assert.Equal(result, _parser.Validate(input, "", _collection));
    }

    [Theory]
    [InlineData("True\n",

                "[Ln 1, Col 0 - Ln 1, Col 3] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 3] BooleanConstantExpression (True)",

                "Boolean [Ln 1, Col 0 - Ln 1, Col 3]")]

    [InlineData("False\n",

                "[Ln 1, Col 0 - Ln 1, Col 4] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 4] BooleanConstantExpression (False)",

                "Boolean [Ln 1, Col 0 - Ln 1, Col 4]")]

    [InlineData("id\n",

                "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] IdentifierExpression (id)",

                "Variable [Ln 1, Col 0 - Ln 1, Col 1]")]

    [InlineData("id = 1\n",

                "[Ln 1, Col 0 - Ln 1, Col 5] AssignmentStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] IdentifierExpression (id)\n" +
                "  [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (1)",

                "Variable [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("1 + 2\n",

                "[Ln 1, Col 0 - Ln 1, Col 4] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 4] BinaryExpression (+)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("1 + 2 * 3 - 40\n",

                "[Ln 1, Col 0 - Ln 1, Col 13] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 13] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (+)\n" +
                "      [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 4 - Ln 1, Col 8] BinaryExpression (*)\n" +
                "        [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                "        [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (3)\n" +
                "    [Ln 1, Col 12 - Ln 1, Col 13] NumberConstantExpression (40)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Number [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Operator [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Number [Ln 1, Col 12 - Ln 1, Col 13]")]

    [InlineData("1 + 2 // 3 - 40 % 5 ** 2 \n",

                "[Ln 1, Col 0 - Ln 1, Col 23] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 23] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 9] BinaryExpression (+)\n" +
                "      [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 4 - Ln 1, Col 9] BinaryExpression (//)\n" +
                "        [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                "        [Ln 1, Col 9 - Ln 1, Col 9] NumberConstantExpression (3)\n" +
                "    [Ln 1, Col 13 - Ln 1, Col 23] BinaryExpression (%)\n" +
                "      [Ln 1, Col 13 - Ln 1, Col 14] NumberConstantExpression (40)\n" +
                "      [Ln 1, Col 18 - Ln 1, Col 23] BinaryExpression (**)\n" +
                "        [Ln 1, Col 18 - Ln 1, Col 18] NumberConstantExpression (5)\n" +
                "        [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (2)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 7]," +
                "Number [Ln 1, Col 9 - Ln 1, Col 9]," +
                "Operator [Ln 1, Col 11 - Ln 1, Col 11]," +
                "Number [Ln 1, Col 13 - Ln 1, Col 14]," +
                "Operator [Ln 1, Col 16 - Ln 1, Col 16]," +
                "Number [Ln 1, Col 18 - Ln 1, Col 18]," +
                "Operator [Ln 1, Col 20 - Ln 1, Col 21]," +
                "Number [Ln 1, Col 23 - Ln 1, Col 23]")]

    [InlineData("(1 + (2)) - (x) - -3\n",

                "[Ln 1, Col 0 - Ln 1, Col 19] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 19] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 14] BinaryExpression (-)\n" +
                "      [Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (+)\n" +
                "        [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                "        [Ln 1, Col 5 - Ln 1, Col 7] NumberConstantExpression (2)\n" +
                "      [Ln 1, Col 12 - Ln 1, Col 14] IdentifierExpression (x)\n" +
                "    [Ln 1, Col 18 - Ln 1, Col 19] UnaryExpression (-)\n" +
                "      [Ln 1, Col 19 - Ln 1, Col 19] NumberConstantExpression (3)",

                "Parenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Parenthesis [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Parenthesis [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Parenthesis [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Operator [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Parenthesis [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Variable [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Parenthesis [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Operator [Ln 1, Col 16 - Ln 1, Col 16]," +
                "Operator [Ln 1, Col 18 - Ln 1, Col 18]," +
                "Number [Ln 1, Col 19 - Ln 1, Col 19]")]

    [InlineData("(1 + 2) * (( 3 - -4 ))\n",

                "[Ln 1, Col 0 - Ln 1, Col 21] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 21] BinaryExpression (*)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 6] BinaryExpression (+)\n" +
                "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (2)\n" +
                "    [Ln 1, Col 10 - Ln 1, Col 21] BinaryExpression (-)\n" +
                "      [Ln 1, Col 13 - Ln 1, Col 13] NumberConstantExpression (3)\n" +
                "      [Ln 1, Col 17 - Ln 1, Col 18] UnaryExpression (-)\n" +
                "        [Ln 1, Col 18 - Ln 1, Col 18] NumberConstantExpression (4)",

                "Parenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Parenthesis [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Parenthesis [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Parenthesis [Ln 1, Col 11 - Ln 1, Col 11]," +
                "Number [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Operator [Ln 1, Col 15 - Ln 1, Col 15]," +
                "Operator [Ln 1, Col 17 - Ln 1, Col 17]," +
                "Number [Ln 1, Col 18 - Ln 1, Col 18]," +
                "Parenthesis [Ln 1, Col 20 - Ln 1, Col 20]," +
                "Parenthesis [Ln 1, Col 21 - Ln 1, Col 21]")]

    [InlineData("1 < 2 > 3 <= 4\n",

                "[Ln 1, Col 0 - Ln 1, Col 13] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 13] ComparisonExpression\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1) (<)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2) (>)\n" +
                "    [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (3) (<=)\n" +
                "    [Ln 1, Col 13 - Ln 1, Col 13] NumberConstantExpression (4)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Number [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Operator [Ln 1, Col 10 - Ln 1, Col 11]," +
                "Number [Ln 1, Col 13 - Ln 1, Col 13]")]

    [InlineData("while True: x = 1\n",

                "[Ln 1, Col 0 - Ln 1, Col 16] WhileStatement\n" +
                "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)\n" +
                "  [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement\n" +
                "    [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x)\n" +
                "    [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 4]," +
                "Boolean [Ln 1, Col 6 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Variable [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Operator [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Number [Ln 1, Col 16 - Ln 1, Col 16]")]
    public void TestPythonParser(string input, string expectedAst, string expectedTokens) {
      Assert.True(_parser.Parse(input, "", _collection, out AstNode node,
                                out IReadOnlyList<SyntaxToken> tokens));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expectedAst, node.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens.Select(token => token.ToString())));
    }
  }
}
