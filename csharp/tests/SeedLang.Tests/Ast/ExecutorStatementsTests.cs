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

using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExecutorStatementsTests {
    internal class TestData : TheoryData<Statement, string> {
      private static TextRange _textRange => new TextRange(0, 1, 2, 3);

      public TestData() {
        AddAssignment();
        AddBlock();
        AddExpression();
        AddIf();
        AddWhile();
      }

      private void AddAssignment() {
        string name = "id";
        var identifier = Expression.Identifier(name, _textRange);
        var one = Expression.NumberConstant(1, _textRange);
        var assignment = Statement.Assignment(identifier, one, _textRange);
        var expectedOutput = $"{_textRange} {name} = 1\n";
        Add(assignment, expectedOutput);
      }

      private void AddBlock() {
        string name = "id";
        var identifier = Expression.Identifier(name, _textRange);
        var one = Expression.NumberConstant(1, _textRange);
        var assignment = Statement.Assignment(identifier, one, _textRange);
        var two = Expression.NumberConstant(2, _textRange);
        var binary = Expression.Binary(identifier, BinaryOperator.Add, two, _textRange);
        var expr = Statement.Expression(binary, _textRange);
        var block = Statement.Block(new Statement[] { assignment, expr }, _textRange);
        var expectedOutput = $"{_textRange} {name} = 1\n" +
                             $"{_textRange} 1 Add 2 = 3\n" +
                             $"{_textRange} Eval 3\n";
        Add(block, expectedOutput);
      }

      private void AddExpression() {
        var one = Expression.NumberConstant(1, _textRange);
        var two = Expression.NumberConstant(2, _textRange);
        var three = Expression.NumberConstant(3, _textRange);
        var left = Expression.Binary(one, BinaryOperator.Add, two, _textRange);
        var binary = Expression.Binary(left, BinaryOperator.Multiply, three, _textRange);
        var expr = Statement.Expression(binary, _textRange);
        var expectedOutput = $"{_textRange} 3 Multiply 3 = 9\n" +
                             $"{_textRange} Eval 9\n";
        Add(expr, expectedOutput);
      }

      private void AddIf() {
        var @true = Expression.BooleanConstant(true, _textRange);
        var @false = Expression.BooleanConstant(false, _textRange);
        var one = Statement.Expression(Expression.NumberConstant(1, _textRange), _textRange);
        var two = Statement.Expression(Expression.NumberConstant(2, _textRange), _textRange);
        var ifTrue = Statement.If(@true, one, two, _textRange);
        var expectedTrueOutput = $"{_textRange} Eval 1\n";
        Add(ifTrue, expectedTrueOutput);
        var ifFalse = Statement.If(@false, one, two, _textRange);
        var expectedFalseOutput = $"{_textRange} Eval 2\n";
        Add(ifFalse, expectedFalseOutput);
      }

      private void AddWhile() {
        var sum = Expression.Identifier("sum", _textRange);
        var i = Expression.Identifier("i", _textRange);
        var zero = Expression.NumberConstant(0, _textRange);
        var one = Expression.NumberConstant(1, _textRange);
        var initialSum = Statement.Assignment(sum, zero, _textRange);
        var initialI = Statement.Assignment(i, zero, _textRange);
        var ops = new ComparisonOperator[] { ComparisonOperator.LessEqual };
        var exprs = new Expression[] { Expression.NumberConstant(10, _textRange) };
        var test = Expression.Comparison(i, ops, exprs, _textRange);
        var addSum = Expression.Binary(sum, BinaryOperator.Add, i, _textRange);
        var assignSum = Statement.Assignment(sum, addSum, _textRange);
        var addI = Expression.Binary(i, BinaryOperator.Add, one, _textRange);
        var assignI = Statement.Assignment(i, addI, _textRange);
        var body = Statement.Block(new Statement[] { assignSum, assignI }, _textRange);
        var @while = Statement.While(test, body, _textRange);
        var evalSum = Statement.Expression(sum, _textRange);
        var program = Statement.Block(new Statement[] { initialSum, initialI, @while, evalSum },
                                      _textRange);
        var expectedOutput = $"{_textRange} i = 11\n" +
                             $"{_textRange} 10 Add 1 = 11\n" +
                             $"{_textRange} 11 LessEqual 10 = False\n" +
                             $"{_textRange} Eval 55\n";
        Add(program, expectedOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestStatement(Statement statement, string expectedOutput) {
      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(statement);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    private static (Executor, MockupVisualizer) NewExecutorWithVisualizer() {
      var visualizer = new MockupVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(visualizer);
      var executor = new Executor(visualizerCenter);
      return (executor, visualizer);
    }
  }
}
