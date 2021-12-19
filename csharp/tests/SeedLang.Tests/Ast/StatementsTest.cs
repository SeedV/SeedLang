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
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class StatementsTests {
    internal class TestData : TheoryData<Statement, string> {
      private static BlockRange _blockRange => new BlockRange(new BlockPosition("id"));

      public TestData() {
        AddAssignmentStatement();
        AddEmptyBlockStatement();
        AddExpressionStatement();
        AddFunctionStatement();
        AddIfStatement();
        AddReturnStatement();
        AddWhileStatement();
      }

      private void AddAssignmentStatement() {
        var identifier = Expression.Identifier("id", _blockRange);
        var expr = Expression.NumberConstant(1, _blockRange);
        var assignment = Statement.Assignment(identifier, expr, _blockRange);
        var expectedOutput = $"{_blockRange} AssignmentStatement\n" +
                             $"  {_blockRange} IdentifierExpression (id)\n" +
                             $"  {_blockRange} NumberConstantExpression (1)";
        Add(assignment, expectedOutput);
      }

      private void AddEmptyBlockStatement() {
        var block = Statement.Block(Array.Empty<Statement>(), _blockRange);
        var expectedOutput = $"{_blockRange} BlockStatement";
        Add(block, expectedOutput);
      }

      private void AddExpressionStatement() {
        var one = Expression.NumberConstant(1, _blockRange);
        var two = Expression.NumberConstant(2, _blockRange);
        var three = Expression.NumberConstant(3, _blockRange);
        var left = Expression.Binary(one, BinaryOperator.Add, two, _blockRange);
        var binary = Expression.Binary(left, BinaryOperator.Multiply, three, _blockRange);
        var expr = Statement.Expression(binary, _blockRange);
        var expectedOutput = $"{_blockRange} ExpressionStatement\n" +
                             $"  {_blockRange} BinaryExpression (*)\n" +
                             $"    {_blockRange} BinaryExpression (+)\n" +
                             $"      {_blockRange} NumberConstantExpression (1)\n" +
                             $"      {_blockRange} NumberConstantExpression (2)\n" +
                             $"    {_blockRange} NumberConstantExpression (3)";
        Add(expr, expectedOutput);
      }

      private void AddFunctionStatement() {
        var arguments = new string[] { "arg1", "arg2", };
        var body = Statement.Block(Array.Empty<Statement>(), _blockRange);
        var function = Statement.Function("func", arguments, body, _blockRange);
        var expectedOutput = $"{_blockRange} FunctionStatement (func: arg1, arg2)\n" +
                             $"  {_blockRange} BlockStatement";
        Add(function, expectedOutput);
      }

      private void AddIfStatement() {
        var test = Expression.BooleanConstant(false, _blockRange);
        var identifier = Expression.Identifier("id", _blockRange);
        var one = Expression.NumberConstant(1, _blockRange);
        var two = Expression.NumberConstant(2, _blockRange);
        var thenBody = Statement.Assignment(identifier, one, _blockRange);
        var elseBody = Statement.Assignment(identifier, two, _blockRange);
        var @if = Statement.If(test, thenBody, elseBody, _blockRange);
        var expectedOutput = $"{_blockRange} IfStatement\n" +
                             $"  {_blockRange} BooleanConstantExpression (False)\n" +
                             $"  {_blockRange} AssignmentStatement\n" +
                             $"    {_blockRange} IdentifierExpression (id)\n" +
                             $"    {_blockRange} NumberConstantExpression (1)\n" +
                             $"  {_blockRange} AssignmentStatement\n" +
                             $"    {_blockRange} IdentifierExpression (id)\n" +
                             $"    {_blockRange} NumberConstantExpression (2)";
        Add(@if, expectedOutput);
      }

      private void AddReturnStatement() {
        var value = Expression.NumberConstant(1, _blockRange);
        var ret = Statement.Return(value, _blockRange);
        var expectedOutput = $"{_blockRange} ReturnStatement\n" +
                             $"  {_blockRange} NumberConstantExpression (1)";
        Add(ret, expectedOutput);
      }

      private void AddWhileStatement() {
        var test = Expression.BooleanConstant(true, _blockRange);
        var identifier = Expression.Identifier("id", _blockRange);
        var one = Expression.NumberConstant(1, _blockRange);
        var body = Statement.Assignment(identifier, one, _blockRange);
        var @while = Statement.While(test, body, _blockRange);
        var expectedOutput = $"{_blockRange} WhileStatement\n" +
                             $"  {_blockRange} BooleanConstantExpression (True)\n" +
                             $"  {_blockRange} AssignmentStatement\n" +
                             $"    {_blockRange} IdentifierExpression (id)\n" +
                             $"    {_blockRange} NumberConstantExpression (1)";
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
