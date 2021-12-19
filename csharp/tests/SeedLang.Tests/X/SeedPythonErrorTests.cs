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
  public class SeedPythonErrorTests {
    private readonly DiagnosticCollection _collection = new DiagnosticCollection();
    private readonly SeedPython _parser = new SeedPython();

    [Theory]
    [InlineData("1.2 =",
                new string[] {
                  "SyntaxErrorUnwantedToken '=' {';', NEWLINE}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("1 +",
                new string[] {
                  @"SyntaxErrorInputMismatch '\n' " +
                  @"{'True', 'False', 'None', '+', '-', '(', '[', NAME, NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]")]

    [InlineData("1 + (",
                new string[] {
                  @"SyntaxErrorNoViableAlternative '(\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Parenthesis [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("1 + ((",
                new string[] {
                  @"SyntaxErrorNoViableAlternative '((\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Parenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Parenthesis [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("1 + (((",
                new string[] {
                  @"SyntaxErrorNoViableAlternative '(((\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Parenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Parenthesis [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Parenthesis [Ln 1, Col 6 - Ln 1, Col 6]")]

    [InlineData("1 + (2 - 1",
                new string[] {
                  @"SyntaxErrorNoViableAlternative '(2-1\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Parenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Number [Ln 1, Col 9 - Ln 1, Col 9]")]

    [InlineData("1 + ))",
                new string[] {
                  "SyntaxErrorInputMismatch ')' " +
                  "{'True', 'False', 'None', '+', '-', '(', '[', NAME, NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Parenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Parenthesis [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("1 < 2 >=",
                new string[] {
                  @"SyntaxErrorInputMismatch '\n' " +
                  @"{'True', 'False', 'None', '+', '-', '(', '[', NAME, NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 7]")]

    [InlineData("6(5 * 6)",
                new string[] {
                  "SyntaxErrorNoViableAlternative '6(5'",
                  "SyntaxErrorUnwantedToken ')' {';', NEWLINE}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Parenthesis [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Number [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Parenthesis [Ln 1, Col 7 - Ln 1, Col 7]")]

    [InlineData("while True",
                new string[] {
                  @"SyntaxErrorMissingToken '\n' ':'",
                  "SyntaxErrorInputMismatch '<EOF>' INDENT",
                },

                "Keyword [Ln 1, Col 0 - Ln 1, Col 4]," +
                "Keyword [Ln 1, Col 6 - Ln 1, Col 9]")]
    public void TestParseSyntaxError(string input, string[] errorMessages, string expectedTokens) {
      Assert.False(_parser.Parse(input, "", _collection, out AstNode node,
                                 out IReadOnlyList<SyntaxToken> tokens));
      Assert.Null(node);
      Assert.Equal(errorMessages.Length, _collection.Diagnostics.Count);
      for (int i = 0; i < errorMessages.Length; ++i) {
        Assert.Equal(SystemReporters.SeedX, _collection.Diagnostics[i].Reporter);
        Assert.Equal(Severity.Fatal, _collection.Diagnostics[i].Severity);
        string diagnostic = _collection.Diagnostics[i].LocalizedMessage.Replace(@"\r\n", @"\n");
        Assert.Equal(errorMessages[i], diagnostic);
      }
      Assert.Equal(expectedTokens, string.Join(",", tokens.Select(token => token.ToString())));
    }
  }
}
