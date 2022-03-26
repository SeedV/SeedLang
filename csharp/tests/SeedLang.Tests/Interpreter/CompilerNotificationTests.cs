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
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;
using static SeedLang.Runtime.HeapObject;

namespace SeedLang.Interpreter.Tests {
  public class CompileNotificationTests {
    private static int _printValFunc =>
        Array.FindIndex(NativeFunctions.Funcs, (NativeFunction func) => {
          return func.Name == NativeFunctions.PrintVal;
        });
    private readonly int _firstGlob = NativeFunctions.Funcs.Length;
    private readonly Common.Range _range = AstHelper.TextRange;

    [Fact]
    public void TestCompileAssignment() {
      var program = AstHelper.Assign(AstHelper.Targets(AstHelper.Id("name")),
                                                       AstHelper.NumberConstant(1));
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 1                 {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    VISNOTIFY 0 0                                  {_range}\n" +
          $"  4    RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    AssignmentNotification: 'name': Global 0 {_range}\n"
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
          $"  1    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  2    ADD       1 -1 -2          ; 1 2               {_range}\n" +
          $"  3    VISNOTIFY 0 0                                  {_range}\n" +
          $"  4    CALL      0 1 0                                {_range}\n" +
          $"  5    RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    BinaryNotification: 250 Add 251 1 {_range}\n"
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
          $"  6    VISNOTIFY 0 0                                  {_range}\n" +
          $"  7    CALL      0 1 0                                {_range}\n" +
          $"  8    RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    ComparisonNotification: 250 Less 251 1 {_range}\n"
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
          $"  3    VISNOTIFY 0 0                                  {_range}\n" +
          $"  4    CALL      0 1 0                                {_range}\n" +
          $"  5    RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    UnaryNotification: Negative 250 1 {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileIfWithNullElse() {
      var program = AstHelper.If(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        null
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    EQ        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 5              ; to 8              {_range}\n" +
          $"  3    VISNOTIFY 0 0                                  {_range}\n" +
          $"  4    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  5    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  6    CALL      0 1 0                                {_range}\n" +
          $"  7    JMP       0 1              ; to 9              {_range}\n" +
          $"  8    VISNOTIFY 0 1                                  {_range}\n" +
          $"  9    RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    ComparisonNotification: 250 EqEqual 251 True {_range}\n" +
          $"  1    ComparisonNotification: 250 EqEqual 251 False {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileIfMultipleComparison() {
      var program = AstHelper.If(
        AstHelper.Comparison(
          AstHelper.NumberConstant(1),
          AstHelper.CompOps(ComparisonOperator.Less, ComparisonOperator.Less),
          AstHelper.NumberConstant(2),
          AstHelper.NumberConstant(3)
        ),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    LT        1 -1 -2          ; 1 2               {_range}\n" +
          $"  2    JMP       0 7              ; to 10             {_range}\n" +
          $"  3    LT        1 -2 -3          ; 2 3               {_range}\n" +
          $"  4    JMP       0 5              ; to 10             {_range}\n" +
          $"  5    VISNOTIFY 0 0                                  {_range}\n" +
          $"  6    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  7    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  8    CALL      0 1 0                                {_range}\n" +
          $"  9    JMP       0 4              ; to 14             {_range}\n" +
          $"  10   VISNOTIFY 0 1                                  {_range}\n" +
          $"  11   GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  12   LOADK     1 -2             ; 2                 {_range}\n" +
          $"  13   CALL      0 1 0                                {_range}\n" +
          $"  14   RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    ComparisonNotification: 250 Less 251 Less 252 True {_range}\n" +
          $"  1    ComparisonNotification: 250 Less 251 Less 252 False {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
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
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; 0                 {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    VISNOTIFY 0 0                                  {_range}\n" +
          $"  4    LOADK     0 -1             ; 0                 {_range}\n" +
          $"  5    SETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  6    VISNOTIFY 0 1                                  {_range}\n" +
          $"  7    GETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  8    LE        1 0 -2           ; 10                {_range}\n" +
          $"  9    JMP       0 13             ; to 23             {_range}\n" +
          $"  10   VISNOTIFY 0 2                                  {_range}\n" +
          $"  11   GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  12   GETGLOB   2 {_firstGlob + 1}                                  {_range}\n" +
          $"  13   ADD       0 1 2                                {_range}\n" +
          $"  14   VISNOTIFY 0 3                                  {_range}\n" +
          $"  15   SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  16   VISNOTIFY 0 4                                  {_range}\n" +
          $"  17   GETGLOB   1 {_firstGlob + 1}                                  {_range}\n" +
          $"  18   ADD       0 1 -3           ; 1                 {_range}\n" +
          $"  19   VISNOTIFY 0 5                                  {_range}\n" +
          $"  20   SETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  21   VISNOTIFY 0 6                                  {_range}\n" +
          $"  22   JMP       0 -16            ; to 7              {_range}\n" +
          $"  23   VISNOTIFY 0 7                                  {_range}\n" +
          $"  24   GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  25   GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  26   CALL      0 1 0                                {_range}\n" +
          $"  27   RETURN    0 0                                  \n" +
          $"Notifications\n" +
          $"  0    AssignmentNotification: 'sum': Global 0 {_range}\n" +
          $"  1    AssignmentNotification: 'i': Global 0 {_range}\n" +
          $"  2    ComparisonNotification: 0 LessEqual 251 True {_range}\n" +
          $"  3    BinaryNotification: 1 Add 2 0 {_range}\n" +
          $"  4    AssignmentNotification: 'sum': Global 0 {_range}\n" +
          $"  5    BinaryNotification: 1 Add 252 0 {_range}\n" +
          $"  6    AssignmentNotification: 'i': Global 0 {_range}\n" +
          $"  7    ComparisonNotification: 0 LessEqual 251 False {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    private static void TestCompiler(AstNode node, string expected, RunMode mode) {
      var env = new GlobalEnvironment(NativeFunctions.Funcs);
      var vc = new VisualizerCenter();
      var visualizer = new MockupVisualizer();
      vc.Register(visualizer);
      var compiler = new Compiler();
      var func = compiler.Compile(node, env, vc, mode);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }
  }
}
