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

namespace SeedLang.Ast.Tests {
  public class StatementsTests {
    internal class TestData : TheoryData<Statement, string> {
      public TestData() {
        AddAssignment();
        AddEmptyBlock();
        AddExpression();
        AddForIn();
        AddFuncDef();
        AddIf();
        AddPass();
        AddReturn();
        AddWhile();
      }

      private void AddAssignment() {
        string name = "id";
        var assignment = AstHelper.Assign(
          AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(name))),
          AstHelper.NumberConstant(1)
        );
        var expectedOutput = (
            $"{AstHelper.TextRange} AssignmentStatement\n" +
            $"  {AstHelper.TextRange} IdentifierExpression ({name}) =\n" +
            $"  {AstHelper.TextRange} NumberConstantExpression (1)"
        ).Replace("\n", Environment.NewLine);
        Add(assignment, expectedOutput);
      }

      private void AddEmptyBlock() {
        var block = AstHelper.Block(Array.Empty<Statement>());
        var expectedOutput = $"{AstHelper.TextRange} BlockStatement";
        Add(block, expectedOutput);
      }

      private void AddExpression() {
        var expr = AstHelper.ExpressionStmt(AstHelper.Binary(
          AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                           AstHelper.NumberConstant(2)),
          BinaryOperator.Multiply,
          AstHelper.NumberConstant(3)
        ));
        var expectedOutput = (
            $"{AstHelper.TextRange} ExpressionStatement\n" +
            $"  {AstHelper.TextRange} BinaryExpression (*)\n" +
            $"    {AstHelper.TextRange} BinaryExpression (+)\n" +
            $"      {AstHelper.TextRange} NumberConstantExpression (1)\n" +
            $"      {AstHelper.TextRange} NumberConstantExpression (2)\n" +
            $"    {AstHelper.TextRange} NumberConstantExpression (3)"
        ).Replace("\n", Environment.NewLine);
        Add(expr, expectedOutput);
      }

      private void AddForIn() {
        var forIn = AstHelper.ForIn(
          AstHelper.Id("a"),
          AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2)),
          AstHelper.Block(Array.Empty<Statement>())
        );
        var expectedOutput = (
            $"{AstHelper.TextRange} ForInStatement\n" +
            $"  {AstHelper.TextRange} IdentifierExpression (a)\n" +
            $"  {AstHelper.TextRange} ListExpression\n" +
            $"    {AstHelper.TextRange} NumberConstantExpression (1)\n" +
            $"    {AstHelper.TextRange} NumberConstantExpression (2)\n" +
            $"  {AstHelper.TextRange} BlockStatement"
        ).Replace("\n", Environment.NewLine);
        Add(forIn, expectedOutput);
      }

      private void AddFuncDef() {
        var funcDef = AstHelper.FuncDef("func", AstHelper.Params("arg1", "arg2"),
                                        AstHelper.Block(Array.Empty<Statement>()));
        var expectedOutput = (
            $"{AstHelper.TextRange} FuncDefStatement (func:arg1,arg2)\n" +
            $"  {AstHelper.TextRange} BlockStatement"
        ).Replace("\n", Environment.NewLine);
        Add(funcDef, expectedOutput);
      }

      private void AddIf() {
        string name = "id";
        var @if = AstHelper.If(
          AstHelper.BooleanConstant(false),
          AstHelper.Assign(AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(name))),
                           AstHelper.NumberConstant(1)),
          AstHelper.Assign(AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(name))),
                           AstHelper.NumberConstant(2))
        );
        var expectedOutput = (
            $"{AstHelper.TextRange} IfStatement\n" +
            $"  {AstHelper.TextRange} BooleanConstantExpression (False)\n" +
            $"  {AstHelper.TextRange} AssignmentStatement\n" +
            $"    {AstHelper.TextRange} IdentifierExpression ({name}) =\n" +
            $"    {AstHelper.TextRange} NumberConstantExpression (1)\n" +
            $"  {AstHelper.TextRange} AssignmentStatement\n" +
            $"    {AstHelper.TextRange} IdentifierExpression ({name}) =\n" +
            $"    {AstHelper.TextRange} NumberConstantExpression (2)"
        ).Replace("\n", Environment.NewLine);
        Add(@if, expectedOutput);
      }

      private void AddPass() {
        var pass = AstHelper.Pass();
        var expectedOutput = $"{AstHelper.TextRange} PassStatement";
        Add(pass, expectedOutput);
      }

      private void AddReturn() {
        var ret = AstHelper.Return(AstHelper.NumberConstant(1));
        var expectedOutput = (
            $"{AstHelper.TextRange} ReturnStatement\n" +
            $"  {AstHelper.TextRange} NumberConstantExpression (1)"
        ).Replace("\n", Environment.NewLine);
        Add(ret, expectedOutput);
      }

      private void AddWhile() {
        string name = "id";
        var @while = AstHelper.While(
          AstHelper.BooleanConstant(true),
          AstHelper.Assign(AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(name))),
                           AstHelper.NumberConstant(1))
        );
        var expectedOutput = (
            $"{AstHelper.TextRange} WhileStatement\n" +
            $"  {AstHelper.TextRange} BooleanConstantExpression (True)\n" +
            $"  {AstHelper.TextRange} AssignmentStatement\n" +
            $"    {AstHelper.TextRange} IdentifierExpression ({name}) =\n" +
            $"    {AstHelper.TextRange} NumberConstantExpression (1)"
        ).Replace("\n", Environment.NewLine);
        Add(@while, expectedOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestStatement(Statement statement, string expectedOutput) {
      Assert.Equal(expectedOutput, statement.ToString());
    }
  }
}
