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
using System.Globalization;
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
                  "Unwanted token. Found token: '='. Expected token: {';', NEWLINE}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("1 +",
                new string[] {
                  @"Mismatched input. Found token: '\n'. Expected token: " +
                  @"{'True', 'False', 'None', '+', '-', '(', '[', '{', NAME, NUMBER, STRING}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]")]

    [InlineData("1 + (",
                new string[] {
                  @"No viable alternative at input '(\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "OpenParenthesis [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("1 + ((",
                new string[] {
                  @"No viable alternative at input '((\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "OpenParenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "OpenParenthesis [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("1 + (((",
                new string[] {
                  @"No viable alternative at input '(((\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "OpenParenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "OpenParenthesis [Ln 1, Col 5 - Ln 1, Col 5]," +
                "OpenParenthesis [Ln 1, Col 6 - Ln 1, Col 6]")]

    [InlineData("1 + (2 - 1",
                new string[] {
                  @"No viable alternative at input '(2-1\n'",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "OpenParenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Number [Ln 1, Col 9 - Ln 1, Col 9]")]

    [InlineData("1 + ))",
                new string[] {
                  "Mismatched input. Found token: ')'. Expected token: " +
                  "{'True', 'False', 'None', '+', '-', '(', '[', '{', NAME, NUMBER, STRING}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "CloseParenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "CloseParenthesis [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("1 < 2 >=",
                new string[] {
                  @"Mismatched input. Found token: '\n'. Expected token: " +
                  @"{'True', 'False', 'None', '+', '-', '(', '[', '{', NAME, NUMBER, STRING}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 7]")]

    [InlineData("while True",
                new string[] {
                  @"Missing token. Found token: '\n'. Expected token: ':'",
                  "Mismatched input. Found token: '<EOF>'. Expected token: INDENT",
                },

                "Keyword [Ln 1, Col 0 - Ln 1, Col 4]," +
                "Boolean [Ln 1, Col 6 - Ln 1, Col 9]")]

    [InlineData("1e9999",
                new string[] {
                  "Overflow",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 5]")]
    public void TestParseSyntaxError(string input, string[] errorMessages, string expectedTokens) {
      CultureInfo.CurrentCulture = new CultureInfo("en-US");
      Assert.False(_parser.Parse(input, "", _collection, out Statement statement,
                                 out IReadOnlyList<TokenInfo> semanticTokens));
      Assert.Null(statement);
      Assert.Equal(errorMessages.Length, _collection.Diagnostics.Count);
      for (int i = 0; i < errorMessages.Length; ++i) {
        Assert.True(_collection.Diagnostics[i].Reporter == SystemReporters.SeedX ||
                    _collection.Diagnostics[i].Reporter == SystemReporters.SeedRuntime);
        Assert.Equal(Severity.Fatal, _collection.Diagnostics[i].Severity);
        string diagnostic = _collection.Diagnostics[i].LocalizedMessage.Replace(@"\r\n", @"\n");
        Assert.Equal(errorMessages[i], diagnostic);
      }
      Assert.Null(semanticTokens);
      IReadOnlyList<TokenInfo> tokens = _parser.ParseSyntaxTokens(input);
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
