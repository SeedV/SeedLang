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
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  using NativeFunction = HeapObject.NativeFunction;

  public class CompilerTests {
    private static GlobalEnvironment _env => new GlobalEnvironment(Array.Empty<NativeFunction>());

    [Fact]
    public void TestCompileNumberConstant() {
      var expr = AstHelper.ExpressionStmt(AstHelper.NumberConstant(1));
      var compiler = new Compiler();
      var func = compiler.Compile(expr, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  3    RETURN    0 0                                  \n"
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
          $"  1    ADD       0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  3    RETURN    0 0                                  \n"
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
          $"  1    ADD       1 -2 -3          ; 2 3               {AstHelper.TextRange}\n" +
          $"  2    SUB       0 -1 1           ; 1                 {AstHelper.TextRange}\n" +
          $"  3    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  4    RETURN    0 0                                  \n"
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
          $"  1    ADD       1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    SUB       0 -1 1           ; 1                 {AstHelper.TextRange}\n" +
          $"  3    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  4    RETURN    0 0                                  \n"
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
          $"  1    UNM       0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  3    RETURN    0 0                                  \n"
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
          $"  1    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
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
          $"  1    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  3    LOADK     1 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  4    SETGLOB   1 1                                  {AstHelper.TextRange}\n" +
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
          $"  1    ADD       0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
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
          $"  1    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     2 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    NEWTUPLE  0 1 2                                {AstHelper.TextRange}\n" +
          $"  4    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  5    GETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  6    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  7    RETURN    0 0                                  \n"
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
          $"  1    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     2 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    NEWLIST   0 1 2                                {AstHelper.TextRange}\n" +
          $"  4    LOADK     2 -3             ; 0                 {AstHelper.TextRange}\n" +
          $"  5    GETELEM   1 0 2                                {AstHelper.TextRange}\n" +
          $"  6    SETGLOB   1 0                                  {AstHelper.TextRange}\n" +
          $"  7    LOADK     2 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  8    GETELEM   1 0 2                                {AstHelper.TextRange}\n" +
          $"  9    SETGLOB   1 1                                  {AstHelper.TextRange}\n" +
          $"  10   GETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  11   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  12   GETGLOB   0 1                                  {AstHelper.TextRange}\n" +
          $"  13   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  14   RETURN    0 0                                  \n"
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
          $"  1    EQ        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 3              ; to 6              {AstHelper.TextRange}\n" +
          $"  3    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  4    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  5    JMP       0 2              ; to 8              {AstHelper.TextRange}\n" +
          $"  6    LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  7    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  8    RETURN    0 0                                  \n"
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
          $"  1    LOADBOOL  0 1 0                                {AstHelper.TextRange}\n" +
          $"  2    TEST      0 0 1                                {AstHelper.TextRange}\n" +
          $"  3    JMP       0 3              ; to 7              {AstHelper.TextRange}\n" +
          $"  4    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  5    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  6    JMP       0 2              ; to 9              {AstHelper.TextRange}\n" +
          $"  7    LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  8    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  9    RETURN    0 0                                  \n"
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
          $"  1    EQ        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 2              ; to 5              {AstHelper.TextRange}\n" +
          $"  3    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  4    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  5    RETURN    0 0                                  \n"
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
          $"  1    LT        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 5              ; to 8              {AstHelper.TextRange}\n" +
          $"  3    LT        1 -2 -3          ; 2 3               {AstHelper.TextRange}\n" +
          $"  4    JMP       0 3              ; to 8              {AstHelper.TextRange}\n" +
          $"  5    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  6    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  7    JMP       0 2              ; to 10             {AstHelper.TextRange}\n" +
          $"  8    LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  9    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  10   RETURN    0 0                                  \n"
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
          $"  1    EQ        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 7              ; to 10             {AstHelper.TextRange}\n" +
          $"  3    LE        0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  4    JMP       0 5              ; to 10             {AstHelper.TextRange}\n" +
          $"  5    LT        0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  6    JMP       0 3              ; to 10             {AstHelper.TextRange}\n" +
          $"  7    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  8    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  9    JMP       0 2              ; to 12             {AstHelper.TextRange}\n" +
          $"  10   LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  11   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  12   RETURN    0 0                                  \n"
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
          $"  1    LT        0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 4              ; to 7              {AstHelper.TextRange}\n" +
          $"  3    LE        0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  4    JMP       0 2              ; to 7              {AstHelper.TextRange}\n" +
          $"  5    EQ        0 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  6    JMP       0 3              ; to 10             {AstHelper.TextRange}\n" +
          $"  7    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  8    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  9    JMP       0 2              ; to 12             {AstHelper.TextRange}\n" +
          $"  10   LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  11   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  12   RETURN    0 0                                  \n"
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
          $"  1    LT        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 2              ; to 5              {AstHelper.TextRange}\n" +
          $"  3    LT        0 -2 -3          ; 2 3               {AstHelper.TextRange}\n" +
          $"  4    JMP       0 4              ; to 9              {AstHelper.TextRange}\n" +
          $"  5    LE        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  6    JMP       0 5              ; to 12             {AstHelper.TextRange}\n" +
          $"  7    LE        1 -2 -3          ; 2 3               {AstHelper.TextRange}\n" +
          $"  8    JMP       0 3              ; to 12             {AstHelper.TextRange}\n" +
          $"  9    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  10   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  11   JMP       0 2              ; to 14             {AstHelper.TextRange}\n" +
          $"  12   LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  13   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  14   RETURN    0 0                                  \n"
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
          $"  1    EQ        1 -1 -2          ; 1 2               {AstHelper.TextRange}\n" +
          $"  2    JMP       0 8              ; to 11             {AstHelper.TextRange}\n" +
          $"  3    LT        1 -3 -4          ; 3 4               {AstHelper.TextRange}\n" +
          $"  4    JMP       0 3              ; to 8              {AstHelper.TextRange}\n" +
          $"  5    LOADK     0 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  6    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  7    JMP       0 2              ; to 10             {AstHelper.TextRange}\n" +
          $"  8    LOADK     0 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  9    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  10   JMP       0 2              ; to 13             {AstHelper.TextRange}\n" +
          $"  11   LOADK     0 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  12   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  13   RETURN    0 0                                  \n"
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
          $"  1    LOADK     0 -1             ; 0                 {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  3    LOADK     0 -1             ; 0                 {AstHelper.TextRange}\n" +
          $"  4    SETGLOB   0 1                                  {AstHelper.TextRange}\n" +
          $"  5    GETGLOB   0 1                                  {AstHelper.TextRange}\n" +
          $"  6    LE        1 0 -2           ; 10                {AstHelper.TextRange}\n" +
          $"  7    JMP       0 8              ; to 16             {AstHelper.TextRange}\n" +
          $"  8    GETGLOB   1 0                                  {AstHelper.TextRange}\n" +
          $"  9    GETGLOB   2 1                                  {AstHelper.TextRange}\n" +
          $"  10   ADD       0 1 2                                {AstHelper.TextRange}\n" +
          $"  11   SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  12   GETGLOB   1 1                                  {AstHelper.TextRange}\n" +
          $"  13   ADD       0 1 -3           ; 1                 {AstHelper.TextRange}\n" +
          $"  14   SETGLOB   0 1                                  {AstHelper.TextRange}\n" +
          $"  15   JMP       0 -11            ; to 5              {AstHelper.TextRange}\n" +
          $"  16   GETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  17   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  18   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileFuncCall() {
      string eval = "eval";
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.FuncDef(eval, AstHelper.Params(a, b),
                          AstHelper.Return(AstHelper.Binary(AstHelper.Id(a), BinaryOperator.Add,
                                                            AstHelper.Id(b)))),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(eval), AstHelper.NumberConstant(1),
                                                AstHelper.NumberConstant(2)))
      );
      var compiler = new Compiler();
      var func = compiler.Compile(block, _env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <eval>       {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  3    GETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  4    LOADK     1 -2             ; 1                 {AstHelper.TextRange}\n" +
          $"  5    LOADK     2 -3             ; 2                 {AstHelper.TextRange}\n" +
          $"  6    CALL      0 2 0                                {AstHelper.TextRange}\n" +
          $"  7    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  8    RETURN    0 0                                  \n" +
          $"\n" +
          $"Function <eval>\n" +
          $"  1    ADD       2 0 1                                {AstHelper.TextRange}\n" +
          $"  2    RETURN    2 1                                  {AstHelper.TextRange}\n" +
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
          $"  1    LOADK     0 -1             ; Func <sum>        {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  3    GETGLOB   0 0                                  {AstHelper.TextRange}\n" +
          $"  4    LOADK     1 -2             ; 10                {AstHelper.TextRange}\n" +
          $"  5    CALL      0 1 0                                {AstHelper.TextRange}\n" +
          $"  6    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  7    RETURN    0 0                                  \n" +
          $"\n" +
          $"Function <sum>\n" +
          $"  1    EQ        1 0 -1           ; 1                 {AstHelper.TextRange}\n" +
          $"  2    JMP       0 3              ; to 6              {AstHelper.TextRange}\n" +
          $"  3    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  4    RETURN    1 1                                  {AstHelper.TextRange}\n" +
          $"  5    JMP       0 5              ; to 11             {AstHelper.TextRange}\n" +
          $"  6    GETGLOB   2 0                                  {AstHelper.TextRange}\n" +
          $"  7    SUB       3 0 -1           ; 1                 {AstHelper.TextRange}\n" +
          $"  8    CALL      2 1 0                                {AstHelper.TextRange}\n" +
          $"  9    ADD       1 0 2                                {AstHelper.TextRange}\n" +
          $"  10   RETURN    1 1                                  {AstHelper.TextRange}\n" +
          $"  11   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileEmptyList() {
      var list = AstHelper.ExpressionStmt(AstHelper.List());
      var compiler = new Compiler();
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(list, env);
      string expected = (
          $"Function <main>\n" +
          $"  1    NEWLIST   0 0 0                                {AstHelper.TextRange}\n" +
          $"  2    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  3    RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    [Fact]
    public void TestCompileList() {
      var list = AstHelper.ExpressionStmt(AstHelper.List(AstHelper.NumberConstant(1),
                                                         AstHelper.NumberConstant(2),
                                                         AstHelper.NumberConstant(3)));
      var compiler = new Compiler();
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(list, env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     2 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    LOADK     3 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  4    NEWLIST   0 1 3                                {AstHelper.TextRange}\n" +
          $"  5    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  6    RETURN    0 0                                  \n"
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
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(subscript, env);
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     2 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     3 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    LOADK     4 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  4    NEWLIST   1 2 3                                {AstHelper.TextRange}\n" +
          $"  5    GETELEM   0 1 -4           ; 0                 {AstHelper.TextRange}\n" +
          $"  6    EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  7    RETURN    0 0                                  \n"
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
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(program, env);
      int firstGlobalId = NativeFunctions.Funcs.Length;
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     2 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    LOADK     3 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  4    NEWLIST   0 1 3                                {AstHelper.TextRange}\n" +
          $"  5    SETGLOB   0 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  6    GETGLOB   0 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  7    SETELEM   0 -1 -4          ; 1 5               {AstHelper.TextRange}\n" +
          $"  8    GETGLOB   1 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  9    GETELEM   0 1 -1           ; 1                 {AstHelper.TextRange}\n" +
          $"  10   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  11   RETURN    0 0                                  \n"
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
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(program, env);
      int firstGlobalId = NativeFunctions.Funcs.Length;
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     2 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    LOADK     3 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  4    NEWLIST   0 1 3                                {AstHelper.TextRange}\n" +
          $"  5    SETGLOB   0 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  6    GETGLOB   1 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  7    GETELEM   0 1 -1           ; 1                 {AstHelper.TextRange}\n" +
          $"  8    GETGLOB   2 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  9    GETELEM   1 2 -4           ; 0                 {AstHelper.TextRange}\n" +
          $"  10   GETGLOB   2 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  11   SETELEM   2 -4 0           ; 0                 {AstHelper.TextRange}\n" +
          $"  12   GETGLOB   3 {firstGlobalId}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  13   SETELEM   3 -1 1           ; 1                 {AstHelper.TextRange}\n" +
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
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(program, env);
      int startOfGlobalVariables = NativeFunctions.Funcs.Length;
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     1 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     2 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    LOADK     3 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  4    NEWLIST   0 1 3                                {AstHelper.TextRange}\n" +
          $"  5    LOADK     1 -4             ; 0                 {AstHelper.TextRange}\n" +
          $"  6    LEN       2 0 0                                {AstHelper.TextRange}\n" +
          $"  7    LOADK     3 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  8    FORPREP   1 4              ; to 13             {AstHelper.TextRange}\n" +
          $"  9    GETELEM   4 0 1                                {AstHelper.TextRange}\n" +
          $"  10   SETGLOB   4 {startOfGlobalVariables}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  11   GETGLOB   4 {startOfGlobalVariables}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  12   EVAL      4                                    {AstHelper.TextRange}\n" +
          $"  13   FORLOOP   1 -5             ; to 9              {AstHelper.TextRange}\n" +
          $"  14   RETURN    0 0                                  \n"
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
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var func = compiler.Compile(program, env);
      int startOfGlobalVariables = NativeFunctions.Funcs.Length;
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <func>       {AstHelper.TextRange}\n" +
          $"  2    SETGLOB   0 {startOfGlobalVariables}" +
          $"                                  {AstHelper.TextRange}\n" +
          $"  3    RETURN    0 0                                  \n" +
          $"\n" +
          $"Function <func>\n" +
          $"  1    LOADK     2 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  2    LOADK     3 -2             ; 2                 {AstHelper.TextRange}\n" +
          $"  3    LOADK     4 -3             ; 3                 {AstHelper.TextRange}\n" +
          $"  4    NEWLIST   1 2 3                                {AstHelper.TextRange}\n" +
          $"  5    LOADK     2 -4             ; 0                 {AstHelper.TextRange}\n" +
          $"  6    LEN       3 1 0                                {AstHelper.TextRange}\n" +
          $"  7    LOADK     4 -1             ; 1                 {AstHelper.TextRange}\n" +
          $"  8    FORPREP   2 2              ; to 11             {AstHelper.TextRange}\n" +
          $"  9    GETELEM   0 1 2                                {AstHelper.TextRange}\n" +
          $"  10   EVAL      0                                    {AstHelper.TextRange}\n" +
          $"  11   FORLOOP   2 -3             ; to 9              {AstHelper.TextRange}\n" +
          $"  12   RETURN    0 0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }
  }
}
