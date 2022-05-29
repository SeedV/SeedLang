using System.Linq;
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
using FluentAssertions;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;
using SeedLang.Tests.Helper;
using SeedLang.Visualization;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class ExprCompilerTests {
    private static int _printValFunc => NativeFunctionIdOf(NativeFunctions.PrintVal);
    private static int _sliceFunc => NativeFunctionIdOf(NativeFunctions.Slice);
    private readonly TextRange _range = AstHelper.TextRange;

    [Fact]
    public void TestCompileNilConstant() {
      var program = AstHelper.ExpressionStmt(AstHelper.NilConstant());
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADNIL   1 1 0                                {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileNumberConstant() {
      var program = AstHelper.ExpressionStmt(AstHelper.NumberConstant(1));
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileComparison() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.Less),
                             AstHelper.NumberConstant(2))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LT        1 -1 -2          ; 1 2               {_range}\n" +
          $"  3    JMP       0 1              ; to 5              {_range}\n" +
          $"  4    LOADBOOL  1 1 1                                {_range}\n" +
          $"  5    LOADBOOL  1 0 0                                {_range}\n" +
          $"  6    CALL      0 1 0                                {_range}\n" +
          $"  7    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileInComparison() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.In),
                             AstHelper.Tuple(AstHelper.NumberConstant(1),
                                             AstHelper.NumberConstant(2)))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADK     3 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     4 -2             ; 2                 {_range}\n" +
          $"  4    NEWTUPLE  2 3 2                                {_range}\n" +
          $"  5    IN        1 -1 2           ; 1                 {_range}\n" +
          $"  6    JMP       0 1              ; to 8              {_range}\n" +
          $"  7    LOADBOOL  1 1 1                                {_range}\n" +
          $"  8    LOADBOOL  1 0 0                                {_range}\n" +
          $"  9    CALL      0 1 0                                {_range}\n" +
          $"  10   HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileNilComparison() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Comparison(AstHelper.NilConstant(),
                             AstHelper.CompOps(ComparisonOperator.Less),
                             AstHelper.NilConstant())
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADNIL   2 1 0                                {_range}\n" +
          $"  3    LOADNIL   3 1 0                                {_range}\n" +
          $"  4    LT        1 2 3                                {_range}\n" +
          $"  5    JMP       0 1              ; to 7              {_range}\n" +
          $"  6    LOADBOOL  1 1 1                                {_range}\n" +
          $"  7    LOADBOOL  1 0 0                                {_range}\n" +
          $"  8    CALL      0 1 0                                {_range}\n" +
          $"  9    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileBoolean() {
      var program = AstHelper.ExpressionStmt(
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
        )
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LT        1 -1 -2          ; 1 2               {_range}\n" +
          $"  3    JMP       0 2              ; to 6              {_range}\n" +
          $"  4    LT        0 -2 -3          ; 2 3               {_range}\n" +
          $"  5    JMP       0 4              ; to 10             {_range}\n" +
          $"  6    LE        1 -1 -2          ; 1 2               {_range}\n" +
          $"  7    JMP       0 3              ; to 11             {_range}\n" +
          $"  8    LE        1 -2 -3          ; 2 3               {_range}\n" +
          $"  9    JMP       0 1              ; to 11             {_range}\n" +
          $"  10   LOADBOOL  1 1 1                                {_range}\n" +
          $"  11   LOADBOOL  1 0 0                                {_range}\n" +
          $"  12   CALL      0 1 0                                {_range}\n" +
          $"  13   HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileBinary() {
      var program = AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.NumberConstant(1),
                                                              BinaryOperator.Add,
                                                              AstHelper.NumberConstant(2)));
      string expected = (
          $"Function <main>\n" +
          $"  1    ADD       0 -1 -2          ; 1 2               {_range}\n" +
          $"  2    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Script);
    }

    [Fact]
    public void TestCompileComplexBinary() {
      var program = AstHelper.ExpressionStmt(AstHelper.Binary(
        AstHelper.NumberConstant(1),
        BinaryOperator.Subtract,
        AstHelper.Binary(AstHelper.NumberConstant(2), BinaryOperator.Add,
                         AstHelper.NumberConstant(3)))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    ADD       2 -2 -3          ; 2 3               {_range}\n" +
          $"  3    SUB       1 -1 2           ; 1                 {_range}\n" +
          $"  4    CALL      0 1 0                                {_range}\n" +
          $"  5    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileBinaryWithSameConstants() {
      var program = AstHelper.ExpressionStmt(AstHelper.Binary(
        AstHelper.NumberConstant(1),
        BinaryOperator.Subtract,
        AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                         AstHelper.NumberConstant(2)))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    ADD       2 -1 -2          ; 1 2               {_range}\n" +
          $"  3    SUB       1 -1 2           ; 1                 {_range}\n" +
          $"  4    CALL      0 1 0                                {_range}\n" +
          $"  5    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileUnary() {
      var program = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
                                                             AstHelper.NumberConstant(1)));
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    UNM       1 -1             ; 1                 {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileEmptyList() {
      var program = AstHelper.ExpressionStmt(AstHelper.List());
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    NEWLIST   1 0 0                                {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileList() {
      var program = AstHelper.ExpressionStmt(AstHelper.List(AstHelper.NumberConstant(1),
                                                         AstHelper.NumberConstant(2),
                                                         AstHelper.NumberConstant(3)));
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADK     2 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     3 -2             ; 2                 {_range}\n" +
          $"  4    LOADK     4 -3             ; 3                 {_range}\n" +
          $"  5    NEWLIST   1 2 3                                {_range}\n" +
          $"  6    CALL      0 1 0                                {_range}\n" +
          $"  7    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileEmptyDict() {
      var program = AstHelper.ExpressionStmt(AstHelper.Dict());
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    NEWDICT   1 0 0                                {_range}\n" +
          $"  3    CALL      0 1 0                                {_range}\n" +
          $"  4    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileDict() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Dict(
          AstHelper.KeyValue(AstHelper.NumberConstant(1), AstHelper.NumberConstant(1)),
          AstHelper.KeyValue(AstHelper.NumberConstant(2), AstHelper.NumberConstant(2))
        )
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADK     2 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     3 -1             ; 1                 {_range}\n" +
          $"  4    LOADK     4 -2             ; 2                 {_range}\n" +
          $"  5    LOADK     5 -2             ; 2                 {_range}\n" +
          $"  6    NEWDICT   1 2 4                                {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileSubscript() {
      var program = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(0)
      ));
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADK     3 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     4 -2             ; 2                 {_range}\n" +
          $"  4    LOADK     5 -3             ; 3                 {_range}\n" +
          $"  5    NEWLIST   2 3 3                                {_range}\n" +
          $"  6    GETELEM   1 2 -4           ; 0                 {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileSubscriptSlice() {
      var program = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.Slice(
          AstHelper.NumberConstant(1),
          AstHelper.NumberConstant(2),
          AstHelper.NumberConstant(1)
        )
      ));
      string expected = (
          $"Function <main>\n" +
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    LOADK     3 -1             ; 1                 {_range}\n" +
          $"  3    LOADK     4 -2             ; 2                 {_range}\n" +
          $"  4    LOADK     5 -3             ; 3                 {_range}\n" +
          $"  5    NEWLIST   2 3 3                                {_range}\n" +
          $"  6    GETGLOB   3 {_sliceFunc}                                  {_range}\n" +
          $"  7    LOADK     4 -1             ; 1                 {_range}\n" +
          $"  8    LOADK     5 -2             ; 2                 {_range}\n" +
          $"  9    LOADK     6 -1             ; 1                 {_range}\n" +
          $"  10   CALL      3 3 0                                {_range}\n" +
          $"  11   GETELEM   1 2 3                                {_range}\n" +
          $"  12   CALL      0 1 0                                {_range}\n" +
          $"  13   HALT      1 0                                  {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestUndefinedVariable() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Id("a"));
      Action action = () => TestCompiler(expr, "", RunMode.Interactive);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorVariableNotDefined);
    }

    private static void TestCompiler(Statement statement, string expected, RunMode mode) {
      var env = new GlobalEnvironment(NativeFunctions.Funcs.Values);
      var vc = new VisualizerCenter();
      var compiler = new Compiler();
      var func = compiler.Compile(statement, env, vc, mode);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    private static int NativeFunctionIdOf(string name) {
      return NativeFunctions.Funcs.Values.ToList().FindIndex(func => { return func.Name == name; });
    }
  }
}
