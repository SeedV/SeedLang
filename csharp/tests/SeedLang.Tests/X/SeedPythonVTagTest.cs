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

using System;
using System.Collections.Generic;
using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonVTagTests {
    [Fact]
    public void TestSingleLineVTag() {
      string source = "# [[ Print ]]\n" +
                      "print(1)";
      string expected = "[Ln 1, Col 0 - Ln 2, Col 7] VTagStatement (Print)\n" +
                        "  [Ln 2, Col 0 - Ln 2, Col 7] ExpressionStatement\n" +
                        "    [Ln 2, Col 0 - Ln 2, Col 7] CallExpression\n" +
                        "      [Ln 2, Col 0 - Ln 2, Col 4] IdentifierExpression (print)\n" +
                        "      [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)";
      string expectedTokens = "Variable [Ln 2, Col 0 - Ln 2, Col 4]," +
                              "OpenParenthesis [Ln 2, Col 5 - Ln 2, Col 5]," +
                              "Number [Ln 2, Col 6 - Ln 2, Col 6]," +
                              "CloseParenthesis [Ln 2, Col 7 - Ln 2, Col 7]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSingleLineVTagWithParameters() {
      string source = "# [[ Assign(x, y + 1), Initialize(x) ]]\n" +
                      "x = y + 1";
      string expected = "[Ln 1, Col 0 - Ln 2, Col 8] VTagStatement (Assign(x,y+1):\n" +
                        "[Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x)\n" +
                        "[Ln 1, Col 15 - Ln 1, Col 19] BinaryExpression (+)\n" +
                        "  [Ln 1, Col 15 - Ln 1, Col 15] IdentifierExpression (y)\n" +
                        "  [Ln 1, Col 19 - Ln 1, Col 19] NumberConstantExpression (1)," +
                        "Initialize(x):\n" +
                        "[Ln 1, Col 34 - Ln 1, Col 34] IdentifierExpression (x))\n" +
                        "  [Ln 2, Col 0 - Ln 2, Col 8] AssignmentStatement\n" +
                        "    [Ln 2, Col 0 - Ln 2, Col 0] IdentifierExpression (x)\n" +
                        "    [Ln 2, Col 4 - Ln 2, Col 8] BinaryExpression (+)\n" +
                        "      [Ln 2, Col 4 - Ln 2, Col 4] IdentifierExpression (y)\n" +
                        "      [Ln 2, Col 8 - Ln 2, Col 8] NumberConstantExpression (1)";
      string expectedTokens = "Variable [Ln 2, Col 0 - Ln 2, Col 0]," +
                              "Operator [Ln 2, Col 2 - Ln 2, Col 2]," +
                              "Variable [Ln 2, Col 4 - Ln 2, Col 4]," +
                              "Operator [Ln 2, Col 6 - Ln 2, Col 6]," +
                              "Number [Ln 2, Col 8 - Ln 2, Col 8]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSingleLineVTagWithMultipleStatements() {
      string source = "# [[ Assign, Initialize ]]\n" +
                      "x = 1\n" +
                      "y = 2";
      string expected = "[Ln 1, Col 0 - Ln 3, Col 4] BlockStatement\n" +
                        "  [Ln 1, Col 0 - Ln 2, Col 4] VTagStatement (Assign,Initialize)\n" +
                        "    [Ln 2, Col 0 - Ln 2, Col 4] AssignmentStatement\n" +
                        "      [Ln 2, Col 0 - Ln 2, Col 0] IdentifierExpression (x)\n" +
                        "      [Ln 2, Col 4 - Ln 2, Col 4] NumberConstantExpression (1)\n" +
                        "  [Ln 3, Col 0 - Ln 3, Col 4] AssignmentStatement\n" +
                        "    [Ln 3, Col 0 - Ln 3, Col 0] IdentifierExpression (y)\n" +
                        "    [Ln 3, Col 4 - Ln 3, Col 4] NumberConstantExpression (2)";
      string expectedTokens = "Variable [Ln 2, Col 0 - Ln 2, Col 0]," +
                              "Operator [Ln 2, Col 2 - Ln 2, Col 2]," +
                              "Number [Ln 2, Col 4 - Ln 2, Col 4]," +
                              "Variable [Ln 3, Col 0 - Ln 3, Col 0]," +
                              "Operator [Ln 3, Col 2 - Ln 3, Col 2]," +
                              "Number [Ln 3, Col 4 - Ln 3, Col 4]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestMultipleLineVTag() {
      string source = "# [[ Assign, Initialize\n" +
                      "x = 1\n" +
                      "y = 2\n" +
                      "# ]]";
      string expected = "[Ln 1, Col 0 - Ln 4, Col 3] VTagStatement (Assign,Initialize)\n" +
                        "  [Ln 2, Col 0 - Ln 2, Col 4] AssignmentStatement\n" +
                        "    [Ln 2, Col 0 - Ln 2, Col 0] IdentifierExpression (x)\n" +
                        "    [Ln 2, Col 4 - Ln 2, Col 4] NumberConstantExpression (1)\n" +
                        "  [Ln 3, Col 0 - Ln 3, Col 4] AssignmentStatement\n" +
                        "    [Ln 3, Col 0 - Ln 3, Col 0] IdentifierExpression (y)\n" +
                        "    [Ln 3, Col 4 - Ln 3, Col 4] NumberConstantExpression (2)";
      string expectedTokens = "Variable [Ln 2, Col 0 - Ln 2, Col 0]," +
                              "Operator [Ln 2, Col 2 - Ln 2, Col 2]," +
                              "Number [Ln 2, Col 4 - Ln 2, Col 4]," +
                              "Variable [Ln 3, Col 0 - Ln 3, Col 0]," +
                              "Operator [Ln 3, Col 2 - Ln 3, Col 2]," +
                              "Number [Ln 3, Col 4 - Ln 3, Col 4]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestMultipleVTags() {
      string source = "if True:\n" +
                      "  # [[ Assign, Initialize ]]\n" +
                      "  x = 1";
      string expected = "[Ln 1, Col 0 - Ln 3, Col 6] IfStatement\n" +
                        "  [Ln 1, Col 3 - Ln 1, Col 6] BooleanConstantExpression (True)\n" +
                        "  [Ln 2, Col 2 - Ln 3, Col 6] VTagStatement (Assign,Initialize)\n" +
                        "    [Ln 3, Col 2 - Ln 3, Col 6] AssignmentStatement\n" +
                        "      [Ln 3, Col 2 - Ln 3, Col 2] IdentifierExpression (x)\n" +
                        "      [Ln 3, Col 6 - Ln 3, Col 6] NumberConstantExpression (1)";
      string expectedTokens = "Keyword [Ln 1, Col 0 - Ln 1, Col 1]," +
                              "Boolean [Ln 1, Col 3 - Ln 1, Col 6]," +
                              "Symbol [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "Variable [Ln 3, Col 2 - Ln 3, Col 2]," +
                              "Operator [Ln 3, Col 4 - Ln 3, Col 4]," +
                              "Number [Ln 3, Col 6 - Ln 3, Col 6]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestVTagForCompoundStatement() {
      string source = "# [[ Check ]]\n" +
                      "if True:\n" +
                      "  x = 1";
      string expected = "[Ln 1, Col 0 - Ln 3, Col 6] VTagStatement (Check)\n" +
                        "  [Ln 2, Col 0 - Ln 3, Col 6] IfStatement\n" +
                        "    [Ln 2, Col 3 - Ln 2, Col 6] BooleanConstantExpression (True)\n" +
                        "    [Ln 3, Col 2 - Ln 3, Col 6] AssignmentStatement\n" +
                        "      [Ln 3, Col 2 - Ln 3, Col 2] IdentifierExpression (x)\n" +
                        "      [Ln 3, Col 6 - Ln 3, Col 6] NumberConstantExpression (1)";
      string expectedTokens = "Keyword [Ln 2, Col 0 - Ln 2, Col 1]," +
                              "Boolean [Ln 2, Col 3 - Ln 2, Col 6]," +
                              "Symbol [Ln 2, Col 7 - Ln 2, Col 7]," +
                              "Variable [Ln 3, Col 2 - Ln 3, Col 2]," +
                              "Operator [Ln 3, Col 4 - Ln 3, Col 4]," +
                              "Number [Ln 3, Col 6 - Ln 3, Col 6]";
      TestPythonParser(source, expected, expectedTokens);
    }

    private static void TestPythonParser(string input, string expected, string expectedTokens) {
      var collection = new DiagnosticCollection();
      Assert.True(new SeedPython().Parse(input, "", collection, out AstNode node,
                                         out IReadOnlyList<TokenInfo> tokens));
      Assert.NotNull(node);
      Assert.Empty(collection.Diagnostics);
      Assert.Equal(expected.Replace("\n", Environment.NewLine), node.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
