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
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    [Fact]
    public void TestCompileEvalNumber() {
      var eval = Statement.Eval(Expression.Number(1));
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
      var left = Expression.Number(1);
      var right = Expression.Number(2);
      var eval = Statement.Eval(Expression.Binary(left, BinaryOperator.Add, right));
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
      var left = Expression.Number(1);
      var right = Expression.Binary(Expression.Number(2), BinaryOperator.Add, Expression.Number(3));
      var eval = Statement.Eval(Expression.Binary(left, BinaryOperator.Subtract, right));
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
      var left = Expression.Number(1);
      var right = Expression.Binary(Expression.Number(2), BinaryOperator.Add, Expression.Number(1));
      var eval = Statement.Eval(Expression.Binary(left, BinaryOperator.Subtract, right));
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);
      string expected = (
          "ADD 1 251 250       \n" +
          "SUB 0 250 1         \n" +
          "EVAL 0              \n" +
          "RETURN 0            \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, chunk.ToString());
    }
  }
}
