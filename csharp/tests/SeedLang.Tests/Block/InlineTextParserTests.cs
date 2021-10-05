using System.Collections.Generic;
using System.Linq;
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

using System.Text;
using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Block.Tests {
  class PartialParseListener : InlineTextParser.IInlineTextListener {
    public readonly StringBuilder Buffer = new StringBuilder();
    public readonly string Text;

    public PartialParseListener(string text) {
      Text = text;
    }

    public void VisitArithmeticOperator(string op, TextRange range) {
      Buffer.Append(op);
    }

    public void VisitIdentifier(string name, TextRange range) {
      Buffer.Append(name);
    }

    public void VisitNumber(string number, TextRange range) {
      Buffer.Append(number);
    }

    public void VisitString(string str, TextRange range) {
      Buffer.Append(str);
    }

    public void VisitOpenParen(TextRange range) {
      Buffer.Append('(');
    }

    public void VisitCloseParen(TextRange range) {
      Buffer.Append(')');
    }

    public void VisitInvalidToken(TextRange range) {
      string token = Text.Substring(range.Start.Column, range.End.Column - range.Start.Column + 1);
      Buffer.Append($"#{token}#");
    }
  }

  public class InlineTextParserTests {
    private readonly DiagnosticCollection _collection = new DiagnosticCollection();
    private readonly InlineTextParser _parser = new InlineTextParser();

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
      Assert.Equal(result, _parser.Validate(input, "", ParseRule.Number, _collection));
    }

    [Theory]
    [InlineData(ParseRule.Number, "0",
                "[Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (0)",
                "Number [Ln 1, Col 0 - Ln 1, Col 0]")]
    [InlineData(ParseRule.Number, "0.",
                "[Ln 1, Col 0 - Ln 1, Col 1] NumberConstantExpression (0)",
                "Number [Ln 1, Col 0 - Ln 1, Col 1]")]
    [InlineData(ParseRule.Number, ".0",
                "[Ln 1, Col 0 - Ln 1, Col 1] NumberConstantExpression (0)",
                "Number [Ln 1, Col 0 - Ln 1, Col 1]")]
    [InlineData(ParseRule.Number, ".5",
                "[Ln 1, Col 0 - Ln 1, Col 1] NumberConstantExpression (0.5)",
                "Number [Ln 1, Col 0 - Ln 1, Col 1]")]
    [InlineData(ParseRule.Number, "1.5",
                "[Ln 1, Col 0 - Ln 1, Col 2] NumberConstantExpression (1.5)",
                "Number [Ln 1, Col 0 - Ln 1, Col 2]")]
    [InlineData(ParseRule.Number, "1e3",
                "[Ln 1, Col 0 - Ln 1, Col 2] NumberConstantExpression (1000)",
                "Number [Ln 1, Col 0 - Ln 1, Col 2]")]
    [InlineData(ParseRule.Number, "1e+20",
                "[Ln 1, Col 0 - Ln 1, Col 4] NumberConstantExpression (1E+20)",
                "Number [Ln 1, Col 0 - Ln 1, Col 4]")]
    [InlineData(ParseRule.Number, "1e-5",
                "[Ln 1, Col 0 - Ln 1, Col 3] NumberConstantExpression (1E-05)",
                "Number [Ln 1, Col 0 - Ln 1, Col 3]")]

    [InlineData(ParseRule.Expression, "1 + 2",

                "[Ln 1, Col 0 - Ln 1, Col 4] BinaryExpression (+)\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "  [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData(ParseRule.Expression, "1 - 2 * 3",

                "[Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (-)\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                "  [Ln 1, Col 4 - Ln 1, Col 8] BinaryExpression (*)\n" +
                "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                "    [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (3)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Number [Ln 1, Col 8 - Ln 1, Col 8]")]

    [InlineData(ParseRule.Expression, "(1 + 2) / 3",

                "[Ln 1, Col 0 - Ln 1, Col 10] BinaryExpression (/)\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 6] BinaryExpression (+)\n" +
                "    [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (2)\n" +
                "  [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (3)",

                "Symbol [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Symbol [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                "Number [Ln 1, Col 10 - Ln 1, Col 10]")]

    [InlineData(ParseRule.Expression, "-1 + 2",

                "[Ln 1, Col 0 - Ln 1, Col 5] BinaryExpression (+)\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 1] UnaryExpression (-)\n" +
                "    [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                "  [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (2)",

                "Operator [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]")]

    [InlineData(ParseRule.Expression, "-(1 + 2)",

                "[Ln 1, Col 0 - Ln 1, Col 7] UnaryExpression (-)\n" +
                "  [Ln 1, Col 1 - Ln 1, Col 7] BinaryExpression (+)\n" +
                "    [Ln 1, Col 2 - Ln 1, Col 2] NumberConstantExpression (1)\n" +
                "    [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (2)",

                "Operator [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Symbol [Ln 1, Col 1 - Ln 1, Col 1]," +
                "Number [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                "Symbol [Ln 1, Col 7 - Ln 1, Col 7]")]

    [InlineData(ParseRule.Expression, "2 - - 1",

                "[Ln 1, Col 0 - Ln 1, Col 6] BinaryExpression (-)\n" +
                "  [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (2)\n" +
                "  [Ln 1, Col 4 - Ln 1, Col 6] UnaryExpression (-)\n" +
                "    [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (1)",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "Operator [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 6 - Ln 1, Col 6]")]
    public void TestInlineTextParser(ParseRule rule, string input, string expected,
                                     string expectedTokens) {
      Assert.True(_parser.Parse(input, "", rule, _collection, out AstNode node,
                                out IReadOnlyList<SyntaxToken> tokens));
      Assert.NotNull(node);
      Assert.Empty(_collection.Diagnostics);
      Assert.Equal(expected, node.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens.Select(token => token.ToString())));
    }

    [Theory]
    [InlineData("-", "-")]
    [InlineData("3-", "3-")]
    [InlineData("3+4-", "3+4-")]
    [InlineData("3+--4-", "3+--4-")]
    [InlineData("3++--4-", "3++--4-")]
    [InlineData(".", "#.#")]
    [InlineData(".3.", ".3#.#")]
    [InlineData(".3@", ".3#@#")]
    public void TestParsePartialOrInvalidExpressions(string input, string expected) {
      var parser = new InlineTextParser();
      var listener = new PartialParseListener(input);
      parser.VisitInlineText(input, listener);
      Assert.Equal(expected, listener.Buffer.ToString());
    }
  }
}
