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

using System;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    private static TextRange _testTextRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestCompileNumberConstant() {
      var number = Expression.NumberConstant(1, _testTextRange);
      var expr = Statement.Expression(number, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          "1    LOADK     0 -1           ; 1                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    EVAL      0                                  [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileBinary() {
      var left = Expression.NumberConstant(1, _testTextRange);
      var right = Expression.NumberConstant(2, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _testTextRange);
      var expr = Statement.Expression(binary, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          "1    ADD       0 -1 -2        ; 1 2               [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    EVAL      0                                  [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileComplexBinary() {
      var left = Expression.NumberConstant(1, _testTextRange);
      var number2 = Expression.NumberConstant(2, _testTextRange);
      var number3 = Expression.NumberConstant(3, _testTextRange);
      var right = Expression.Binary(number2, BinaryOperator.Add, number3, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Subtract, right, _testTextRange);
      var expr = Statement.Expression(binary, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          "1    ADD       1 -2 -3        ; 2 3               [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    SUB       0 -1 1         ; 1                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    EVAL      0                                  [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "4    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileBinaryWithSameConstants() {
      var left = Expression.NumberConstant(1, _testTextRange);
      var number1 = Expression.NumberConstant(1, _testTextRange);
      var number2 = Expression.NumberConstant(2, _testTextRange);
      var right = Expression.Binary(number1, BinaryOperator.Add, number2, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Subtract, right, _testTextRange);
      var expr = Statement.Expression(binary, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          "1    ADD       1 -1 -2        ; 1 2               [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    SUB       0 -1 1         ; 1                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    EVAL      0                                  [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "4    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileUnary() {
      var number = Expression.NumberConstant(1, _testTextRange);
      var unary = Expression.Unary(UnaryOperator.Negative, number, _testTextRange);
      var expr = Statement.Expression(unary, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          "1    UNM       0 -1           ; 1                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    EVAL      0                                  [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileAssignNumberConstant() {
      var identifier = Expression.Identifier("name", _testTextRange);
      var number = Expression.NumberConstant(1, _testTextRange);
      var assignment = Statement.Assignment(identifier, number, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(assignment);
      string expected = (
          "1    LOADK     0 -1           ; 1                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    SETGLOB   0 -2           ; name              [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileAssignBinary() {
      var identifier = Expression.Identifier("name", _testTextRange);
      var left = Expression.NumberConstant(1, _testTextRange);
      var right = Expression.NumberConstant(2, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _testTextRange);
      var assignment = Statement.Assignment(identifier, binary, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(assignment);
      string expected = (
          "1    ADD       0 -1 -2        ; 1 2               [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    SETGLOB   0 -3           ; name              [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }
  }
}
