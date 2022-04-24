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
  public class SeedPythonDictTests {
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

    private static void TestPythonParser(string source, string expected, string expectedTokens) {
      var collection = new DiagnosticCollection();
      var parser = new SeedPython();
      Assert.True(parser.Parse(source, "", collection, out Statement statement,
                               out IReadOnlyList<TokenInfo> tokens));
      Assert.NotNull(statement);
      Assert.Empty(collection.Diagnostics);
      Assert.Equal(expected.Replace("\n", Environment.NewLine), statement.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
