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
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    private static GlobalEnvironment _env => new GlobalEnvironment(NativeFunctions.Funcs);
    private const int _evalFunc = 1;
    private readonly int _firstGlob = NativeFunctions.Funcs.Length;
    private readonly Common.Range _range = AstHelper.TextRange;

    [Fact]
    public void TestCompileNumberConstant() {
      var expr = AstHelper.ExpressionStmt(AstHelper.NumberConstant(1));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
        $"Function <main>\n" +
        $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
        $"  2    LOADK     1 -1             ; 1                 {_range}\n" +
        $"  3    CALL      0 1 0                                {_range}\n" +
        $"  4    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileBinary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.NumberConstant(1),
                                                           BinaryOperator.Add,
                                                           AstHelper.NumberConstant(2)));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    ADD       1 -1 -2          ; 1 2               {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileComplexBinary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(
        AstHelper.NumberConstant(1),
        BinaryOperator.Subtract,
        AstHelper.Binary(AstHelper.NumberConstant(2), BinaryOperator.Add,
                         AstHelper.NumberConstant(3)))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    ADD       2 -2 -3          ; 2 3               {_range}\n" +
          $"  3    SUB       1 -1 2           ; 1                 {_range}\n" +
          $"  4    CALL      0 1 0                                {_range}\n" +
          $"  5    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileBinaryWithSameConstants() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(
        AstHelper.NumberConstant(1),
        BinaryOperator.Subtract,
        AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                         AstHelper.NumberConstant(2)))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    ADD       2 -1 -2          ; 1 2               {_range}\n" +
          $"  3    SUB       1 -1 2           ; 1                 {_range}\n" +
          $"  4    CALL      0 1 0                                {_range}\n" +
          $"  5    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileUnary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
                                                          AstHelper.NumberConstant(1)));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    UNM       1 -1             ; 1                 {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileAssignment() {
      var assignment = AstHelper.Assign(AstHelper.Targets(AstHelper.Id("name")),
                                        AstHelper.NumberConstant(1));
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileMultipleAssignment() {
      var assignment = AstHelper.Assign(AstHelper.Targets(AstHelper.Id("x"), AstHelper.Id("y")),
                                        AstHelper.NumberConstant(1), AstHelper.NumberConstant(2));
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    LOADK     1 -2             ; 2                 {_range}\n" +
          $"  4    SETGLOB   1 {_firstGlob + 1}                                  {_range}\n" +
          $"  5    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileAssignBinary() {
      var assignment = AstHelper.Assign(
        AstHelper.Targets(AstHelper.Id("name")),
        AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                         AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(assignment, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       0 -1 -2          ; 1 2               {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompilePackAssignment() {
      string name = "id";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)),
                         AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.Id(name))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  2    LOADK     2 -2             ; 2                 {_range}\n" +
          $"  3    NEWTUPLE  0 1 2                                {_range}\n" +
          $"  4    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  5    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  6    GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileUnpackAssignment() {
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b)),
                         AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2))),
        AstHelper.ExpressionStmt(AstHelper.Id(a)),
        AstHelper.ExpressionStmt(AstHelper.Id(b))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  2    LOADK     2 -2             ; 2                 {_range}\n" +
          $"  3    NEWLIST   0 1 2                                {_range}\n" +
          $"  4    LOADK     2 -3             ; 0                 {_range}\n" +
          $"  5    GETELEM   1 0 2                                {_range}\n" +
          $"  6    SETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  7    LOADK     2 -1             ; 1                 {_range}\n" +
          $"  8    GETELEM   1 0 2                                {_range}\n" +
          $"  9    SETGLOB   1 {_firstGlob + 1}                                  {_range}\n" +
          $"  10   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  11   GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  12   CALL      0 1 0                                {_range}\n" +
          $"  13   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  14   GETGLOB   1 {_firstGlob + 1}                                  {_range}\n" +
          $"  15   CALL      0 1 0                                {_range}\n" +
          $"  16   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIf() {
      var @if = AstHelper.If(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 4              ; to 7              {_range}\n" +
          $"  3    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  4    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  5    CALL      0 1 0                                {_range}\n" +
          $"  6    JMP       0 3              ; to 10             {_range}\n" +
          $"  7    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  8    LOADK     1 -2             ; 2                 {_range}\n" +
          $"  9    CALL      0 1 0                                {_range}\n" +
          $"  10   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfTrue() {
      var @if = AstHelper.If(
        AstHelper.BooleanConstant(true),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADBOOL  0 1 0                                {_range}\n" +
          $"  2    TEST      0 0 1                                {_range}\n" +
          $"  3    JMP       0 4              ; to 8              {_range}\n" +
          $"  4    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  5    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  6    CALL      0 1 0                                {_range}\n" +
          $"  7    JMP       0 3              ; to 11             {_range}\n" +
          $"  8    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  9    LOADK     1 -2             ; 2                 {_range}\n" +
          $"  10   CALL      0 1 0                                {_range}\n" +
          $"  11   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfWithNullElse() {
      var @if = AstHelper.If(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        null
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 3              ; to 6              {_range}\n" +
          $"  3    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  4    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  5    CALL      0 1 0                                {_range}\n" +
          $"  6    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfMultipleComparison() {
      var @if = AstHelper.If(
        AstHelper.Comparison(
          AstHelper.NumberConstant(1),
          AstHelper.CompOps(ComparisonOperator.Less, ComparisonOperator.Less),
          AstHelper.NumberConstant(2),
          AstHelper.NumberConstant(3)
        ),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 6              ; to 9              {_range}\n" +
          $"  3    LT        1 -2 -3          ; 2 3               {_range}\n" +
          $"  4    JMP       0 4              ; to 9              {_range}\n" +
          $"  5    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  6    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    JMP       0 3              ; to 12             {_range}\n" +
          $"  9    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  10   LOADK     1 -2             ; 2                 {_range}\n" +
          $"  11   CALL      0 1 0                                {_range}\n" +
          $"  12   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfAndBooleanExpression() {
      var @if = AstHelper.If(
        AstHelper.Boolean(
          BooleanOperator.And,
          AstHelper.Comparison(AstHelper.NumberConstant(1),
                               AstHelper.CompOps(ComparisonOperator.EqEqual),
                               AstHelper.NumberConstant(2)),
          AstHelper.Comparison(AstHelper.NumberConstant(1),
                               AstHelper.CompOps(ComparisonOperator.Greater),
                               AstHelper.NumberConstant(2)),
          AstHelper.Comparison(AstHelper.NumberConstant(1),
                               AstHelper.CompOps(ComparisonOperator.GreaterEqual),
                               AstHelper.NumberConstant(2))
        ),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 8              ; to 11             {_range}\n" +
          $"  3    LE        0 -1 -2          ; 1 2               {_range}\n" +
          $"  4    JMP       0 6              ; to 11             {_range}\n" +
          $"  5    LT        0 -1 -2          ; 1 2               {_range}\n" +
          $"  6    JMP       0 4              ; to 11             {_range}\n" +
          $"  7    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  8    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  9    CALL      0 1 0                                {_range}\n" +
          $"  10   JMP       0 3              ; to 14             {_range}\n" +
          $"  11   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  12   LOADK     1 -2             ; 2                 {_range}\n" +
          $"  13   CALL      0 1 0                                {_range}\n" +
          $"  14   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfOrBooleanExpression() {
      var @if = AstHelper.If(
        AstHelper.Boolean(
          BooleanOperator.Or,
          AstHelper.Comparison(AstHelper.NumberConstant(1),
                               AstHelper.CompOps(ComparisonOperator.Less),
                               AstHelper.NumberConstant(2)),
          AstHelper.Comparison(AstHelper.NumberConstant(1),
                               AstHelper.CompOps(ComparisonOperator.LessEqual),
                               AstHelper.NumberConstant(2)),
          AstHelper.Comparison(AstHelper.NumberConstant(1),
                               AstHelper.CompOps(ComparisonOperator.NotEqual),
                               AstHelper.NumberConstant(2))
        ),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        0 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 4              ; to 7              {_range}\n" +
          $"  3    LE        0 -1 -2          ; 1 2               {_range}\n" +
          $"  4    JMP       0 2              ; to 7              {_range}\n" +
          $"  5    EQ        0 -1 -2          ; 1 2               {_range}\n" +
          $"  6    JMP       0 4              ; to 11             {_range}\n" +
          $"  7    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  8    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  9    CALL      0 1 0                                {_range}\n" +
          $"  10   JMP       0 3              ; to 14             {_range}\n" +
          $"  11   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  12   LOADK     1 -2             ; 2                 {_range}\n" +
          $"  13   CALL      0 1 0                                {_range}\n" +
          $"  14   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileIfOrWithMultipleComparison() {
      var @if = AstHelper.If(
        AstHelper.Boolean(
          BooleanOperator.Or,
          AstHelper.Comparison(
            AstHelper.NumberConstant(1),
            AstHelper.CompOps(ComparisonOperator.Less, ComparisonOperator.Less),
            AstHelper.NumberConstant(2),
            AstHelper.NumberConstant(3)
          ),
          AstHelper.Comparison(
            AstHelper.NumberConstant(1),
            AstHelper.CompOps(ComparisonOperator.LessEqual, ComparisonOperator.LessEqual),
            AstHelper.NumberConstant(2),
            AstHelper.NumberConstant(3)
          )
        ),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 2              ; to 5              {_range}\n" +
          $"  3    LT        0 -2 -3          ; 2 3               {_range}\n" +
          $"  4    JMP       0 4              ; to 9              {_range}\n" +
          $"  5    LE        1 -1 -2          ; 1 2               {_range}\n" +
          $"  6    JMP       0 6              ; to 13             {_range}\n" +
          $"  7    LE        1 -2 -3          ; 2 3               {_range}\n" +
          $"  8    JMP       0 4              ; to 13             {_range}\n" +
          $"  9    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  10   LOADK     1 -1             ; 1                 {_range}\n" +
          $"  11   CALL      0 1 0                                {_range}\n" +
          $"  12   JMP       0 3              ; to 16             {_range}\n" +
          $"  13   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  14   LOADK     1 -2             ; 2                 {_range}\n" +
          $"  15   CALL      0 1 0                                {_range}\n" +
          $"  16   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileNestedIf() {
      var @if = AstHelper.If(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(2)),
        AstHelper.If(
          AstHelper.Comparison(AstHelper.NumberConstant(3),
                               AstHelper.CompOps(ComparisonOperator.Less),
                               AstHelper.NumberConstant(4)),
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
        ),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(3))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(@if, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 10             ; to 13             {_range}\n" +
          $"  3    LT        1 -3 -4          ; 3 4               {_range}\n" +
          $"  4    JMP       0 4              ; to 9              {_range}\n" +
          $"  5    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  6    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    JMP       0 3              ; to 12             {_range}\n" +
          $"  9    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  10   LOADK     1 -2             ; 2                 {_range}\n" +
          $"  11   CALL      0 1 0                                {_range}\n" +
          $"  12   JMP       0 3              ; to 16             {_range}\n" +
          $"  13   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  14   LOADK     1 -3             ; 3                 {_range}\n" +
          $"  15   CALL      0 1 0                                {_range}\n" +
          $"  16   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileWhile() {
      string sum = "sum";
      string i = "i";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(sum)), AstHelper.NumberConstant(0)),
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(i)), AstHelper.NumberConstant(0)),
        AstHelper.While(
          AstHelper.Comparison(AstHelper.Id(i), AstHelper.CompOps(ComparisonOperator.LessEqual),
                               AstHelper.NumberConstant(10)),
          AstHelper.Block(
            AstHelper.Assign(AstHelper.Targets(AstHelper.Id(sum)),
                             AstHelper.Binary(AstHelper.Id(sum), BinaryOperator.Add,
                                              AstHelper.Id(i))),
            AstHelper.Assign(AstHelper.Targets(AstHelper.Id(i)),
                             AstHelper.Binary(AstHelper.Id(i), BinaryOperator.Add,
                                              AstHelper.NumberConstant(1)))
          )
        ),
        AstHelper.ExpressionStmt(AstHelper.Id(sum))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(program, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 0                 {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    LOADK     0 -1             ; 0                 {_range}\n" +
          $"  4    SETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  5    GETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  6    LE        1 0 -2           ; 10                {_range}\n" +
          $"  7    JMP       0 8              ; to 16             {_range}\n" +
          $"  8    GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  9    GETGLOB   2 {_firstGlob + 1}                                  {_range}\n" +
          $"  10   ADD       0 1 2                                {_range}\n" +
          $"  11   SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  12   GETGLOB   1 {_firstGlob + 1}                                  {_range}\n" +
          $"  13   ADD       0 1 -3           ; 1                 {_range}\n" +
          $"  14   SETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  15   JMP       0 -11            ; to 5              {_range}\n" +
          $"  16   GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  17   GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  18   CALL      0 1 0                                {_range}\n" +
          $"  19   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileFuncCall() {
      string add = "add";
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.FuncDef(add, AstHelper.Params(a, b),
                          AstHelper.Return(AstHelper.Binary(AstHelper.Id(a), BinaryOperator.Add,
                                                            AstHelper.Id(b)))),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(add), AstHelper.NumberConstant(1),
                                                AstHelper.NumberConstant(2)))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <add>        {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  4    GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  5    LOADK     2 -2             ; 1                 {_range}\n" +
          $"  6    LOADK     3 -3             ; 2                 {_range}\n" +
          $"  7    CALL      1 2 0                                {_range}\n" +
          $"  8    CALL      0 1 0                                {_range}\n" +
          $"  9    RETURN    0 0                                  \n" +
          $"\n" +
          $"Function <add>\n" +
          $"  1    ADD       2 0 1                                {_range}\n" +
          $"  2    RETURN    2 1                                  {_range}\n" +
          $"  3    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestRecursiveFuncCall() {
      string sum = "sum";
      var n = "n";
      var block = AstHelper.Block(
        AstHelper.FuncDef(sum, AstHelper.Params(n), AstHelper.Block(
          AstHelper.If(
            AstHelper.Comparison(AstHelper.Id(n), AstHelper.CompOps(ComparisonOperator.EqEqual),
                                 AstHelper.NumberConstant(1)),
            AstHelper.Return(AstHelper.NumberConstant(1)),
            AstHelper.Return(
              AstHelper.Binary(AstHelper.Id(n), BinaryOperator.Add,
                               AstHelper.Call(AstHelper.Id(sum),
                                              AstHelper.Binary(AstHelper.Id(n),
                                                               BinaryOperator.Subtract,
                                                               AstHelper.NumberConstant(1)))
            ))
          )
        )),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(sum), AstHelper.NumberConstant(10)))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <sum>        {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  4    GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  5    LOADK     2 -2             ; 10                {_range}\n" +
          $"  6    CALL      1 1 0                                {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    RETURN    0 0                                  \n" +
          $"\n" +
          $"Function <sum>\n" +
          $"  1    EQ        1 0 -1           ; 1                 {_range}\n" +
          $"  2    JMP       0 3              ; to 6              {_range}\n" +
          $"  3    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  4    RETURN    1 1                                  {_range}\n" +
          $"  5    JMP       0 5              ; to 11             {_range}\n" +
          $"  6    GETGLOB   2 {_firstGlob}                                  {_range}\n" +
          $"  7    SUB       3 0 -1           ; 1                 {_range}\n" +
          $"  8    CALL      2 1 0                                {_range}\n" +
          $"  9    ADD       1 0 2                                {_range}\n" +
          $"  10   RETURN    1 1                                  {_range}\n" +
          $"  11   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileEmptyList() {
      var list = AstHelper.ExpressionStmt(AstHelper.List());
      var compiler = new Compiler();
      var func = compiler.Compile(list, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    NEWLIST   1 0 0                                {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileList() {
      var list = AstHelper.ExpressionStmt(AstHelper.List(AstHelper.NumberConstant(1),
                                                         AstHelper.NumberConstant(2),
                                                         AstHelper.NumberConstant(3)));
      var compiler = new Compiler();
      var func = compiler.Compile(list, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    LOADK     2 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     3 -2             ; 2                 {_range}\n" +
          $"  4    LOADK     4 -3             ; 3                 {_range}\n" +
          $"  5    NEWLIST   1 2 3                                {_range}\n" +
          $"  6    CALL      0 1 0                                {_range}\n" +
          $"  7    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileSubscript() {
      var subscript = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(0)
      ));
      var compiler = new Compiler();
      var func = compiler.Compile(subscript, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  2    LOADK     3 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     4 -2             ; 2                 {_range}\n" +
          $"  4    LOADK     5 -3             ; 3                 {_range}\n" +
          $"  5    NEWLIST   2 3 3                                {_range}\n" +
          $"  6    GETELEM   1 2 -4           ; 0                 {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileSubscriptAssignment() {
      string a = "a";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)),
                         AstHelper.List(AstHelper.NumberConstant(1),
                                        AstHelper.NumberConstant(2),
                                        AstHelper.NumberConstant(3))),
        AstHelper.Assign(AstHelper.Targets(AstHelper.Subscript(AstHelper.Id(a),
                                                               AstHelper.NumberConstant(1))),
                         AstHelper.NumberConstant(5)),
        AstHelper.ExpressionStmt(AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(1)))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(program, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  2    LOADK     2 -2             ; 2                 {_range}\n" +
          $"  3    LOADK     3 -3             ; 3                 {_range}\n" +
          $"  4    NEWLIST   0 1 3                                {_range}\n" +
          $"  5    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  6    GETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  7    SETELEM   0 -1 -4          ; 1 5               {_range}\n" +
          $"  8    GETGLOB   0 {_evalFunc}                                  {_range}\n" +
          $"  9    GETGLOB   2 {_firstGlob}                                  {_range}\n" +
          $"  10   GETELEM   1 2 -1           ; 1                 {_range}\n" +
          $"  11   CALL      0 1 0                                {_range}\n" +
          $"  12   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileMultipleSubscriptAssignment() {
      string a = "a";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)),
                         AstHelper.List(AstHelper.NumberConstant(1),
                                        AstHelper.NumberConstant(2),
                                        AstHelper.NumberConstant(3))),
        AstHelper.Assign(AstHelper.Targets(
            AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(0)),
            AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(1))
          ),
          AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(1)),
          AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(0))
        )
      );
      var compiler = new Compiler();
      var func = compiler.Compile(program, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  2    LOADK     2 -2             ; 2                 {_range}\n" +
          $"  3    LOADK     3 -3             ; 3                 {_range}\n" +
          $"  4    NEWLIST   0 1 3                                {_range}\n" +
          $"  5    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  6    GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  7    GETELEM   0 1 -1           ; 1                 {_range}\n" +
          $"  8    GETGLOB   2 {_firstGlob}                                  {_range}\n" +
          $"  9    GETELEM   1 2 -4           ; 0                 {_range}\n" +
          $"  10   GETGLOB   2 {_firstGlob}                                  {_range}\n" +
          $"  11   SETELEM   2 -4 0           ; 0                 {_range}\n" +
          $"  12   GETGLOB   3 {_firstGlob}                                  {_range}\n" +
          $"  13   SETELEM   3 -1 1           ; 1                 {_range}\n" +
          $"  14   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileForIn() {
      string a = "a";
      var program = AstHelper.ForIn(
        AstHelper.Id(a),
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.ExpressionStmt(AstHelper.Id(a))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(program, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  2    LOADK     2 -2             ; 2                 {_range}\n" +
          $"  3    LOADK     3 -3             ; 3                 {_range}\n" +
          $"  4    NEWLIST   0 1 3                                {_range}\n" +
          $"  5    LOADK     1 -4             ; 0                 {_range}\n" +
          $"  6    LEN       2 0 0                                {_range}\n" +
          $"  7    LOADK     3 -1             ; 1                 {_range}\n" +
          $"  8    FORPREP   1 5              ; to 14             {_range}\n" +
          $"  9    GETELEM   4 0 1                                {_range}\n" +
          $"  10   SETGLOB   4 {_firstGlob}                                  {_range}\n" +
          $"  11   GETGLOB   4 {_evalFunc}                                  {_range}\n" +
          $"  12   GETGLOB   5 {_firstGlob}                                  {_range}\n" +
          $"  13   CALL      4 1 0                                {_range}\n" +
          $"  14   FORLOOP   1 -6             ; to 9              {_range}\n" +
          $"  15   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileLocalScopeForIn() {
      string a = "a";
      var program = AstHelper.FuncDef("func", Array.Empty<string>(), AstHelper.Block(
        AstHelper.ForIn(
          AstHelper.Id(a),
          AstHelper.List(AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2),
                         AstHelper.NumberConstant(3)),
          AstHelper.ExpressionStmt(AstHelper.Id(a))
        )
      ));
      var compiler = new Compiler();
      var func = compiler.Compile(program, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <func>       {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    RETURN    0 0                                  \n" +
          $"\n" +
          $"Function <func>\n" +
          $"  1    LOADK     2 -1             ; 1                 {_range}\n" +
          $"  2    LOADK     3 -2             ; 2                 {_range}\n" +
          $"  3    LOADK     4 -3             ; 3                 {_range}\n" +
          $"  4    NEWLIST   1 2 3                                {_range}\n" +
          $"  5    LOADK     2 -4             ; 0                 {_range}\n" +
          $"  6    LEN       3 1 0                                {_range}\n" +
          $"  7    LOADK     4 -1             ; 1                 {_range}\n" +
          $"  8    FORPREP   2 4              ; to 13             {_range}\n" +
          $"  9    GETELEM   0 1 2                                {_range}\n" +
          $"  10   GETGLOB   5 {_evalFunc}                                  {_range}\n" +
          $"  11   MOVE      6 0                                  {_range}\n" +
          $"  12   CALL      5 1 0                                {_range}\n" +
          $"  13   FORLOOP   2 -5             ; to 9              {_range}\n" +
          $"  14   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestUndefinedVariable() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Id("a"));
      var compiler = new Compiler();
      var exception = Assert.Throws<DiagnosticException>(() => compiler.Compile(expr, _env));
      Assert.Equal(Message.RuntimeErrorVariableNotDefined, exception.Diagnostic.MessageId);
    }
  }
}
