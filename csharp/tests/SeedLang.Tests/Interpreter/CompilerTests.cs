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

using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    private static TextRange _textRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestCompileNumberConstant() {
      var number = Expression.NumberConstant(1, _textRange);
      var expr = Statement.Expression(number, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          $"1    LOADK     0 -1           ; 1                 {_textRange}\n" +
          $"2    EVAL      0                                  {_textRange}\n" +
          $"3    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileBinary() {
      var left = Expression.NumberConstant(1, _textRange);
      var right = Expression.NumberConstant(2, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
      var expr = Statement.Expression(binary, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          $"1    ADD       0 -1 -2        ; 1 2               {_textRange}\n" +
          $"2    EVAL      0                                  {_textRange}\n" +
          $"3    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileComplexBinary() {
      var left = Expression.NumberConstant(1, _textRange);
      var number2 = Expression.NumberConstant(2, _textRange);
      var number3 = Expression.NumberConstant(3, _textRange);
      var right = Expression.Binary(number2, BinaryOperator.Add, number3, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Subtract, right, _textRange);
      var expr = Statement.Expression(binary, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          $"1    ADD       1 -2 -3        ; 2 3               {_textRange}\n" +
          $"2    SUB       0 -1 1         ; 1                 {_textRange}\n" +
          $"3    EVAL      0                                  {_textRange}\n" +
          $"4    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileBinaryWithSameConstants() {
      var left = Expression.NumberConstant(1, _textRange);
      var number1 = Expression.NumberConstant(1, _textRange);
      var number2 = Expression.NumberConstant(2, _textRange);
      var right = Expression.Binary(number1, BinaryOperator.Add, number2, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Subtract, right, _textRange);
      var expr = Statement.Expression(binary, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          $"1    ADD       1 -1 -2        ; 1 2               {_textRange}\n" +
          $"2    SUB       0 -1 1         ; 1                 {_textRange}\n" +
          $"3    EVAL      0                                  {_textRange}\n" +
          $"4    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileUnary() {
      var number = Expression.NumberConstant(1, _textRange);
      var unary = Expression.Unary(UnaryOperator.Negative, number, _textRange);
      var expr = Statement.Expression(unary, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(expr);
      string expected = (
          $"1    UNM       0 -1           ; 1                 {_textRange}\n" +
          $"2    EVAL      0                                  {_textRange}\n" +
          $"3    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileAssignNumberConstant() {
      var identifier = Expression.Identifier("name", _textRange);
      var number = Expression.NumberConstant(1, _textRange);
      var assignment = Statement.Assignment(identifier, number, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(assignment);
      string expected = (
          $"1    MOVE      0 -1           ; 1                 {_textRange}\n" +
          $"2    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileAssignBinary() {
      var identifier = Expression.Identifier("name", _textRange);
      var left = Expression.NumberConstant(1, _textRange);
      var right = Expression.NumberConstant(2, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
      var assignment = Statement.Assignment(identifier, binary, _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(assignment);
      string expected = (
          $"1    ADD       0 -1 -2        ; 1 2               {_textRange}\n" +
          $"2    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }

    [Fact]
    public void TestCompileWhile() {
      var sum = Expression.Identifier("sum", _textRange);
      var i = Expression.Identifier("i", _textRange);
      var zero = Expression.NumberConstant(0, _textRange);
      var one = Expression.NumberConstant(1, _textRange);
      var initialSum = Statement.Assignment(sum, zero, _textRange);
      var initialI = Statement.Assignment(i, zero, _textRange);
      var ops = new ComparisonOperator[] { ComparisonOperator.LessEqual };
      var exprs = new Expression[] { Expression.NumberConstant(10, _textRange) };
      var test = Expression.Comparison(i, ops, exprs, _textRange);
      var addSum = Expression.Binary(sum, BinaryOperator.Add, i, _textRange);
      var assignSum = Statement.Assignment(sum, addSum, _textRange);
      var addI = Expression.Binary(i, BinaryOperator.Add, one, _textRange);
      var assignI = Statement.Assignment(i, addI, _textRange);
      var body = Statement.Block(new Statement[] { assignSum, assignI }, _textRange);
      var @while = Statement.While(test, body, _textRange);
      var evalSum = Statement.Expression(sum, _textRange);
      var program = Statement.Block(new Statement[] { initialSum, initialI, @while, evalSum },
                                    _textRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(program);
      string expected = (
          $"1    MOVE      0 -1           ; 0                 {_textRange}\n" +
          $"2    MOVE      1 -1           ; 0                 {_textRange}\n" +
          $"3    LE        1 1 -2         ; 10                {_textRange}\n" +
          $"4    JMP       0 3                                {_textRange}\n" +
          $"5    ADD       0 0 1                              {_textRange}\n" +
          $"6    ADD       1 1 -3         ; 1                 {_textRange}\n" +
          $"7    JMP       0 -5                               {_textRange}\n" +
          $"8    EVAL      0                                  {_textRange}\n" +
          $"9    RETURN    0                                  \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }
  }
}
