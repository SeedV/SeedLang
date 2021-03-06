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
using FluentAssertions;
using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonExpressionTests {
    private readonly DiagnosticCollection _collection = new DiagnosticCollection();
    private readonly SeedPython _parser = new SeedPython();

    [Theory]
    [InlineData("1 + 2 * 3 - 4", true)]
    [InlineData("1 +", false)]
    public void TestValidateExpressionStatement(string input, bool result) {
      Assert.Equal(result, _parser.Validate(input, "", _collection));
    }

    [Fact]
    public void TestAssignment() {
      var source = "id = 1";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 5] AssignmentStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 1] IdentifierExpression (id) =\n" +
                        "  [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (1)";
      var expectedTokens = "Variable [Ln 1, Col 0 - Ln 1, Col 1]," +
                           "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                           "Number [Ln 1, Col 5 - Ln 1, Col 5]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestAttribute() {
      var source = "a.append(1)";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 10] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 10] CallExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 7] AttributeExpression\n" +
                        "      [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (a)\n" +
                        "      [Ln 1, Col 2 - Ln 1, Col 7] IdentifierExpression (append)\n" +
                        "    [Ln 1, Col 9 - Ln 1, Col 9] NumberConstantExpression (1)";
      var expectedTokens = "Variable [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Symbol [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Variable [Ln 1, Col 2 - Ln 1, Col 7]," +
                           "OpenParenthesis [Ln 1, Col 8 - Ln 1, Col 8]," +
                           "Number [Ln 1, Col 9 - Ln 1, Col 9]," +
                           "CloseParenthesis [Ln 1, Col 10 - Ln 1, Col 10]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBinary() {
      var source = "1 + 2";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 4] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 4] BinaryExpression (+)\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)";
      var expectedTokens = "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBinaryWithExtraParentheses() {
      var source = "(1 + 2) * (( 3 - -4 ))";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 21] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 21] BinaryExpression (*)\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 6] BinaryExpression (+)\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (2)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 21] BinaryExpression (-)\n" +
                        "      [Ln 1, Col 13 - Ln 1, Col 13] NumberConstantExpression (3)\n" +
                        "      [Ln 1, Col 17 - Ln 1, Col 18] UnaryExpression (-)\n" +
                        "        [Ln 1, Col 18 - Ln 1, Col 18] NumberConstantExpression (4)";
      var expectedTokens = "OpenParenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                           "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "CloseParenthesis [Ln 1, Col 6 - Ln 1, Col 6]," +
                           "Operator [Ln 1, Col 8 - Ln 1, Col 8]," +
                           "OpenParenthesis [Ln 1, Col 10 - Ln 1, Col 10]," +
                           "OpenParenthesis [Ln 1, Col 11 - Ln 1, Col 11]," +
                           "Number [Ln 1, Col 13 - Ln 1, Col 13]," +
                           "Operator [Ln 1, Col 15 - Ln 1, Col 15]," +
                           "Operator [Ln 1, Col 17 - Ln 1, Col 17]," +
                           "Number [Ln 1, Col 18 - Ln 1, Col 18]," +
                           "CloseParenthesis [Ln 1, Col 20 - Ln 1, Col 20]," +
                           "CloseParenthesis [Ln 1, Col 21 - Ln 1, Col 21]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBinaryWithUnary() {
      var source = "(1 + (2)) - (x) - -3";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 19] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 19] BinaryExpression (-)\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 14] BinaryExpression (-)\n" +
                        "      [Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (+)\n" +
                        "        [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "        [Ln 1, Col 5 - Ln 1, Col 7] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 12 - Ln 1, Col 14] IdentifierExpression (x)\n" +
                        "    [Ln 1, Col 18 - Ln 1, Col 19] UnaryExpression (-)\n" +
                        "      [Ln 1, Col 19 - Ln 1, Col 19] NumberConstantExpression (3)";
      var expectedTokens = "OpenParenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Operator [Ln 1, Col 3 - Ln 1, Col 3]," +
                           "OpenParenthesis [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                           "CloseParenthesis [Ln 1, Col 7 - Ln 1, Col 7]," +
                           "CloseParenthesis [Ln 1, Col 8 - Ln 1, Col 8]," +
                           "Operator [Ln 1, Col 10 - Ln 1, Col 10]," +
                           "OpenParenthesis [Ln 1, Col 12 - Ln 1, Col 12]," +
                           "Variable [Ln 1, Col 13 - Ln 1, Col 13]," +
                           "CloseParenthesis [Ln 1, Col 14 - Ln 1, Col 14]," +
                           "Operator [Ln 1, Col 16 - Ln 1, Col 16]," +
                           "Operator [Ln 1, Col 18 - Ln 1, Col 18]," +
                           "Number [Ln 1, Col 19 - Ln 1, Col 19]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBoolean() {
      var source = "True and False and True or False and True";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 40] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 40] BooleanExpression (or)\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 22] BooleanExpression (and)\n" +
                        "      [Ln 1, Col 0 - Ln 1, Col 3] BooleanConstantExpression (True)\n" +
                        "      [Ln 1, Col 9 - Ln 1, Col 13] BooleanConstantExpression (False)\n" +
                        "      [Ln 1, Col 19 - Ln 1, Col 22] BooleanConstantExpression (True)\n" +
                        "    [Ln 1, Col 27 - Ln 1, Col 40] BooleanExpression (and)\n" +
                        "      [Ln 1, Col 27 - Ln 1, Col 31] BooleanConstantExpression (False)\n" +
                        "      [Ln 1, Col 37 - Ln 1, Col 40] BooleanConstantExpression (True)";
      var expectedTokens = "Boolean [Ln 1, Col 0 - Ln 1, Col 3]," +
                           "Operator [Ln 1, Col 5 - Ln 1, Col 7]," +
                           "Boolean [Ln 1, Col 9 - Ln 1, Col 13]," +
                           "Operator [Ln 1, Col 15 - Ln 1, Col 17]," +
                           "Boolean [Ln 1, Col 19 - Ln 1, Col 22]," +
                           "Operator [Ln 1, Col 24 - Ln 1, Col 25]," +
                           "Boolean [Ln 1, Col 27 - Ln 1, Col 31]," +
                           "Operator [Ln 1, Col 33 - Ln 1, Col 35]," +
                           "Boolean [Ln 1, Col 37 - Ln 1, Col 40]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBoolFalse() {
      var source = "False";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 4] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 4] BooleanConstantExpression (False)";
      var expectedTokens = "Boolean [Ln 1, Col 0 - Ln 1, Col 4]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBoolTrue() {
      var source = "True";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 3] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 3] BooleanConstantExpression (True)";
      var expectedTokens = "Boolean [Ln 1, Col 0 - Ln 1, Col 3]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestCall() {
      var source = "add(1, 2)";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 8] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 8] CallExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 2] IdentifierExpression (add)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (2)";
      var expectedTokens = "Variable [Ln 1, Col 0 - Ln 1, Col 2]," +
                           "OpenParenthesis [Ln 1, Col 3 - Ln 1, Col 3]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                           "CloseParenthesis [Ln 1, Col 8 - Ln 1, Col 8]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestComparison() {
      var source = "1 < 2 > 3 <= 4";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 13] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 13] ComparisonExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1) (<)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2) (>)\n" +
                        "    [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (3) (<=)\n" +
                        "    [Ln 1, Col 13 - Ln 1, Col 13] NumberConstantExpression (4)";
      var expectedTokens = "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                           "Number [Ln 1, Col 8 - Ln 1, Col 8]," +
                           "Operator [Ln 1, Col 10 - Ln 1, Col 11]," +
                           "Number [Ln 1, Col 13 - Ln 1, Col 13]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestComplexBinary() {
      var source = "1 + 2 // 3 - 40 % 5 ** 2 ";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 23] ExpressionStatement\n" +
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
                        "        [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (2)";
      var expectedTokens = "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Operator [Ln 1, Col 6 - Ln 1, Col 7]," +
                           "Number [Ln 1, Col 9 - Ln 1, Col 9]," +
                           "Operator [Ln 1, Col 11 - Ln 1, Col 11]," +
                           "Number [Ln 1, Col 13 - Ln 1, Col 14]," +
                           "Operator [Ln 1, Col 16 - Ln 1, Col 16]," +
                           "Number [Ln 1, Col 18 - Ln 1, Col 18]," +
                           "Operator [Ln 1, Col 20 - Ln 1, Col 21]," +
                           "Number [Ln 1, Col 23 - Ln 1, Col 23]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestEmptyTuple() {
      var source = "()";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 1] TupleExpression";
      var expectedTokens = "OpenParenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "CloseParenthesis [Ln 1, Col 1 - Ln 1, Col 1]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestIdentifier() {
      var source = "id";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 1] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 1] IdentifierExpression (id)";
      var expectedTokens = "Variable [Ln 1, Col 0 - Ln 1, Col 1]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestIn() {
      var source = "1 in (1, 2) == (1, 2)";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 20] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 20] ComparisonExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1) (in)\n" +
                        "    [Ln 1, Col 5 - Ln 1, Col 10] TupleExpression\n" +
                        "      [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 9 - Ln 1, Col 9] NumberConstantExpression (2) (==)\n" +
                        "    [Ln 1, Col 15 - Ln 1, Col 20] TupleExpression\n" +
                        "      [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 19 - Ln 1, Col 19] NumberConstantExpression (2)";
      var expectedTokens = "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Operator [Ln 1, Col 2 - Ln 1, Col 3]," +
                           "OpenParenthesis [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "Number [Ln 1, Col 6 - Ln 1, Col 6]," +
                           "Symbol [Ln 1, Col 7 - Ln 1, Col 7]," +
                           "Number [Ln 1, Col 9 - Ln 1, Col 9]," +
                           "CloseParenthesis [Ln 1, Col 10 - Ln 1, Col 10]," +
                           "Operator [Ln 1, Col 12 - Ln 1, Col 13]," +
                           "OpenParenthesis [Ln 1, Col 15 - Ln 1, Col 15]," +
                           "Number [Ln 1, Col 16 - Ln 1, Col 16]," +
                           "Symbol [Ln 1, Col 17 - Ln 1, Col 17]," +
                           "Number [Ln 1, Col 19 - Ln 1, Col 19]," +
                           "CloseParenthesis [Ln 1, Col 20 - Ln 1, Col 20]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestList() {
      var source = "[1, 2, 3]";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 8] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "    [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "    [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)";
      var expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                           "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestMultipleBinary() {
      var source = "1 + 2 * 3 - 40";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 13] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 13] BinaryExpression (-)\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] BinaryExpression (+)\n" +
                        "      [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 8] BinaryExpression (*)\n" +
                        "        [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "        [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 12 - Ln 1, Col 13] NumberConstantExpression (40)";
      var expectedTokens = "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Operator [Ln 1, Col 6 - Ln 1, Col 6]," +
                           "Number [Ln 1, Col 8 - Ln 1, Col 8]," +
                           "Operator [Ln 1, Col 10 - Ln 1, Col 10]," +
                           "Number [Ln 1, Col 12 - Ln 1, Col 13]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestNil() {
      var source = "None";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 3] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 3] NilConstantExpression";
      var expectedTokens = "Nil [Ln 1, Col 0 - Ln 1, Col 3]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestSubscript() {
      var source = "[1, 2, 3][1]";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 11] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 11] SubscriptExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 8] ListExpression\n" +
                        "      [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "      [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "      [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)\n" +
                        "    [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (1)";
      var expectedTokens = "OpenBracket [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                           "CloseBracket [Ln 1, Col 8 - Ln 1, Col 8]," +
                           "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]," +
                           "Number [Ln 1, Col 10 - Ln 1, Col 10]," +
                           "CloseBracket [Ln 1, Col 11 - Ln 1, Col 11]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestTuple() {
      var source = "(1, 2, 3)";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 8] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 8] TupleExpression\n" +
                        "    [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (2)\n" +
                        "    [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (3)";
      var expectedTokens = "OpenParenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "Number [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Symbol [Ln 1, Col 5 - Ln 1, Col 5]," +
                           "Number [Ln 1, Col 7 - Ln 1, Col 7]," +
                           "CloseParenthesis [Ln 1, Col 8 - Ln 1, Col 8]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestTupleWithOneItem() {
      var source = "(1,)";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 3] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 3] TupleExpression\n" +
                        "    [Ln 1, Col 1 - Ln 1, Col 1] NumberConstantExpression (1)";
      var expectedTokens = "OpenParenthesis [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Number [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Symbol [Ln 1, Col 2 - Ln 1, Col 2]," +
                           "CloseParenthesis [Ln 1, Col 3 - Ln 1, Col 3]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestTupleWithoutParentheses() {
      var source = "1, 2, 3";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 6] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 6] TupleExpression\n" +
                        "    [Ln 1, Col 0 - Ln 1, Col 0] NumberConstantExpression (1)\n" +
                        "    [Ln 1, Col 3 - Ln 1, Col 3] NumberConstantExpression (2)\n" +
                        "    [Ln 1, Col 6 - Ln 1, Col 6] NumberConstantExpression (3)";
      var expectedTokens = "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                           "Symbol [Ln 1, Col 1 - Ln 1, Col 1]," +
                           "Number [Ln 1, Col 3 - Ln 1, Col 3]," +
                           "Symbol [Ln 1, Col 4 - Ln 1, Col 4]," +
                           "Number [Ln 1, Col 6 - Ln 1, Col 6]";
      TestParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestUnary() {
      var source = "not True";
      var expectedAst = "[Ln 1, Col 0 - Ln 1, Col 7] ExpressionStatement\n" +
                        "  [Ln 1, Col 0 - Ln 1, Col 7] UnaryExpression (not)\n" +
                        "    [Ln 1, Col 4 - Ln 1, Col 7] BooleanConstantExpression (True)";
      var expectedTokens = "Operator [Ln 1, Col 0 - Ln 1, Col 2]," +
                           "Boolean [Ln 1, Col 4 - Ln 1, Col 7]";
      TestParser(source, expectedAst, expectedTokens);
    }

    private static void TestParser(string source, string expectedAst, string expectedTokens) {
      var collection = new DiagnosticCollection();
      new SeedPython().Parse(source, "", collection, out Statement statement,
                             out IReadOnlyList<TokenInfo> tokens).Should().Be(true);
      statement.Should().NotBeNull();
      collection.Diagnostics.Should().BeEmpty();
      Assert.Equal(expectedAst.Replace("\n", Environment.NewLine), statement.ToString());
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
