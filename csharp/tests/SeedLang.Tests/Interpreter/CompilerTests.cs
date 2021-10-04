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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    private static TextRange _testTextRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestCompileEvalNumber() {
      var number = Expression.Number(1, _testTextRange);
      var eval = Statement.Eval(number, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);
      string expected = (
          "LOADK 0 250         ; 1\n" +
          "EVAL 0              \n" +
          "RETURN 0            \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, chunk.ToString());
    }

    [Fact]
    public void TestCompileEvalBinary() {
      var left = Expression.Number(1, _testTextRange);
      var right = Expression.Number(2, _testTextRange);
      var expr = Expression.Binary(left, BinaryOperator.Add, right, _testTextRange);
      var eval = Statement.Eval(expr, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);
      string expected = (
          "ADD 0 250 251       \n" +
          "EVAL 0              \n" +
          "RETURN 0            \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, chunk.ToString());
    }

    [Fact]
    public void TestCompileEvalComplexBinary() {
      var left = Expression.Number(1, _testTextRange);
      var number2 = Expression.Number(2, _testTextRange);
      var number3 = Expression.Number(3, _testTextRange);
      var right = Expression.Binary(number2, BinaryOperator.Add, number3, _testTextRange);
      var expr = Expression.Binary(left, BinaryOperator.Subtract, right, _testTextRange);
      var eval = Statement.Eval(expr, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);
      string expected = (
          "ADD 1 251 252       \n" +
          "SUB 0 250 1         \n" +
          "EVAL 0              \n" +
          "RETURN 0            \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, chunk.ToString());
    }

    [Fact]
    public void TestCompileinaryWithSameConstants() {
      var left = Expression.Number(1, _testTextRange);
      var number1 = Expression.Number(1, _testTextRange);
      var number2 = Expression.Number(2, _testTextRange);
      var right = Expression.Binary(number1, BinaryOperator.Add, number2, _testTextRange);
      var expr = Expression.Binary(left, BinaryOperator.Subtract, right, _testTextRange);
      var eval = Statement.Eval(expr, _testTextRange);
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);
      string expected = (
          "ADD 1 250 251       \n" +
          "SUB 0 250 1         \n" +
          "EVAL 0              \n" +
          "RETURN 0            \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, chunk.ToString());
    }
  }
}
