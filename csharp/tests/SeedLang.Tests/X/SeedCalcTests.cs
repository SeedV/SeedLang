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
using System.Globalization;
using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedCalcTests {
    private readonly DiagnosticCollection _collection = new DiagnosticCollection();
    private readonly SeedCalc _parser = new SeedCalc();

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
      Assert.Equal(result, _parser.Validate(input, "", _collection));
    }

    [Theory]
    [InlineData("0",

                "[Ln 1, Col 0 - Ln 1, Col 0] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (0)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]")]

    [InlineData("0\n",

                "[Ln 1, Col 0 - Ln 1, Col 0] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (0)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]")]

    [InlineData("0\n\n",

                "[Ln 1, Col 0 - Ln 1, Col 0] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (0)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]")]

    [InlineData("0.",

                "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] NumberConstantExpression (0)",

                "Number [Ln 1, Col 0 - Ln 1, Col 1]")]

    [InlineData(".0",

                "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] NumberConstantExpression (0)",

                "Number [Ln 1, Col 0 - Ln 1, Col 1]")]

    [InlineData(".5",

                "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] NumberConstantExpression (0.5)",

                "Number [Ln 1, Col 0 - Ln 1, Col 1]")]

    [InlineData("1.5",

                "[Ln 1, Col 0 - Ln 1, Col 2] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 2] NumberConstantExpression (1.5)",

                "Number [Ln 1, Col 0 - Ln 1, Col 2]")]

    [InlineData("1e3",

                "[Ln 1, Col 0 - Ln 1, Col 2] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 2] NumberConstantExpression (1000)",

                "Number [Ln 1, Col 0 - Ln 1, Col 2]")]

    [InlineData("1e+20",

                "[Ln 1, Col 0 - Ln 1, Col 4] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 4] NumberConstantExpression (1E+20)",

                "Number [Ln 1, Col 0 - Ln 1, Col 4]")]

    [InlineData("1e-5",

                "[Ln 1, Col 0 - Ln 1, Col 3] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 3] NumberConstantExpression (1E-05)",

                "Number [Ln 1, Col 0 - Ln 1, Col 3]")]

    [InlineData("1 + 2",

                "[Ln 1, Col 0 - Ln 1, Col 4] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 4] BinaryExpression (+)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("1 - 2 * 3",

                "[Ln 1, Col 0 - Ln 1, Col 8] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 8] BinaryExpression (*)\n" +
                "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                "      [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (3)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Number [Ln 1, Col 8 - Ln 1, Col 8]")]

    [InlineData("(1 + 2) / 3",

                "[Ln 1, Col 0 - Ln 1, Col 10] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 10] BinaryExpression (/)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 6] BinaryExpression (+)\n" +
                "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (2)\n" +
                "    [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (3)",

                "OpenParenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "CloseParenthesis [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Number [Ln 1, Col 10 - Ln 1, Col 10]")]

    [InlineData("-1 + 2",

                "[Ln 1, Col 0 - Ln 1, Col 5] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 5] BinaryExpression (+)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 1] UnaryExpression (-)\n" +
                "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (2)",

                "Operator [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("-(1 + 2)",

                "[Ln 1, Col 0 - Ln 1, Col 7] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 7] UnaryExpression (-)\n" +
                "    [Ln 1, Col 1 - Ln 1, Col 7] BinaryExpression (+)\n" +
                "      [Ln 1, Col 2 - Ln 1, Col 2] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (2)",

                "Operator [Ln 1, Col 0 - Ln 1, Col 0]," +
                "OpenParenthesis [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Number [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "CloseParenthesis [Ln 1, Col 7 - Ln 1, Col 7]")]

    [InlineData("2 - - 1",

                "[Ln 1, Col 0 - Ln 1, Col 6] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 6] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (2)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 6] UnaryExpression (-)\n" +
                "      [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (1)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]")]

    [InlineData("2 - -(-1)",

                "[Ln 1, Col 0 - Ln 1, Col 8] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (2)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 8] UnaryExpression (-)\n" +
                "      [Ln 1, Col 5 - Ln 1, Col 8] UnaryExpression (-)\n" +
                "        [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (1)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "OpenParenthesis [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                "CloseParenthesis [Ln 1, Col 8 - Ln 1, Col 8]")]

    [InlineData("1 + + 2 * - 3 - - 4 / 5",

                "[Ln 1, Col 0 - Ln 1, Col 22] ExpressionStatement\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 22] BinaryExpression (-)\n" +
                "    [Ln 1, Col 0 - Ln 1, Col 12] BinaryExpression (+)\n" +
                "      [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "      [Ln 1, Col 4 - Ln 1, Col 12] BinaryExpression (*)\n" +
                "        [Ln 1, Col 4 - Ln 1, Col 6] UnaryExpression (+)\n" +
                "          [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (2)\n" +
                "        [Ln 1, Col 10 - Ln 1, Col 12] UnaryExpression (-)\n" +
                "          [Ln 1, Col 12 - Ln 1, Col 12] NumberConstantExpression (3)\n" +
                "    [Ln 1, Col 16 - Ln 1, Col 22] BinaryExpression (/)\n" +
                "      [Ln 1, Col 16 - Ln 1, Col 18] UnaryExpression (-)\n" +
                "        [Ln 1, Col 18 - Ln 1, Col 18] NumberConstantExpression (4)\n" +
                "      [Ln 1, Col 22 - Ln 1, Col 22] NumberConstantExpression (5)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Operator [Ln 1, Col 10 - Ln 1, Col 10]," +
                "Number [Ln 1, Col 12 - Ln 1, Col 12]," +
                "Operator [Ln 1, Col 14 - Ln 1, Col 14]," +
                "Operator [Ln 1, Col 16 - Ln 1, Col 16]," +
                "Number [Ln 1, Col 18 - Ln 1, Col 18]," +
                "Operator [Ln 1, Col 20 - Ln 1, Col 20]," +
                "Number [Ln 1, Col 22 - Ln 1, Col 22]")]
    public void TestSeedCalcParser(string input, string expected, string expectedTokens) {
      Assert.True(_parser.Parse(input, "", _collection, out Statement statement,
                                out IReadOnlyList<TokenInfo> tokens));
      Assert.NotNull(statement);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected.Replace("\n", Environment.NewLine), statement.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }

    [Theory]
    [InlineData("-",
                new string[] {
                  "Mismatched input. Found token: '<EOF>'. Expected token: {'(', NUMBER}",
                },

                "Operator [Ln 1, Col 0 - Ln 1, Col 0]")]

    [InlineData("3-",
                new string[] {
                  "Mismatched input. Found token: '<EOF>'. Expected token: {'+', '-', '(', NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 1 - Ln 1, Col 1]")]

    [InlineData("3+4-",
                new string[] {
                  "Mismatched input. Found token: '<EOF>'. Expected token: {'+', '-', '(', NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Number [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]")]

    [InlineData("3+--4-",
                new string[] {
                  "Unwanted token. Found token: '-'. Expected token: {'(', NUMBER}",
                  "Mismatched input. Found token: '<EOF>'. Expected token: {'+', '-', '(', NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData("3++--4-",
                new string[] {
                  "Mismatched input. Found token: '-'. Expected token: {'(', NUMBER}",
                  "Mismatched input. Found token: '<EOF>'. Expected token: {'+', '-', '(', NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 6]")]

    [InlineData(".",
                new string[] {
                  "Mismatched input. Found token: '.'. Expected token: {'+', '-', '(', NUMBER}",
                },

                "Unknown [Ln 1, Col 0 - Ln 1, Col 0]")]

    [InlineData(".3.",
                new string[] {
                  "Unwanted token. Found token: '.'. Expected token: {<EOF>, NEWLINE}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Unknown [Ln 1, Col 2 - Ln 1, Col 2]")]

    [InlineData(".3@",
                new string[] {
                  "Unwanted token. Found token: '@'. Expected token: {<EOF>, NEWLINE}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 1]," +
                "Unknown [Ln 1, Col 2 - Ln 1, Col 2]")]

    [InlineData("3+++4",
                new string[] {
                  "Unwanted token. Found token: '+'. Expected token: {'(', NUMBER}",
                },

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]")]
    public void TestParsePartialOrInvalidExpressions(string input, string[] errorMessages,
                                                     string expectedTokens) {
      CultureInfo.CurrentCulture = new CultureInfo("en-US");
      Assert.False(_parser.Parse(input, "", _collection, out Statement statement,
                   out IReadOnlyList<TokenInfo> semanticTokens));
      Assert.Null(statement);
      Assert.Equal(errorMessages.Length, _collection.Diagnostics.Count);
      for (int i = 0; i < errorMessages.Length; ++i) {
        Assert.Equal(SystemReporters.SeedX, _collection.Diagnostics[i].Reporter);
        Assert.Equal(Severity.Fatal, _collection.Diagnostics[i].Severity);
        Assert.Equal(errorMessages[i], _collection.Diagnostics[i].LocalizedMessage);
      }
      Assert.Null(semanticTokens);
      IReadOnlyList<TokenInfo> tokens = _parser.ParseSyntaxTokens(input);
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
