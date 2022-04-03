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

using System.Collections.Generic;
using System.Linq;
using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonVTagTests {
    [Fact]
    public void TestVTag() {
      string source = "# [[ Swap ]]\nprint(1)";
      string expected = "[Ln 2, Col 0 - Ln 2, Col 7] ExpressionStatement\n" +
                        "  [Ln 2, Col 0 - Ln 2, Col 7] CallExpression\n" +
                        "    [Ln 2, Col 0 - Ln 2, Col 4] IdentifierExpression (print)\n" +
                        "    [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)";
      string expectedTokens = "Variable [Ln 2, Col 0 - Ln 2, Col 4]," +
                              "OpenParenthesis [Ln 2, Col 5 - Ln 2, Col 5]," +
                              "Number [Ln 2, Col 6 - Ln 2, Col 6]," +
                              "CloseParenthesis [Ln 2, Col 7 - Ln 2, Col 7]";
      TestPythonParser(source, expected, expectedTokens);
    }

    private static void TestPythonParser(string input, string expected, string expectedTokens) {
      var collection = new DiagnosticCollection();
      Assert.True(new SeedPython().Parse(input, "", collection, out AstNode node,
                                         out IReadOnlyList<TokenInfo> tokens));
      Assert.NotNull(node);
      Assert.Empty(collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens.Select(token => token.ToString())));
    }
  }
}
