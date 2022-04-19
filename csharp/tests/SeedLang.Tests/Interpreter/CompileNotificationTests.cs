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
          $"  4    RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.Assignment: 'name': Global 0 {_range}\n"
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
          $"  5    RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.Binary: 250 Add 251 1 {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileFuncCall() {
      string add = "add";
      string a = "a";
      string b = "b";
      var program = AstHelper.Block(
          AstHelper.FuncDef(add, AstHelper.Params(a, b),
                            AstHelper.Return(AstHelper.Binary(AstHelper.Id(a), BinaryOperator.Add,
                                                              AstHelper.Id(b)))),
          AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(add), AstHelper.NumberConstant(1),
                                                  AstHelper.NumberConstant(2)))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    LOADK     0 -1             ; Func <add>        {_range}\n" +
          $"  2    SETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  3    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  4    GETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  5    LOADK     2 -2             ; 1                 {_range}\n" +
          $"  6    LOADK     3 -3             ; 2                 {_range}\n" +
          $"  7    VISNOTIFY 0 0                                  {_range}\n" +
          $"  8    CALL      1 2 0                                {_range}\n" +
          $"  9    VISNOTIFY 1 0                                  {_range}\n" +
          $"  10   CALL      0 1 0                                {_range}\n" +
          $"  11   RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.Function: add 1 2 {_range}\n" +
          $"\n" +
          $"Function <add>\n" +
          $"  1    ADD       2 0 1                                {_range}\n" +
          $"  2    VISNOTIFY 0 0                                  {_range}\n" +
          $"  3    RETURN    2 1                                  {_range}\n" +
          $"  4    RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.Binary: 0 Add 1 2 {_range}\n"
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
          $"  5    RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.Unary: Negative 250 1 {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }


    [Fact]
    public void TestCompileVTag() {
      var vTagInfos = AstHelper.VTagInfos(AstHelper.VTagInfo("Add"));
      var program = AstHelper.VTag(vTagInfos, AstHelper.ExpressionStmt(
          AstHelper.Binary(AstHelper.NumberConstant(1),
                           BinaryOperator.Add,
                           AstHelper.NumberConstant(2))
      ));
      string expected = (
          $"Function <main>\n" +
          $"  1    VISNOTIFY 0 0                                  {_range}\n" +
          $"  2    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  3    ADD       1 -1 -2          ; 1 2               {_range}\n" +
          $"  4    VISNOTIFY 0 1                                  {_range}\n" +
          $"  5    CALL      0 1 0                                {_range}\n" +
          $"  6    VISNOTIFY 0 2                                  {_range}\n" +
          $"  7    RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.VTagEntered: Add {_range}\n" +
          $"  1    Notification.Binary: 250 Add 251 1 {_range}\n" +
          $"  2    Notification.VTagExited: Add {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileVTagWithArguments() {
      var vTagInfos = AstHelper.VTagInfos(AstHelper.VTagInfo(
          "Add",
          AstHelper.VTagArg("1", AstHelper.NumberConstant(1)),
          AstHelper.VTagArg("2", AstHelper.NumberConstant(2))
      ));
      var program = AstHelper.VTag(vTagInfos, AstHelper.ExpressionStmt(
          AstHelper.Binary(AstHelper.NumberConstant(1),
                           BinaryOperator.Add,
                           AstHelper.NumberConstant(2))
      ));
      string expected = (
          $"Function <main>\n" +
          $"  1    VISNOTIFY 0 0                                  {_range}\n" +
          $"  2    GETGLOB   0 {_printValFunc}                                  {_range}\n" +
          $"  3    ADD       1 -1 -2          ; 1 2               {_range}\n" +
          $"  4    VISNOTIFY 0 1                                  {_range}\n" +
          $"  5    CALL      0 1 0                                {_range}\n" +
          $"  6    VISNOTIFY 0 2                                  {_range}\n" +
          $"  7    RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.VTagEntered: Add(1,2) {_range}\n" +
          $"  1    Notification.Binary: 250 Add 251 1 {_range}\n" +
          $"  2    Notification.VTagExited: Add(250,251) {_range}\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(program, expected, RunMode.Interactive);
    }

    [Fact]
    public void TestCompileVTagWithComplexArguments() {
      string x = "x";
      string y = "y";
      var vTagInfos = AstHelper.VTagInfos(AstHelper.VTagInfo(
          "Assign",
          AstHelper.VTagArg("x", AstHelper.Id(x)),
          AstHelper.VTagArg("1", AstHelper.NumberConstant(1)),
          AstHelper.VTagArg("y", AstHelper.Id(y)),
          AstHelper.VTagArg("1+2", AstHelper.Binary(AstHelper.NumberConstant(1),
                                                      BinaryOperator.Add,
                                                      AstHelper.NumberConstant(2)))
      ));
      var program = AstHelper.VTag(vTagInfos,
          AstHelper.Assign(AstHelper.Targets(AstHelper.Id("x"), AstHelper.Id("y")),
                           AstHelper.NumberConstant(1),
                           AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                                            AstHelper.NumberConstant(2)))
      );
      string expected = (
          $"Function <main>\n" +
          $"  1    VISNOTIFY 0 0                                  {_range}\n" +
          $"  2    ADD       0 -1 -2          ; 1 2               {_range}\n" +
          $"  3    VISNOTIFY 0 1                                  {_range}\n" +
          $"  4    LOADK     1 -1             ; 1                 {_range}\n" +
          $"  5    SETGLOB   1 {_firstGlob}                                  {_range}\n" +
          $"  6    VISNOTIFY 0 2                                  {_range}\n" +
          $"  7    SETGLOB   0 {_firstGlob + 1}                                  {_range}\n" +
          $"  8    VISNOTIFY 0 3                                  {_range}\n" +
          $"  9    GETGLOB   0 {_firstGlob}                                  {_range}\n" +
          $"  10   GETGLOB   1 {_firstGlob + 1}                                  {_range}\n" +
          $"  11   ADD       2 -1 -2          ; 1 2               {_range}\n" +
          $"  12   VISNOTIFY 0 4                                  {_range}\n" +
          $"  13   VISNOTIFY 0 5                                  {_range}\n" +
          $"  14   RETURN    0 0                                  {_range}\n" +
          $"Notifications\n" +
          $"  0    Notification.VTagEntered: Assign(x,1,y,1+2) {_range}\n" +
          $"  1    Notification.Binary: 250 Add 251 0 {_range}\n" +
          $"  2    Notification.Assignment: 'x': Global 1 {_range}\n" +
          $"  3    Notification.Assignment: 'y': Global 0 {_range}\n" +
          $"  4    Notification.Binary: 250 Add 251 2 {_range}\n" +
          $"  5    Notification.VTagExited: Assign(0,250,1,2) {_range}\n"
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
