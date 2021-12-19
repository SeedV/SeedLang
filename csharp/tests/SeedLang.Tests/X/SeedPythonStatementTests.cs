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
  public class SeedPythonStatementTests {
    private readonly DiagnosticCollection _collection = new DiagnosticCollection();
    private readonly SeedPython _parser = new SeedPython();

    [Theory]
    [InlineData("if 1 < 2: x = 1",

                "[Ln 1, Col 0 - Ln 1, Col 14] IfStatement\n" +
                "  [Ln 1, Col 3 - Ln 1, Col 7] ComparisonExpression\n" +
                "    [Ln 1, Col 3 - Ln 1, Col 3] NumberConstantExpression (1) (<)\n" +
                "    [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (2)\n" +
                "  [Ln 1, Col 10 - Ln 1, Col 14] AssignmentStatement\n" +
                "    [Ln 1, Col 10 - Ln 1, Col 10] IdentifierExpression (x)\n" +
                "    [Ln 1, Col 14 - Ln 1, Col 14] NumberConstantExpression (1)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Number [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Operator [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Symbol [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Variable [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Operator [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Number [Ln 1, Col 14 - Ln 1, Col 14]")]

    [InlineData("if True:\n" +
                "  x = 1\n" +
                "else:\n" +
                "  x = 2",

                "[Ln 1, Col 0 - Ln 4, Col 6] IfStatement\n" +
                "  [Ln 1, Col 3 - Ln 1, Col 6] BooleanConstantExpression (True)\n" +
                "  [Ln 2, Col 2 - Ln 2, Col 6] AssignmentStatement\n" +
                "    [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (x)\n" +
                "    [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)\n" +
                "  [Ln 4, Col 2 - Ln 4, Col 6] AssignmentStatement\n" +
                "    [Ln 4, Col 2 - Ln 4, Col 2] IdentifierExpression (x)\n" +
                "    [Ln 4, Col 6 - Ln 4, Col 6] NumberConstantExpression (2)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Boolean [Ln 1, Col 3 - Ln 1, Col 6]," +
                "Symbol [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Variable [Ln 2, Col 2 - Ln 2, Col 2]," +
                "Operator [Ln 2, Col 4 - Ln 2, Col 4]," +
                "Number [Ln 2, Col 6 - Ln 2, Col 6]," +
                "Keyword [Ln 3, Col 0 - Ln 3, Col 3]," +
                "Symbol [Ln 3, Col 4 - Ln 3, Col 4]," +
                "Variable [Ln 4, Col 2 - Ln 4, Col 2]," +
                "Operator [Ln 4, Col 4 - Ln 4, Col 4]," +
                "Number [Ln 4, Col 6 - Ln 4, Col 6]")]

    [InlineData("if True:\n" +
                "  x = 1\n" +
                "elif False:\n" +
                "  x = 2\n" +
                "elif 1 < 2:\n" +
                "  x = 3\n" +
                "else:\n" +
                "  x = 4",

                "[Ln 1, Col 0 - Ln 8, Col 6] IfStatement\n" +
                "  [Ln 1, Col 3 - Ln 1, Col 6] BooleanConstantExpression (True)\n" +
                "  [Ln 2, Col 2 - Ln 2, Col 6] AssignmentStatement\n" +
                "    [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (x)\n" +
                "    [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)\n" +
                "  [Ln 3, Col 0 - Ln 8, Col 6] IfStatement\n" +
                "    [Ln 3, Col 5 - Ln 3, Col 9] BooleanConstantExpression (False)\n" +
                "    [Ln 4, Col 2 - Ln 4, Col 6] AssignmentStatement\n" +
                "      [Ln 4, Col 2 - Ln 4, Col 2] IdentifierExpression (x)\n" +
                "      [Ln 4, Col 6 - Ln 4, Col 6] NumberConstantExpression (2)\n" +
                "    [Ln 5, Col 0 - Ln 8, Col 6] IfStatement\n" +
                "      [Ln 5, Col 5 - Ln 5, Col 9] ComparisonExpression\n" +
                "        [Ln 5, Col 5 - Ln 5, Col 5] NumberConstantExpression (1) (<)\n" +
                "        [Ln 5, Col 9 - Ln 5, Col 9] NumberConstantExpression (2)\n" +
                "      [Ln 6, Col 2 - Ln 6, Col 6] AssignmentStatement\n" +
                "        [Ln 6, Col 2 - Ln 6, Col 2] IdentifierExpression (x)\n" +
                "        [Ln 6, Col 6 - Ln 6, Col 6] NumberConstantExpression (3)\n" +
                "      [Ln 8, Col 2 - Ln 8, Col 6] AssignmentStatement\n" +
                "        [Ln 8, Col 2 - Ln 8, Col 2] IdentifierExpression (x)\n" +
                "        [Ln 8, Col 6 - Ln 8, Col 6] NumberConstantExpression (4)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Boolean [Ln 1, Col 3 - Ln 1, Col 6]," +
                "Symbol [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Variable [Ln 2, Col 2 - Ln 2, Col 2]," +
                "Operator [Ln 2, Col 4 - Ln 2, Col 4]," +
                "Number [Ln 2, Col 6 - Ln 2, Col 6]," +
                "Keyword [Ln 3, Col 0 - Ln 3, Col 3]," +
                "Boolean [Ln 3, Col 5 - Ln 3, Col 9]," +
                "Symbol [Ln 3, Col 10 - Ln 3, Col 10]," +
                "Variable [Ln 4, Col 2 - Ln 4, Col 2]," +
                "Operator [Ln 4, Col 4 - Ln 4, Col 4]," +
                "Number [Ln 4, Col 6 - Ln 4, Col 6]," +
                "Keyword [Ln 5, Col 0 - Ln 5, Col 3]," +
                "Number [Ln 5, Col 5 - Ln 5, Col 5]," +
                "Operator [Ln 5, Col 7 - Ln 5, Col 7]," +
                "Number [Ln 5, Col 9 - Ln 5, Col 9]," +
                "Symbol [Ln 5, Col 10 - Ln 5, Col 10]," +
                "Variable [Ln 6, Col 2 - Ln 6, Col 2]," +
                "Operator [Ln 6, Col 4 - Ln 6, Col 4]," +
                "Number [Ln 6, Col 6 - Ln 6, Col 6]," +
                "Keyword [Ln 7, Col 0 - Ln 7, Col 3]," +
                "Symbol [Ln 7, Col 4 - Ln 7, Col 4]," +
                "Variable [Ln 8, Col 2 - Ln 8, Col 2]," +
                "Operator [Ln 8, Col 4 - Ln 8, Col 4]," +
                "Number [Ln 8, Col 6 - Ln 8, Col 6]")]

    [InlineData("while True: x = 1",

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

    [InlineData("while True:\n" +
                "  x = 1\n" +
                "  y = 2",

                "[Ln 1, Col 0 - Ln 3, Col 6] WhileStatement\n" +
                "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)\n" +
                "  [Ln 2, Col 2 - Ln 3, Col 6] BlockStatement\n" +
                "    [Ln 2, Col 2 - Ln 2, Col 6] AssignmentStatement\n" +
                "      [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (x)\n" +
                "      [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)\n" +
                "    [Ln 3, Col 2 - Ln 3, Col 6] AssignmentStatement\n" +
                "      [Ln 3, Col 2 - Ln 3, Col 2] IdentifierExpression (y)\n" +
                "      [Ln 3, Col 6 - Ln 3, Col 6] NumberConstantExpression (2)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 4]," +
                "Boolean [Ln 1, Col 6 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Variable [Ln 2, Col 2 - Ln 2, Col 2]," +
                "Operator [Ln 2, Col 4 - Ln 2, Col 4]," +
                "Number [Ln 2, Col 6 - Ln 2, Col 6]," +
                "Variable [Ln 3, Col 2 - Ln 3, Col 2]," +
                "Operator [Ln 3, Col 4 - Ln 3, Col 4]," +
                "Number [Ln 3, Col 6 - Ln 3, Col 6]")]

    [InlineData("while True: x = 1; y = 2",

                "[Ln 1, Col 0 - Ln 1, Col 23] WhileStatement\n" +
                "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)\n" +
                "  [Ln 1, Col 12 - Ln 1, Col 23] BlockStatement\n" +
                "    [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement\n" +
                "      [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x)\n" +
                "      [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 19 - Ln 1, Col 23] AssignmentStatement\n" +
                "      [Ln 1, Col 19 - Ln 1, Col 19] IdentifierExpression (y)\n" +
                "      [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (2)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 4]," +
                "Boolean [Ln 1, Col 6 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Variable [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Operator [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Number [Ln 1, Col 16 - Ln 1, Col 16]," +
                "Symbol [Ln 1, Col 17 - Ln 1, Col 17]," +
                "Variable [Ln 1, Col 19 - Ln 1, Col 19]," +
                "Operator [Ln 1, Col 21 - Ln 1, Col 21]," +
                "Number [Ln 1, Col 23 - Ln 1, Col 23]")]

    [InlineData("while True: x = 1; y = 2;",

                "[Ln 1, Col 0 - Ln 1, Col 23] WhileStatement\n" +
                "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)\n" +
                "  [Ln 1, Col 12 - Ln 1, Col 23] BlockStatement\n" +
                "    [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement\n" +
                "      [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x)\n" +
                "      [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 19 - Ln 1, Col 23] AssignmentStatement\n" +
                "      [Ln 1, Col 19 - Ln 1, Col 19] IdentifierExpression (y)\n" +
                "      [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (2)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 4]," +
                "Boolean [Ln 1, Col 6 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Variable [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Operator [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Number [Ln 1, Col 16 - Ln 1, Col 16]," +
                "Symbol [Ln 1, Col 17 - Ln 1, Col 17]," +
                "Variable [Ln 1, Col 19 - Ln 1, Col 19]," +
                "Operator [Ln 1, Col 21 - Ln 1, Col 21]," +
                "Number [Ln 1, Col 23 - Ln 1, Col 23]," +
                "Symbol [Ln 1, Col 24 - Ln 1, Col 24]")]

    [InlineData("def func(): x = 1",

                "[Ln 1, Col 0 - Ln 1, Col 16] FunctionStatement (func:)\n" +
                "  [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement\n" +
                "    [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x)\n" +
                "    [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 2]," +
                "Function [Ln 1, Col 4 - Ln 1, Col 7]," +
                "Parenthesis [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Parenthesis [Ln 1, Col 9 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Variable [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Operator [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Number [Ln 1, Col 16 - Ln 1, Col 16]")]

    [InlineData("def func(a, b):\n" +
                "  return a + b",

                "[Ln 1, Col 0 - Ln 2, Col 13] FunctionStatement (func: a, b)\n" +
                "  [Ln 2, Col 2 - Ln 2, Col 13] ReturnStatement\n" +
                "    [Ln 2, Col 9 - Ln 2, Col 13] BinaryExpression (+)\n" +
                "      [Ln 2, Col 9 - Ln 2, Col 9] IdentifierExpression (a)\n" +
                "      [Ln 2, Col 13 - Ln 2, Col 13] IdentifierExpression (b)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 2]," +
                "Function [Ln 1, Col 4 - Ln 1, Col 7]," +
                "Parenthesis [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Parameter [Ln 1, Col 9 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Parameter [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Parenthesis [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Symbol [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Keyword [Ln 2, Col 2 - Ln 2, Col 7]," +
                "Variable [Ln 2, Col 9 - Ln 2, Col 9]," +
                "Operator [Ln 2, Col 11 - Ln 2, Col 11]," +
                "Variable [Ln 2, Col 13 - Ln 2, Col 13]")]

    [InlineData("def func(a, b):\n" +
                "  c = a + b\n" +
                "  return c",

                "[Ln 1, Col 0 - Ln 3, Col 9] FunctionStatement (func: a, b)\n" +
                "  [Ln 2, Col 2 - Ln 3, Col 9] BlockStatement\n" +
                "    [Ln 2, Col 2 - Ln 2, Col 10] AssignmentStatement\n" +
                "      [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (c)\n" +
                "      [Ln 2, Col 6 - Ln 2, Col 10] BinaryExpression (+)\n" +
                "        [Ln 2, Col 6 - Ln 2, Col 6] IdentifierExpression (a)\n" +
                "        [Ln 2, Col 10 - Ln 2, Col 10] IdentifierExpression (b)\n" +
                "    [Ln 3, Col 2 - Ln 3, Col 9] ReturnStatement\n" +
                "      [Ln 3, Col 9 - Ln 3, Col 9] IdentifierExpression (c)",

                "Keyword [Ln 1, Col 0 - Ln 1, Col 2]," +
                "Function [Ln 1, Col 4 - Ln 1, Col 7]," +
                "Parenthesis [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Parameter [Ln 1, Col 9 - Ln 1, Col 9]," +
                "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Parameter [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Parenthesis [Ln 1, Col 13 - Ln 1, Col 13]," +
                "Symbol [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Variable [Ln 2, Col 2 - Ln 2, Col 2]," +
                "Operator [Ln 2, Col 4 - Ln 2, Col 4]," +
                "Variable [Ln 2, Col 6 - Ln 2, Col 6]," +
                "Operator [Ln 2, Col 8 - Ln 2, Col 8]," +
                "Variable [Ln 2, Col 10 - Ln 2, Col 10]," +
                "Keyword [Ln 3, Col 2 - Ln 3, Col 7]," +
                "Variable [Ln 3, Col 9 - Ln 3, Col 9]")]
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
