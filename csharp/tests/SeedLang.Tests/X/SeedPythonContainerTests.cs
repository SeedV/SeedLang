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
  public class SeedPythonContainerTests {
    [Fact]
    public void TestEmptyDict() {
      string source = "{}";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 1] DictExpression";
      string expectedTokens = "OpenBrace [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "CloseBrace [Ln 1, Col 1 - Ln 1, Col 1]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestDict() {
      string source = "{'a': 1, 'b': 2, 'c': 3}";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 23] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 23] DictExpression\n" +
                        "    [Ln 1, Col 1 - Ln 1, Col 3] StringConstantExpression (a)\n" +
                        "    [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 9 - Ln 1, Col 11] StringConstantExpression (b)\n" +
                        "    [Ln 1, Col 14 - Ln 1, Col 14] NumberConstantExpression (2)\n" +
                        "    [Ln 1, Col 17 - Ln 1, Col 19] StringConstantExpression (c)\n" +
                        "    [Ln 1, Col 22 - Ln 1, Col 22] NumberConstantExpression (3)";
      string expectedTokens = "OpenBrace [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "String [Ln 1, Col 1 - Ln 1, Col 3]," +
                              "Symbol [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                              "String [Ln 1, Col 9 - Ln 1, Col 11]," +
                              "Symbol [Ln 1, Col 12 - Ln 1, Col 12]," +
                              "Number [Ln 1, Col 14 - Ln 1, Col 14]," +
                              "String [Ln 1, Col 17 - Ln 1, Col 19]," +
                              "Symbol [Ln 1, Col 20 - Ln 1, Col 20]," +
                              "Number [Ln 1, Col 22 - Ln 1, Col 22]," +
                              "CloseBrace [Ln 1, Col 23 - Ln 1, Col 23]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestEmptyList() {
      string source = "[]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 1] ListExpression";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "CloseBracket [Ln 1, Col 1 - Ln 1, Col 1]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestList() {
      string source = "[1, 2, 3]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 8] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "    [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "    [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptIndex() {
      string source = "[1, 2, 3][1]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 11] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 11] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (1)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Number [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "CloseBracket [Ln 1, Col 11 - Ln 1, Col 11]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptSlice() {
      string source = "[1, 2, 3][1:3:2]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 15] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 15] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 14] SliceExpression (start:stop:step)\n" +
                        "      [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 12 - Ln 1, Col 12] NumberConstantExpression (3)\n" +
                        "      [Ln 1, Col 14 - Ln 1, Col 14] NumberConstantExpression (2)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Number [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "Symbol [Ln 1, Col 11 - Ln 1, Col 11]," +
                              "Number [Ln 1, Col 12 - Ln 1, Col 12]," +
                              "Symbol [Ln 1, Col 13 - Ln 1, Col 13]," +
                              "Number [Ln 1, Col 14 - Ln 1, Col 14]," +
                              "CloseBracket [Ln 1, Col 15 - Ln 1, Col 15]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptEmptySlice() {
      string source = "[1, 2, 3][:]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 11] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 11] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 10] SliceExpression (::)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "CloseBracket [Ln 1, Col 11 - Ln 1, Col 11]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptEmptySliceWithTwoColons() {
      string source = "[1, 2, 3][::]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 12] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 12] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 11] SliceExpression (::)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "Symbol [Ln 1, Col 11 - Ln 1, Col 11]," +
                              "CloseBracket [Ln 1, Col 12 - Ln 1, Col 12]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptSliceWithStart() {
      string source = "[1, 2, 3][1:]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 12] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 12] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 11] SliceExpression (start::)\n" +
                        "      [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (1)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Number [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "Symbol [Ln 1, Col 11 - Ln 1, Col 11]," +
                              "CloseBracket [Ln 1, Col 12 - Ln 1, Col 12]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptSliceWithStop() {
      string source = "[1, 2, 3][:3:]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 13] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 13] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 12] SliceExpression (:stop:)\n" +
                        "      [Ln 1, Col 11 - Ln 1, Col 11] NumberConstantExpression (3)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "Number [Ln 1, Col 11 - Ln 1, Col 11]," +
                              "Symbol [Ln 1, Col 12 - Ln 1, Col 12]," +
                              "CloseBracket [Ln 1, Col 13 - Ln 1, Col 13]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptSliceWithStep() {
      string source = "[1, 2, 3][::2]";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 13] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 13] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 12] SliceExpression (::step)\n" +
                        "      [Ln 1, Col 12 - Ln 1, Col 12] NumberConstantExpression (2)";
      string expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                              "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                              "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                              "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                              "Symbol [Ln 1, Col 10 - Ln 1, Col 10]," +
                              "Symbol [Ln 1, Col 11 - Ln 1, Col 11]," +
                              "Number [Ln 1, Col 12 - Ln 1, Col 12]," +
                              "CloseBracket [Ln 1, Col 13 - Ln 1, Col 13]";
      TestPythonParser(source, expected, expectedTokens);
    }

    [Fact]
    public void TestSubscriptAssignment() {
      string source = "a[0] = 1";
      string expected = "[Ln 1, Col 0 - Ln 1, Col 7] AssignmentStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 3] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (a)\n" +
                        "    [Ln 1, Col 2 - Ln 1, Col 2] NumberConstantExpression (0) =\n" +
                        "  [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (1)";
      string expectedTokens = "Variable [Ln 1, Col 0 - Ln 1, Col 0]," +
                              "OpenBracket [Ln 1, Col 1 - Ln 1, Col 1]," +
                              "Number [Ln 1, Col 2 - Ln 1, Col 2]," +
                              "CloseBracket [Ln 1, Col 3 - Ln 1, Col 3]," +
                              "Operator [Ln 1, Col 5 - Ln 1, Col 5]," +
                              "Number [Ln 1, Col 7 - Ln 1, Col 7]";
      TestPythonParser(source, expected, expectedTokens);
    }

    private static void TestPythonParser(string source, string expected, string expectedTokens) {
      var collection = new DiagnosticCollection();
      Assert.True(new SeedPython().Parse(source, "", collection, out Statement statement,
                                         out IReadOnlyList<TokenInfo> tokens));
      Assert.NotNull(statement);
      Assert.Empty(collection.Diagnostics);
      Assert.Equal(expected.Replace("\n", Environment.NewLine), statement.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
