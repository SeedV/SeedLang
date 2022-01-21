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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    private static GlobalEnvironment _env => new GlobalEnvironment(Array.Empty<NativeFunction>());
    private static TextRange _textRange => new TextRange(0, 1, 2, 3);


    [Fact]
    public void TestCompileNumberConstant() {
      var expr = ExpressionStmt(NumberConstant(1));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  2    EVAL      0                                    {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileBinary() {
      var expr = ExpressionStmt(Binary(NumberConstant(1), BinaryOperator.Add, NumberConstant(2)));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    EVAL      0                                    {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileComplexBinary() {
      var expr = ExpressionStmt(
        Binary(
          NumberConstant(1),
          BinaryOperator.Subtract,
          Binary(NumberConstant(2), BinaryOperator.Add, NumberConstant(3))
        )
      );
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       1 -2 -3          ; 2 3               {_textRange}\n" +
          $"  2    SUB       0 -1 1           ; 1                 {_textRange}\n" +
          $"  3    EVAL      0                                    {_textRange}\n" +
          $"  4    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileBinaryWithSameConstants() {
      var expr = ExpressionStmt(
        Binary(
          NumberConstant(1),
          BinaryOperator.Subtract,
          Binary(NumberConstant(1), BinaryOperator.Add, NumberConstant(2))
        )
      );
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    SUB       0 -1 1           ; 1                 {_textRange}\n" +
          $"  3    EVAL      0                                    {_textRange}\n" +
          $"  4    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileUnary() {
      var expr = ExpressionStmt(Unary(UnaryOperator.Negative, NumberConstant(1)));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    UNM       0 -1             ; 1                 {_textRange}\n" +
          $"  2    EVAL      0                                    {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileAssignNumberConstant() {
      var assignment = Assign(Id("name"), NumberConstant(1));
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileAssignBinary() {
      var assignment = Assign(
        Id("name"),
        Binary(NumberConstant(1), BinaryOperator.Add, NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    SETGLOB   0 0                                  {_textRange}\n" +
          $"  3    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIf() {
      var @if = If(
        Comparison(NumberConstant(1), CompOps(ComparisonOperator.EqEqual), NumberConstant(2)),
        ExpressionStmt(NumberConstant(1)),
        ExpressionStmt(NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    JMP       0 3              ; to 6              {_textRange}\n" +
          $"  3    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  4    EVAL      0                                    {_textRange}\n" +
          $"  5    JMP       0 2              ; to 8              {_textRange}\n" +
          $"  6    LOADK     0 -2             ; 2                 {_textRange}\n" +
          $"  7    EVAL      0                                    {_textRange}\n" +
          $"  8    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfMultipleComparison() {
      var @if = If(
        Comparison(
          NumberConstant(1),
          CompOps(ComparisonOperator.Less, ComparisonOperator.Less),
          NumberConstant(2),
          NumberConstant(3)
        ),
        ExpressionStmt(NumberConstant(1)),
        ExpressionStmt(NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    JMP       0 5              ; to 8              {_textRange}\n" +
          $"  3    LT        1 -2 -3          ; 2 3               {_textRange}\n" +
          $"  4    JMP       0 3              ; to 8              {_textRange}\n" +
          $"  5    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  6    EVAL      0                                    {_textRange}\n" +
          $"  7    JMP       0 2              ; to 10             {_textRange}\n" +
          $"  8    LOADK     0 -2             ; 2                 {_textRange}\n" +
          $"  9    EVAL      0                                    {_textRange}\n" +
          $"  10   RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfAndBooleanExpression() {
      var @if = If(
        Boolean(
          BooleanOperator.And,
          Comparison(NumberConstant(1), CompOps(ComparisonOperator.EqEqual), NumberConstant(2)),
          Comparison(NumberConstant(1), CompOps(ComparisonOperator.Greater), NumberConstant(2)),
          Comparison(NumberConstant(1), CompOps(ComparisonOperator.GreaterEqual), NumberConstant(2))
        ),
        ExpressionStmt(NumberConstant(1)),
        ExpressionStmt(NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    JMP       0 7              ; to 10             {_textRange}\n" +
          $"  3    LE        0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  4    JMP       0 5              ; to 10             {_textRange}\n" +
          $"  5    LT        0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  6    JMP       0 3              ; to 10             {_textRange}\n" +
          $"  7    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  8    EVAL      0                                    {_textRange}\n" +
          $"  9    JMP       0 2              ; to 12             {_textRange}\n" +
          $"  10   LOADK     0 -2             ; 2                 {_textRange}\n" +
          $"  11   EVAL      0                                    {_textRange}\n" +
          $"  12   RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfOrBooleanExpression() {
      var @if = If(
        Boolean(
          BooleanOperator.Or,
          Comparison(NumberConstant(1), CompOps(ComparisonOperator.Less), NumberConstant(2)),
          Comparison(NumberConstant(1), CompOps(ComparisonOperator.LessEqual), NumberConstant(2)),
          Comparison(NumberConstant(1), CompOps(ComparisonOperator.NotEqual), NumberConstant(2))
        ),
        ExpressionStmt(NumberConstant(1)),
        ExpressionStmt(NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    JMP       0 4              ; to 7              {_textRange}\n" +
          $"  3    LE        0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  4    JMP       0 2              ; to 7              {_textRange}\n" +
          $"  5    EQ        0 -1 -2          ; 1 2               {_textRange}\n" +
          $"  6    JMP       0 3              ; to 10             {_textRange}\n" +
          $"  7    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  8    EVAL      0                                    {_textRange}\n" +
          $"  9    JMP       0 2              ; to 12             {_textRange}\n" +
          $"  10   LOADK     0 -2             ; 2                 {_textRange}\n" +
          $"  11   EVAL      0                                    {_textRange}\n" +
          $"  12   RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfOrWithMultipleComparison() {
      var @if = If(
        Boolean(
          BooleanOperator.Or,
          Comparison(
            NumberConstant(1),
            CompOps(ComparisonOperator.Less, ComparisonOperator.Less),
            NumberConstant(2),
            NumberConstant(3)
          ),
          Comparison(
            NumberConstant(1),
            CompOps(ComparisonOperator.LessEqual, ComparisonOperator.LessEqual),
            NumberConstant(2),
            NumberConstant(3)
          )
        ),
        ExpressionStmt(NumberConstant(1)),
        ExpressionStmt(NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  2    JMP       0 2              ; to 5              {_textRange}\n" +
          $"  3    LT        0 -2 -3          ; 2 3               {_textRange}\n" +
          $"  4    JMP       0 4              ; to 9              {_textRange}\n" +
          $"  5    LE        1 -1 -2          ; 1 2               {_textRange}\n" +
          $"  6    JMP       0 5              ; to 12             {_textRange}\n" +
          $"  7    LE        1 -2 -3          ; 2 3               {_textRange}\n" +
          $"  8    JMP       0 3              ; to 12             {_textRange}\n" +
          $"  9    LOADK     0 -1             ; 1                 {_textRange}\n" +
          $"  10   EVAL      0                                    {_textRange}\n" +
          $"  11   JMP       0 2              ; to 14             {_textRange}\n" +
          $"  12   LOADK     0 -2             ; 2                 {_textRange}\n" +
          $"  13   EVAL      0                                    {_textRange}\n" +
          $"  14   RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileWhile() {
      string sum = "sum";
      string i = "i";
      var program = Block(
        Assign(Id(sum), NumberConstant(0)),
        Assign(Id(i), NumberConstant(0)),
        While(
          Comparison(Id(i), CompOps(ComparisonOperator.LessEqual), NumberConstant(10)),
          Block(
            Assign(Id(sum), Binary(Id(sum), BinaryOperator.Add, Id(i))),
            Assign(Id(i), Binary(Id(i), BinaryOperator.Add, NumberConstant(1)))
          )
        ),
        ExpressionStmt(Id(sum))
      );
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
          $"  7    JMP       0 8              ; to 16             {_textRange}\n" +
          $"  8    GETGLOB   1 0                                  {_textRange}\n" +
          $"  9    GETGLOB   2 1                                  {_textRange}\n" +
          $"  10   ADD       0 1 2                                {_textRange}\n" +
          $"  11   SETGLOB   0 0                                  {_textRange}\n" +
          $"  12   GETGLOB   1 1                                  {_textRange}\n" +
          $"  13   ADD       0 1 -3           ; 1                 {_textRange}\n" +
          $"  14   SETGLOB   0 1                                  {_textRange}\n" +
          $"  15   JMP       0 -11            ; to 5              {_textRange}\n" +
          $"  16   GETGLOB   0 0                                  {_textRange}\n" +
          $"  17   EVAL      0                                    {_textRange}\n" +
          $"  18   RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileFuncCall() {
      string eval = "eval";
      string a = "a";
      string b = "b";
      var block = Block(
        FuncDef(eval, Params(a, b), Return(Binary(Id(a), BinaryOperator.Add, Id(b)))),
        ExpressionStmt(Call(Id(eval), NumberConstant(1), NumberConstant(2)))
      );
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
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestRecursiveFuncCall() {
      string sum = "sum";
      var n = "n";
      var block = Block(
        FuncDef(sum, Params(n), Block(
          If(
            Comparison(Id(n), CompOps(ComparisonOperator.EqEqual), NumberConstant(1)),
            Return(NumberConstant(1)),
            Return(Binary(
              Id(n),
              BinaryOperator.Add,
              Call(Id(sum), Binary(Id(n), BinaryOperator.Subtract, NumberConstant(1)))
            ))
          )
        )),
        ExpressionStmt(Call(Id(sum), NumberConstant(10)))
      );
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
          $"  2    JMP       0 3              ; to 6              {_textRange}\n" +
          $"  3    LOADK     1 -1             ; 1                 {_textRange}\n" +
          $"  4    RETURN    1                                    {_textRange}\n" +
          $"  5    JMP       0 5              ; to 11             {_textRange}\n" +
          $"  6    GETGLOB   2 0                                  {_textRange}\n" +
          $"  7    SUB       3 0 -1           ; 1                 {_textRange}\n" +
          $"  8    CALL      2 1 0                                {_textRange}\n" +
          $"  9    ADD       1 0 2                                {_textRange}\n" +
          $"  10   RETURN    1                                    {_textRange}\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileList() {
      var list = ExpressionStmt(List(NumberConstant(1), NumberConstant(2), NumberConstant(3)));
      var compiler = new Compiler();
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(list, env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 0                                  {_textRange}\n" +
          $"  2    LOADK     1 -1             ; 1                 {_textRange}\n" +
          $"  3    LOADK     2 -2             ; 2                 {_textRange}\n" +
          $"  4    LOADK     3 -3             ; 3                 {_textRange}\n" +
          $"  5    CALL      0 3 0                                {_textRange}\n" +
          $"  6    EVAL      0                                    {_textRange}\n" +
          $"  7    RETURN    0                                    \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    private static AssignmentStatement Assign(Expression target, Expression expr) {
      return Statement.Assignment(target, expr, _textRange);
    }

    private static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right) {
      return Expression.Binary(left, op, right, _textRange);
    }

    private static BlockStatement Block(params Statement[] statements) {
      return Statement.Block(statements, _textRange);
    }

    private static BooleanExpression Boolean(BooleanOperator op, params Expression[] exprs) {
      return Expression.Boolean(op, exprs, _textRange);
    }

    private static CallExpression Call(Expression func, params Expression[] arguments) {
      return Expression.Call(func, arguments, _textRange);
    }

    private static ComparisonExpression Comparison(Expression first, ComparisonOperator[] ops,
                                                    params Expression[] exprs) {
      return Expression.Comparison(first, ops, exprs, _textRange);
    }

    private static ComparisonOperator[] CompOps(params ComparisonOperator[] ops) {
      return ops;
    }

    private static ExpressionStatement ExpressionStmt(Expression expr) {
      return Statement.Expression(expr, _textRange);
    }

    private static FuncDefStatement FuncDef(string name, string[] parameters, Statement body) {
      return Statement.FuncDef(name, parameters, body, _textRange);
    }

    private static ListExpression List(params Expression[] exprs) {
      return Expression.List(exprs, _textRange);
    }

    private static string[] Params(params string[] parameters) {
      return parameters;
    }

    private static IdentifierExpression Id(string name) {
      return Expression.Identifier(name, _textRange);
    }

    private static IfStatement If(Expression test, Statement thenBody, Statement elseBody) {
      return Statement.If(test, thenBody, elseBody, _textRange);
    }

    private static NumberConstantExpression NumberConstant(double value) {
      return Expression.NumberConstant(value, _textRange);
    }

    private static ReturnStatement Return(Expression result) {
      return Statement.Return(result, _textRange);
    }

    private static UnaryExpression Unary(UnaryOperator op, Expression expr) {
      return Expression.Unary(op, expr, _textRange);
    }

    private static WhileStatement While(Expression test, Statement body) {
      return Statement.While(test, body, _textRange);
    }
  }
}
