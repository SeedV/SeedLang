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
using System.Linq;
using FluentAssertions;
using SeedLang.Ast;
using SeedLang.Common;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonStatementTests {
    [Fact]
    public void TestOneLineComments() {
      string source = "# comment";
      var expectedAst = new string[] { "[Ln 1, Col 9 - Ln 1, Col 9] BlockStatement" };
      var expectedTokens = Array.Empty<string>();
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestComments() {
      string source = "# comment\n" +
                      "print(1)";
      var expectedAst = new string[] {
        "[Ln 2, Col 0 - Ln 2, Col 7] ExpressionStatement",
        "  [Ln 2, Col 0 - Ln 2, Col 7] CallExpression",
        "    [Ln 2, Col 0 - Ln 2, Col 4] IdentifierExpression (print)",
        "    [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 2, Col 0 - Ln 2, Col 4]",
        "OpenParenthesis [Ln 2, Col 5 - Ln 2, Col 5]",
        "Number [Ln 2, Col 6 - Ln 2, Col 6]",
        "CloseParenthesis [Ln 2, Col 7 - Ln 2, Col 7]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestAssignment() {
      string source = "x = 1";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 1, Col 4] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (x) =",
        "  [Ln 1, Col 4 - Ln 1, Col 4] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 0]",
        "Operator [Ln 1, Col 2 - Ln 1, Col 2]",
        "Number [Ln 1, Col 4 - Ln 1, Col 4]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestMultipleAssignment() {
      string source = "x, y = 1, 2";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 1, Col 10] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (x)",
        "  [Ln 1, Col 3 - Ln 1, Col 3] IdentifierExpression (y) =",
        "  [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (1)",
        "  [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (2)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 0]",
        "Symbol [Ln 1, Col 1 - Ln 1, Col 1]",
        "Variable [Ln 1, Col 3 - Ln 1, Col 3]",
        "Operator [Ln 1, Col 5 - Ln 1, Col 5]",
        "Number [Ln 1, Col 7 - Ln 1, Col 7]",
        "Symbol [Ln 1, Col 8 - Ln 1, Col 8]",
        "Number [Ln 1, Col 10 - Ln 1, Col 10]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestChainedAssignment() {
      string source = "x = y = 1";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 1, Col 8] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (x) =",
        "  [Ln 1, Col 4 - Ln 1, Col 4] IdentifierExpression (y) =",
        "  [Ln 1, Col 8 - Ln 1, Col 8] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 0]",
        "Operator [Ln 1, Col 2 - Ln 1, Col 2]",
        "Variable [Ln 1, Col 4 - Ln 1, Col 4]",
        "Operator [Ln 1, Col 6 - Ln 1, Col 6]",
        "Number [Ln 1, Col 8 - Ln 1, Col 8]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestChainedMultipleAssignment() {
      string source = "a, b = x, y = 1, 2";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 1, Col 17] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (a)",
        "  [Ln 1, Col 3 - Ln 1, Col 3] IdentifierExpression (b) =",
        "  [Ln 1, Col 7 - Ln 1, Col 7] IdentifierExpression (x)",
        "  [Ln 1, Col 10 - Ln 1, Col 10] IdentifierExpression (y) =",
        "  [Ln 1, Col 14 - Ln 1, Col 14] NumberConstantExpression (1)",
        "  [Ln 1, Col 17 - Ln 1, Col 17] NumberConstantExpression (2)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 0]",
        "Symbol [Ln 1, Col 1 - Ln 1, Col 1]",
        "Variable [Ln 1, Col 3 - Ln 1, Col 3]",
        "Operator [Ln 1, Col 5 - Ln 1, Col 5]",
        "Variable [Ln 1, Col 7 - Ln 1, Col 7]",
        "Symbol [Ln 1, Col 8 - Ln 1, Col 8]",
        "Variable [Ln 1, Col 10 - Ln 1, Col 10]",
        "Operator [Ln 1, Col 12 - Ln 1, Col 12]",
        "Number [Ln 1, Col 14 - Ln 1, Col 14]",
        "Symbol [Ln 1, Col 15 - Ln 1, Col 15]",
        "Number [Ln 1, Col 17 - Ln 1, Col 17]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestAugmentedAssignment() {
      string source = "x += 1";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 5] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (x) =",
        "  [Ln 1, Col 0 - Ln 1, Col 5] BinaryExpression (+)",
        "    [Ln 1, Col 0 - Ln 1, Col 0] IdentifierExpression (x)",
        "    [Ln 1, Col 5 - Ln 1, Col 5] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 0]",
        "Operator [Ln 1, Col 2 - Ln 1, Col 3]",
        "Number [Ln 1, Col 5 - Ln 1, Col 5]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestStringAssignment() {
      string source = "str = 'test string'";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 18] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 2] IdentifierExpression (str) =",
        "  [Ln 1, Col 6 - Ln 1, Col 18] StringConstantExpression (test string)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 2]",
        "Operator [Ln 1, Col 4 - Ln 1, Col 4]",
        "String [Ln 1, Col 6 - Ln 1, Col 18]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestMultipleStringAssignment() {
      string source = "str = 'a' \"b\" 'c'";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 16] AssignmentStatement",
        "  [Ln 1, Col 0 - Ln 1, Col 2] IdentifierExpression (str) =",
        "  [Ln 1, Col 6 - Ln 1, Col 16] StringConstantExpression (abc)",
      };
      var expectedTokens = new string[] {
        "Variable [Ln 1, Col 0 - Ln 1, Col 2]",
        "Operator [Ln 1, Col 4 - Ln 1, Col 4]",
        "String [Ln 1, Col 6 - Ln 1, Col 8]",
        "String [Ln 1, Col 10 - Ln 1, Col 12]",
        "String [Ln 1, Col 14 - Ln 1, Col 16]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestIf() {
      string source = "if 1 < 2: x = 1";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 14] IfStatement",
        "  [Ln 1, Col 3 - Ln 1, Col 7] ComparisonExpression",
        "    [Ln 1, Col 3 - Ln 1, Col 3] NumberConstantExpression (1) (<)",
        "    [Ln 1, Col 7 - Ln 1, Col 7] NumberConstantExpression (2)",
        "  [Ln 1, Col 10 - Ln 1, Col 14] AssignmentStatement",
        "    [Ln 1, Col 10 - Ln 1, Col 10] IdentifierExpression (x) =",
        "    [Ln 1, Col 14 - Ln 1, Col 14] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 1]",
        "Number [Ln 1, Col 3 - Ln 1, Col 3]",
        "Operator [Ln 1, Col 5 - Ln 1, Col 5]",
        "Number [Ln 1, Col 7 - Ln 1, Col 7]",
        "Symbol [Ln 1, Col 8 - Ln 1, Col 8]",
        "Variable [Ln 1, Col 10 - Ln 1, Col 10]",
        "Operator [Ln 1, Col 12 - Ln 1, Col 12]",
        "Number [Ln 1, Col 14 - Ln 1, Col 14]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestIfElse() {
      string source = "if True:\n" +
                      "  x = 1\n" +
                      "else:\n" +
                      "  x = 2";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 4, Col 6] IfStatement",
        "  [Ln 1, Col 3 - Ln 1, Col 6] BooleanConstantExpression (True)",
        "  [Ln 2, Col 2 - Ln 2, Col 6] AssignmentStatement",
        "    [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (x) =",
        "    [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)",
        "  [Ln 4, Col 2 - Ln 4, Col 6] AssignmentStatement",
        "    [Ln 4, Col 2 - Ln 4, Col 2] IdentifierExpression (x) =",
        "    [Ln 4, Col 6 - Ln 4, Col 6] NumberConstantExpression (2)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 1]",
        "Boolean [Ln 1, Col 3 - Ln 1, Col 6]",
        "Symbol [Ln 1, Col 7 - Ln 1, Col 7]",
        "Variable [Ln 2, Col 2 - Ln 2, Col 2]",
        "Operator [Ln 2, Col 4 - Ln 2, Col 4]",
        "Number [Ln 2, Col 6 - Ln 2, Col 6]",
        "Keyword [Ln 3, Col 0 - Ln 3, Col 3]",
        "Symbol [Ln 3, Col 4 - Ln 3, Col 4]",
        "Variable [Ln 4, Col 2 - Ln 4, Col 2]",
        "Operator [Ln 4, Col 4 - Ln 4, Col 4]",
        "Number [Ln 4, Col 6 - Ln 4, Col 6]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestIfElseWithPassStatement() {
      string source = "if True:\n" +
                      "  pass\n" +
                      "else:\n" +
                      "  pass\n";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 4, Col 5] IfStatement",
        "  [Ln 1, Col 3 - Ln 1, Col 6] BooleanConstantExpression (True)",
        "  [Ln 2, Col 2 - Ln 2, Col 5] PassStatement",
        "  [Ln 4, Col 2 - Ln 4, Col 5] PassStatement",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 1]",
        "Boolean [Ln 1, Col 3 - Ln 1, Col 6]",
        "Symbol [Ln 1, Col 7 - Ln 1, Col 7]",
        "Keyword [Ln 2, Col 2 - Ln 2, Col 5]",
        "Keyword [Ln 3, Col 0 - Ln 3, Col 3]",
        "Symbol [Ln 3, Col 4 - Ln 3, Col 4]",
        "Keyword [Ln 4, Col 2 - Ln 4, Col 5]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestIfElif() {
      string source = "if True:\n" +
                      "  x = 1\n" +
                      "elif False:\n" +
                      "  x = 2\n" +
                      "elif 1 < 2:\n" +
                      "  x = 3\n" +
                      "else:\n" +
                      "  x = 4";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 8, Col 6] IfStatement",
        "  [Ln 1, Col 3 - Ln 1, Col 6] BooleanConstantExpression (True)",
        "  [Ln 2, Col 2 - Ln 2, Col 6] AssignmentStatement",
        "    [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (x) =",
        "    [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)",
        "  [Ln 3, Col 0 - Ln 8, Col 6] IfStatement",
        "    [Ln 3, Col 5 - Ln 3, Col 9] BooleanConstantExpression (False)",
        "    [Ln 4, Col 2 - Ln 4, Col 6] AssignmentStatement",
        "      [Ln 4, Col 2 - Ln 4, Col 2] IdentifierExpression (x) =",
        "      [Ln 4, Col 6 - Ln 4, Col 6] NumberConstantExpression (2)",
        "    [Ln 5, Col 0 - Ln 8, Col 6] IfStatement",
        "      [Ln 5, Col 5 - Ln 5, Col 9] ComparisonExpression",
        "        [Ln 5, Col 5 - Ln 5, Col 5] NumberConstantExpression (1) (<)",
        "        [Ln 5, Col 9 - Ln 5, Col 9] NumberConstantExpression (2)",
        "      [Ln 6, Col 2 - Ln 6, Col 6] AssignmentStatement",
        "        [Ln 6, Col 2 - Ln 6, Col 2] IdentifierExpression (x) =",
        "        [Ln 6, Col 6 - Ln 6, Col 6] NumberConstantExpression (3)",
        "      [Ln 8, Col 2 - Ln 8, Col 6] AssignmentStatement",
        "        [Ln 8, Col 2 - Ln 8, Col 2] IdentifierExpression (x) =",
        "        [Ln 8, Col 6 - Ln 8, Col 6] NumberConstantExpression (4)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 1]",
        "Boolean [Ln 1, Col 3 - Ln 1, Col 6]",
        "Symbol [Ln 1, Col 7 - Ln 1, Col 7]",
        "Variable [Ln 2, Col 2 - Ln 2, Col 2]",
        "Operator [Ln 2, Col 4 - Ln 2, Col 4]",
        "Number [Ln 2, Col 6 - Ln 2, Col 6]",
        "Keyword [Ln 3, Col 0 - Ln 3, Col 3]",
        "Boolean [Ln 3, Col 5 - Ln 3, Col 9]",
        "Symbol [Ln 3, Col 10 - Ln 3, Col 10]",
        "Variable [Ln 4, Col 2 - Ln 4, Col 2]",
        "Operator [Ln 4, Col 4 - Ln 4, Col 4]",
        "Number [Ln 4, Col 6 - Ln 4, Col 6]",
        "Keyword [Ln 5, Col 0 - Ln 5, Col 3]",
        "Number [Ln 5, Col 5 - Ln 5, Col 5]",
        "Operator [Ln 5, Col 7 - Ln 5, Col 7]",
        "Number [Ln 5, Col 9 - Ln 5, Col 9]",
        "Symbol [Ln 5, Col 10 - Ln 5, Col 10]",
        "Variable [Ln 6, Col 2 - Ln 6, Col 2]",
        "Operator [Ln 6, Col 4 - Ln 6, Col 4]",
        "Number [Ln 6, Col 6 - Ln 6, Col 6]",
        "Keyword [Ln 7, Col 0 - Ln 7, Col 3]",
        "Symbol [Ln 7, Col 4 - Ln 7, Col 4]",
        "Variable [Ln 8, Col 2 - Ln 8, Col 2]",
        "Operator [Ln 8, Col 4 - Ln 8, Col 4]",
        "Number [Ln 8, Col 6 - Ln 8, Col 6]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestForInList() {
      string source = "for n in [1, 2, 3]: n";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 20] ForInStatement",
        "  [Ln 1, Col 4 - Ln 1, Col 4] IdentifierExpression (n)",
        "  [Ln 1, Col 9 - Ln 1, Col 17] ListExpression",
        "    [Ln 1, Col 10 - Ln 1, Col 10] NumberConstantExpression (1)",
        "    [Ln 1, Col 13 - Ln 1, Col 13] NumberConstantExpression (2)",
        "    [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (3)",
        "  [Ln 1, Col 20 - Ln 1, Col 20] ExpressionStatement",
        "    [Ln 1, Col 20 - Ln 1, Col 20] IdentifierExpression (n)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Variable [Ln 1, Col 4 - Ln 1, Col 4]",
        "Keyword [Ln 1, Col 6 - Ln 1, Col 7]",
        "OpenBracket [Ln 1, Col 9 - Ln 1, Col 9]",
        "Number [Ln 1, Col 10 - Ln 1, Col 10]",
        "Symbol [Ln 1, Col 11 - Ln 1, Col 11]",
        "Number [Ln 1, Col 13 - Ln 1, Col 13]",
        "Symbol [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 16 - Ln 1, Col 16]",
        "CloseBracket [Ln 1, Col 17 - Ln 1, Col 17]",
        "Symbol [Ln 1, Col 18 - Ln 1, Col 18]",
        "Variable [Ln 1, Col 20 - Ln 1, Col 20]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestForInRange() {
      string source = "for n in range(2, 10, 3): n";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 26] ForInStatement",
        "  [Ln 1, Col 4 - Ln 1, Col 4] IdentifierExpression (n)",
        "  [Ln 1, Col 9 - Ln 1, Col 23] CallExpression",
        "    [Ln 1, Col 9 - Ln 1, Col 13] IdentifierExpression (range)",
        "    [Ln 1, Col 15 - Ln 1, Col 15] NumberConstantExpression (2)",
        "    [Ln 1, Col 18 - Ln 1, Col 19] NumberConstantExpression (10)",
        "    [Ln 1, Col 22 - Ln 1, Col 22] NumberConstantExpression (3)",
        "  [Ln 1, Col 26 - Ln 1, Col 26] ExpressionStatement",
        "    [Ln 1, Col 26 - Ln 1, Col 26] IdentifierExpression (n)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Variable [Ln 1, Col 4 - Ln 1, Col 4]",
        "Keyword [Ln 1, Col 6 - Ln 1, Col 7]",
        "Variable [Ln 1, Col 9 - Ln 1, Col 13]",
        "OpenParenthesis [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 15 - Ln 1, Col 15]",
        "Symbol [Ln 1, Col 16 - Ln 1, Col 16]",
        "Number [Ln 1, Col 18 - Ln 1, Col 19]",
        "Symbol [Ln 1, Col 20 - Ln 1, Col 20]",
        "Number [Ln 1, Col 22 - Ln 1, Col 22]",
        "CloseParenthesis [Ln 1, Col 23 - Ln 1, Col 23]",
        "Symbol [Ln 1, Col 24 - Ln 1, Col 24]",
        "Variable [Ln 1, Col 26 - Ln 1, Col 26]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestWhile() {
      string source = "while True: x = 1";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 16] WhileStatement",
        "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)",
        "  [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement",
        "    [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x) =",
        "    [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 4]",
        "Boolean [Ln 1, Col 6 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Variable [Ln 1, Col 12 - Ln 1, Col 12]",
        "Operator [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 16 - Ln 1, Col 16]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestWhileWithBody() {
      string source = "while True:\n" +
                      "  x = 1\n" +
                      "  y = 2";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 3, Col 6] WhileStatement",
        "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)",
        "  [Ln 2, Col 2 - Ln 3, Col 6] BlockStatement",
        "    [Ln 2, Col 2 - Ln 2, Col 6] AssignmentStatement",
        "      [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (x) =",
        "      [Ln 2, Col 6 - Ln 2, Col 6] NumberConstantExpression (1)",
        "    [Ln 3, Col 2 - Ln 3, Col 6] AssignmentStatement",
        "      [Ln 3, Col 2 - Ln 3, Col 2] IdentifierExpression (y) =",
        "      [Ln 3, Col 6 - Ln 3, Col 6] NumberConstantExpression (2)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 4]",
        "Boolean [Ln 1, Col 6 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Variable [Ln 2, Col 2 - Ln 2, Col 2]",
        "Operator [Ln 2, Col 4 - Ln 2, Col 4]",
        "Number [Ln 2, Col 6 - Ln 2, Col 6]",
        "Variable [Ln 3, Col 2 - Ln 3, Col 2]",
        "Operator [Ln 3, Col 4 - Ln 3, Col 4]",
        "Number [Ln 3, Col 6 - Ln 3, Col 6]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestWhileWithSimpleStatementsBody() {
      string source = "while True: x = 1; y = 2";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 23] WhileStatement",
        "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)",
        "  [Ln 1, Col 12 - Ln 1, Col 23] BlockStatement",
        "    [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement",
        "      [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x) =",
        "      [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)",
        "    [Ln 1, Col 19 - Ln 1, Col 23] AssignmentStatement",
        "      [Ln 1, Col 19 - Ln 1, Col 19] IdentifierExpression (y) =",
        "      [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (2)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 4]",
        "Boolean [Ln 1, Col 6 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Variable [Ln 1, Col 12 - Ln 1, Col 12]",
        "Operator [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 16 - Ln 1, Col 16]",
        "Symbol [Ln 1, Col 17 - Ln 1, Col 17]",
        "Variable [Ln 1, Col 19 - Ln 1, Col 19]",
        "Operator [Ln 1, Col 21 - Ln 1, Col 21]",
        "Number [Ln 1, Col 23 - Ln 1, Col 23]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestWhileWithSimpleStatementsAndFinalCommaBody() {
      string source = "while True: x = 1; y = 2;";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 23] WhileStatement",
        "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)",
        "  [Ln 1, Col 12 - Ln 1, Col 23] BlockStatement",
        "    [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement",
        "      [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x) =",
        "      [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)",
        "    [Ln 1, Col 19 - Ln 1, Col 23] AssignmentStatement",
        "      [Ln 1, Col 19 - Ln 1, Col 19] IdentifierExpression (y) =",
        "      [Ln 1, Col 23 - Ln 1, Col 23] NumberConstantExpression (2)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 4]",
        "Boolean [Ln 1, Col 6 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Variable [Ln 1, Col 12 - Ln 1, Col 12]",
        "Operator [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 16 - Ln 1, Col 16]",
        "Symbol [Ln 1, Col 17 - Ln 1, Col 17]",
        "Variable [Ln 1, Col 19 - Ln 1, Col 19]",
        "Operator [Ln 1, Col 21 - Ln 1, Col 21]",
        "Number [Ln 1, Col 23 - Ln 1, Col 23]",
        "Symbol [Ln 1, Col 24 - Ln 1, Col 24]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestFuncWithoutReturn() {
      string source = "def func(): x = 1";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 16] FuncDefStatement (func)",
        "  [Ln 1, Col 12 - Ln 1, Col 16] AssignmentStatement",
        "    [Ln 1, Col 12 - Ln 1, Col 12] IdentifierExpression (x) =",
        "    [Ln 1, Col 16 - Ln 1, Col 16] NumberConstantExpression (1)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Function [Ln 1, Col 4 - Ln 1, Col 7]",
        "OpenParenthesis [Ln 1, Col 8 - Ln 1, Col 8]",
        "CloseParenthesis [Ln 1, Col 9 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Variable [Ln 1, Col 12 - Ln 1, Col 12]",
        "Operator [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 16 - Ln 1, Col 16]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestEmptyReturn() {
      string source = "def func(): return";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 1, Col 17] FuncDefStatement (func)",
        "  [Ln 1, Col 12 - Ln 1, Col 17] ReturnStatement",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Function [Ln 1, Col 4 - Ln 1, Col 7]",
        "OpenParenthesis [Ln 1, Col 8 - Ln 1, Col 8]",
        "CloseParenthesis [Ln 1, Col 9 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Keyword [Ln 1, Col 12 - Ln 1, Col 17]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestMultipleReturn() {
      string source = "def func():\n" +
                      "  return 1, 2, 3";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 2, Col 15] FuncDefStatement (func)",
        "  [Ln 2, Col 2 - Ln 2, Col 15] ReturnStatement",
        "    [Ln 2, Col 9 - Ln 2, Col 9] NumberConstantExpression (1)",
        "    [Ln 2, Col 12 - Ln 2, Col 12] NumberConstantExpression (2)",
        "    [Ln 2, Col 15 - Ln 2, Col 15] NumberConstantExpression (3)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Function [Ln 1, Col 4 - Ln 1, Col 7]",
        "OpenParenthesis [Ln 1, Col 8 - Ln 1, Col 8]",
        "CloseParenthesis [Ln 1, Col 9 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Keyword [Ln 2, Col 2 - Ln 2, Col 7]",
        "Number [Ln 2, Col 9 - Ln 2, Col 9]",
        "Symbol [Ln 2, Col 10 - Ln 2, Col 10]",
        "Number [Ln 2, Col 12 - Ln 2, Col 12]",
        "Symbol [Ln 2, Col 13 - Ln 2, Col 13]",
        "Number [Ln 2, Col 15 - Ln 2, Col 15]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestReturnVariable() {
      string source = "def add(a, b):\n" +
                      "  c = a + b\n" +
                      "  return c";
      var expectedAst = new string[] { "[Ln 1, Col 0 - Ln 3, Col 9] FuncDefStatement (add:a,b)",
        "  [Ln 2, Col 2 - Ln 3, Col 9] BlockStatement",
        "    [Ln 2, Col 2 - Ln 2, Col 10] AssignmentStatement",
        "      [Ln 2, Col 2 - Ln 2, Col 2] IdentifierExpression (c) =",
        "      [Ln 2, Col 6 - Ln 2, Col 10] BinaryExpression (+)",
        "        [Ln 2, Col 6 - Ln 2, Col 6] IdentifierExpression (a)",
        "        [Ln 2, Col 10 - Ln 2, Col 10] IdentifierExpression (b)",
        "    [Ln 3, Col 2 - Ln 3, Col 9] ReturnStatement",
        "      [Ln 3, Col 9 - Ln 3, Col 9] IdentifierExpression (c)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Function [Ln 1, Col 4 - Ln 1, Col 6]",
        "OpenParenthesis [Ln 1, Col 7 - Ln 1, Col 7]",
        "Parameter [Ln 1, Col 8 - Ln 1, Col 8]",
        "Symbol [Ln 1, Col 9 - Ln 1, Col 9]",
        "Parameter [Ln 1, Col 11 - Ln 1, Col 11]",
        "CloseParenthesis [Ln 1, Col 12 - Ln 1, Col 12]",
        "Symbol [Ln 1, Col 13 - Ln 1, Col 13]",
        "Variable [Ln 2, Col 2 - Ln 2, Col 2]",
        "Operator [Ln 2, Col 4 - Ln 2, Col 4]",
        "Variable [Ln 2, Col 6 - Ln 2, Col 6]",
        "Operator [Ln 2, Col 8 - Ln 2, Col 8]",
        "Variable [Ln 2, Col 10 - Ln 2, Col 10]",
        "Keyword [Ln 3, Col 2 - Ln 3, Col 7]",
        "Variable [Ln 3, Col 9 - Ln 3, Col 9]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestReturnBinary() {
      string source = "def add(a, b):\n" +
                      "  return a + b";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 2, Col 13] FuncDefStatement (add:a,b)",
        "  [Ln 2, Col 2 - Ln 2, Col 13] ReturnStatement",
        "    [Ln 2, Col 9 - Ln 2, Col 13] BinaryExpression (+)",
        "      [Ln 2, Col 9 - Ln 2, Col 9] IdentifierExpression (a)",
        "      [Ln 2, Col 13 - Ln 2, Col 13] IdentifierExpression (b)",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Function [Ln 1, Col 4 - Ln 1, Col 6]",
        "OpenParenthesis [Ln 1, Col 7 - Ln 1, Col 7]",
        "Parameter [Ln 1, Col 8 - Ln 1, Col 8]",
        "Symbol [Ln 1, Col 9 - Ln 1, Col 9]",
        "Parameter [Ln 1, Col 11 - Ln 1, Col 11]",
        "CloseParenthesis [Ln 1, Col 12 - Ln 1, Col 12]",
        "Symbol [Ln 1, Col 13 - Ln 1, Col 13]",
        "Keyword [Ln 2, Col 2 - Ln 2, Col 7]",
        "Variable [Ln 2, Col 9 - Ln 2, Col 9]",
        "Operator [Ln 2, Col 11 - Ln 2, Col 11]",
        "Variable [Ln 2, Col 13 - Ln 2, Col 13]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestBreak() {
      string source = "while True:\n" +
                      "  break";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 2, Col 6] WhileStatement",
        "  [Ln 1, Col 6 - Ln 1, Col 9] BooleanConstantExpression (True)",
        "  [Ln 2, Col 2 - Ln 2, Col 6] BreakStatement",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 4]",
        "Boolean [Ln 1, Col 6 - Ln 1, Col 9]",
        "Symbol [Ln 1, Col 10 - Ln 1, Col 10]",
        "Keyword [Ln 2, Col 2 - Ln 2, Col 6]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    [Fact]
    public void TestContinue() {
      string source = "for i in range(10):\n" +
                      "  continue";
      var expectedAst = new string[] {
        "[Ln 1, Col 0 - Ln 2, Col 9] ForInStatement",
        "  [Ln 1, Col 4 - Ln 1, Col 4] IdentifierExpression (i)",
        "  [Ln 1, Col 9 - Ln 1, Col 17] CallExpression",
        "    [Ln 1, Col 9 - Ln 1, Col 13] IdentifierExpression (range)",
        "    [Ln 1, Col 15 - Ln 1, Col 16] NumberConstantExpression (10)",
        "  [Ln 2, Col 2 - Ln 2, Col 9] ContinueStatement",
      };
      var expectedTokens = new string[] {
        "Keyword [Ln 1, Col 0 - Ln 1, Col 2]",
        "Variable [Ln 1, Col 4 - Ln 1, Col 4]",
        "Keyword [Ln 1, Col 6 - Ln 1, Col 7]",
        "Variable [Ln 1, Col 9 - Ln 1, Col 13]",
        "OpenParenthesis [Ln 1, Col 14 - Ln 1, Col 14]",
        "Number [Ln 1, Col 15 - Ln 1, Col 16]",
        "CloseParenthesis [Ln 1, Col 17 - Ln 1, Col 17]",
        "Symbol [Ln 1, Col 18 - Ln 1, Col 18]",
        "Keyword [Ln 2, Col 2 - Ln 2, Col 9]",
      };
      TestPythonParser(source, expectedAst, expectedTokens);
    }

    private static void TestPythonParser(string source,
                                         string[] expectedAst,
                                         string[] expectedTokens) {
      var collection = new DiagnosticCollection();
      new SeedPython().Parse(source, "", collection, out Statement statement,
                             out IReadOnlyList<TokenInfo> tokens).Should().Be(true);
      statement.Should().NotBe(null);
      collection.Diagnostics.Should().BeEmpty();
      statement.ToString().Split(Environment.NewLine).Should().BeEquivalentTo(expectedAst);
      tokens.Select(token => token.ToString()).Should().BeEquivalentTo(expectedTokens);
    }
  }
}
