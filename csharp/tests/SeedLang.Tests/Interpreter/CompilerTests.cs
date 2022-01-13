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
    private static GlobalEnvironment _env => new GlobalEnvironment();
    private static TextRange _textRange => new TextRange(0, 1, 2, 3);


    [Fact]
    public void TestCompileNumberConstant() {
      var number = Expression.NumberConstant(1, _textRange);
      var expr = Statement.Expression(number, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  2    EVAL      0                                    {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileBinary() {
      var left = Expression.NumberConstant(1, _textRange);
      var right = Expression.NumberConstant(2, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
      var expr = Statement.Expression(binary, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    EVAL      0                                    {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
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
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       1 -2 -3          ; 2 3               {_textRange}\n" +
          $"  2    SUB       0 -1 1           ; 1                 {_textRange}\n" +
          $"  3    EVAL      0                                    {_textRange}\n" +
          $"  4    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
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
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    SUB       0 -1 1           ; 1                 {_textRange}\n" +
          $"  3    EVAL      0                                    {_textRange}\n" +
          $"  4    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileUnary() {
      var number = Expression.NumberConstant(1, _textRange);
      var unary = Expression.Unary(UnaryOperator.Negative, number, _textRange);
      var expr = Statement.Expression(unary, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    UNM       0 -1             ; 1                 {_textRange}\n" +
          $"  2    EVAL      0                                    {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileAssignNumberConstant() {
      var identifier = Expression.Identifier("name", _textRange);
      var number = Expression.NumberConstant(1, _textRange);
      var assignment = Statement.Assignment(identifier, number, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileAssignBinary() {
      var identifier = Expression.Identifier("name", _textRange);
      var left = Expression.NumberConstant(1, _textRange);
      var right = Expression.NumberConstant(2, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
      var assignment = Statement.Assignment(identifier, binary, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIf() {
      var @if = Statement.If(
        Expression.Comparison(
          Expression.NumberConstant(1, _textRange),
          new ComparisonOperator[] { ComparisonOperator.EqEqual },
          new Expression[] { Expression.NumberConstant(2, _textRange) },
          _textRange
        ),
        Statement.Expression(Expression.NumberConstant(1, _textRange), _textRange),
        Statement.Expression(Expression.NumberConstant(2, _textRange), _textRange),
        _textRange
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    JMP       0 3                                  {_textRange}\n" +
          $"  3    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  4    EVAL      0                                    {_textRange}\n" +
          $"  5    JMP       0 2                                  {_textRange}\n" +
          $"  6    LOADK     0 -2             ; 2                 {_textRange}\n" +
          $"  7    EVAL      0                                    {_textRange}\n" +
          $"  8    RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
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
      var func = compiler.Compile(program, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 0                 {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    LOADK     0 -1             ; 0                 {_textRange}\n" +
          $"  4    SETGLOB   0 1                                  {_textRange}\n" +
          $"  5    GETGLOB   0 1                                  {_textRange}\n" +
          $"  6    LE        1 0 -2           ; 10                {_textRange}\n" +
          $"  7    JMP       0 8                                  {_textRange}\n" +
          $"  8    GETGLOB   1 0                                  {_textRange}\n" +
          $"  9    GETGLOB   2 1                                  {_textRange}\n" +
          $"  10   ADD       0 1 2                                {_textRange}\n" +
          $"  11   SETGLOB   0 0                                  {_textRange}\n" +
          $"  12   GETGLOB   1 1                                  {_textRange}\n" +
          $"  13   ADD       0 1 -3           ; 1                 {_textRange}\n" +
          $"  14   SETGLOB   0 1                                  {_textRange}\n" +
          $"  15   JMP       0 -11                                {_textRange}\n" +
          $"  16   GETGLOB   0 0                                  {_textRange}\n" +
          $"  17   EVAL      0                                    {_textRange}\n" +
          $"  18   RETURN    0                                    \n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileFuncCall() {
      string a = "a";
      string b = "b";
      var left = Expression.Identifier(a, _textRange);
      var right = Expression.Identifier(b, _textRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
      var ret = Statement.Return(binary, _textRange);
      string name = "eval";
      var funcDef = Statement.FuncDef(name, new string[] { a, b }, ret, _textRange);
      var identifier = Expression.Identifier(name, _textRange);
      var call = Expression.Call(identifier, new Expression[] {
        Expression.NumberConstant(1, _textRange),
        Expression.NumberConstant(2, _textRange),
      }, _textRange);
      var exprStatement = Statement.Expression(call, _textRange);
      var block = Statement.Block(new Statement[] { funcDef, exprStatement }, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <eval>       {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    GETGLOB   0 0                                  {_textRange}\n" +
          $"  4    LOADK     1 -2             ; 1                 {_textRange}\n" +
          $"  5    LOADK     2 -3             ; 2                 {_textRange}\n" +
          $"  6    CALL      0 2 0                                {_textRange}\n" +
          $"  7    EVAL      0                                    {_textRange}\n" +
          $"  8    RETURN    0                                    \n" +
          $"\n" +
          $"Function <eval>\n" +
          $"  1    ADD       2 0 1                                {_textRange}\n" +
          $"  2    RETURN    2                                    {_textRange}\n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestRecursiveFuncCall() {
      string sum = "sum";
      var n = "n";
      var test = Expression.Comparison(
          Expression.Identifier(n, _textRange),
          new ComparisonOperator[] { ComparisonOperator.EqEqual },
          new Expression[] { Expression.NumberConstant(1, _textRange) }, _textRange);
      var thenBlock = Statement.Return(Expression.NumberConstant(1, _textRange), _textRange);
      var subtract = Expression.Binary(
          Expression.Identifier(n, _textRange), BinaryOperator.Subtract,
          Expression.NumberConstant(1, _textRange), _textRange);
      var funcName = Expression.Identifier(sum, _textRange);
      var recursiveCall = Expression.Call(funcName, new Expression[] { subtract }, _textRange);
      var add = Expression.Binary(
       Expression.Identifier(n, _textRange), BinaryOperator.Add, recursiveCall, _textRange);
      var elseBlock = Statement.Return(add, _textRange);
      var @if = Statement.If(test, thenBlock, elseBlock, _textRange);
      var funcBody = Statement.Block(new Statement[] { @if }, _textRange);
      var funcDef = Statement.FuncDef(sum, new string[] { n }, funcBody, _textRange);
      var call = Statement.Expression(Expression.Call(funcName, new Expression[] {
     Expression.NumberConstant(10, _textRange)
   }, _textRange), _textRange);
      var block = Statement.Block(new Statement[] { funcDef, call }, _textRange);
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <sum>        {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    GETGLOB   0 0                                  {_textRange}\n" +
          $"  4    LOADK     1 -2             ; 10                {_textRange}\n" +
          $"  5    CALL      0 1 0                                {_textRange}\n" +
          $"  6    EVAL      0                                    {_textRange}\n" +
          $"  7    RETURN    0                                    \n" +
          $"\n" +
          $"Function <sum>\n" +
          $"  1    EQ        1 0 -1           ; 1                 {_textRange}\n" +
          $"  2    JMP       0 3                                  {_textRange}\n" +
          $"  3    LOADK     1 -1             ; 1                 {_textRange}\n" +
          $"  4    RETURN    1                                    {_textRange}\n" +
          $"  5    JMP       0 5                                  {_textRange}\n" +
          $"  6    GETGLOB   2 0                                  {_textRange}\n" +
          $"  7    SUB       3 0 -1           ; 1                 {_textRange}\n" +
          $"  8    CALL      2 1 0                                {_textRange}\n" +
          $"  9    ADD       1 0 2                                {_textRange}\n" +
          $"  10   RETURN    1                                    {_textRange}\n"
      ).Replace("\n", System.Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }
  }
}
