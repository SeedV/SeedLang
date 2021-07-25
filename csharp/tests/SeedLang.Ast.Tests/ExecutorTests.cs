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

using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExecutorTests {
    [Fact]
    public void TestExecuteNumberExpression() {
      double value = 1.5;
      var number = Expression.Number(value);
      var executor = new Executor();
      Assert.Equal(value, executor.Run(number).ToNumber());
    }

    [Fact]
    public void TestExecuteBinaryExpression() {
      var left = Expression.Number(1);
      var right = Expression.Number(2);
      var binary = Expression.Binary(left, BinaryOperator.Add, right);
      var executor = new Executor();
      Assert.Equal(3, executor.Run(binary).ToNumber());
    }

    [Fact]
    public void TestExecuteEvalStatement() {
      var one = Expression.Number(1);
      var two = Expression.Number(2);
      var three = Expression.Number(3);
      var left = Expression.Binary(one, BinaryOperator.Add, two);
      var binary = Expression.Binary(left, BinaryOperator.Multiply, three);
      var eval = Statement.Eval(binary);
      var executor = new Executor();
      BaseValue result = null;
      executor.RegisterNativeFunc("print", value => {
        result = value;
        return null;
      });
      executor.Run(eval);
      Assert.NotNull(result);
      Assert.Equal(9, result.ToNumber());
    }
  }
}
