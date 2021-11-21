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
  public class ExecutorExpresionsTests {
    internal class TestData : TheoryData<Expression, string> {
      private static TextRange _textRange => new TextRange(0, 1, 2, 3);

      public TestData() {
        AddBinary();
        AddBoolean();
        AddComparison();
        AddUnary();
      }

      private void AddBinary() {
        var left = Expression.NumberConstant(1, _textRange);
        var right = Expression.NumberConstant(2, _textRange);
        var add = Expression.Binary(left, BinaryOperator.Add, right, _textRange);
        var expectedAddOutput = $"{_textRange} 1 Add 2 = 3\n";
        Add(add, expectedAddOutput);
        var sub = Expression.Binary(left, BinaryOperator.Subtract, right, _textRange);
        var expectedSubOutput = $"{_textRange} 1 Subtract 2 = -1\n";
        Add(sub, expectedSubOutput);
        var mul = Expression.Binary(left, BinaryOperator.Multiply, right, _textRange);
        var expectedMulOutput = $"{_textRange} 1 Multiply 2 = 2\n";
        Add(mul, expectedMulOutput);
        var div = Expression.Binary(left, BinaryOperator.Divide, right, _textRange);
        var expectedDivOutput = $"{_textRange} 1 Divide 2 = 0.5\n";
        Add(div, expectedDivOutput);
      }

      private void AddBoolean() {
        var @true = Expression.BooleanConstant(true, _textRange);
        var @false = Expression.BooleanConstant(false, _textRange);
        var andFalse = Expression.Boolean(BooleanOperator.And, new Expression[] { @false, @true },
                                          _textRange);
        var expectedAndFalseOutput = $"{_textRange} False And ? = False\n";
        Add(andFalse, expectedAndFalseOutput);
        var andTrue = Expression.Boolean(BooleanOperator.And, new Expression[] { @true, @true },
                                         _textRange);
        var expectedAndTrueOutput = $"{_textRange} True And True = True\n";
        Add(andTrue, expectedAndTrueOutput);
        var orFalse = Expression.Boolean(BooleanOperator.Or, new Expression[] { @false, @false },
                                         _textRange);
        var expectedOrFalseOutput = $"{_textRange} False Or False = False\n";
        Add(orFalse, expectedOrFalseOutput);
        var orTrue = Expression.Boolean(BooleanOperator.Or, new Expression[] { @true, @false },
                                        _textRange);
        var expectedOrTrueOutput = $"{_textRange} True Or ? = True\n";
        Add(orTrue, expectedOrTrueOutput);
      }

      private void AddComparison() {
        var first = Expression.NumberConstant(1, _textRange);
        var second = Expression.NumberConstant(2, _textRange);
        var less = new ComparisonOperator[] { ComparisonOperator.Less };
        var exprs = new Expression[] { second };
        var comparisonLess = Expression.Comparison(first, less, exprs, _textRange);
        var expectedLessOutput = $"{_textRange} 1 Less 2 = True\n";
        Add(comparisonLess, expectedLessOutput);
        var greater = new ComparisonOperator[] { ComparisonOperator.Greater };
        var comparisonGreater = Expression.Comparison(first, greater, exprs, _textRange);
        var expectedGreaterOutput = $"{_textRange} 1 Greater 2 = False\n";
        Add(comparisonGreater, expectedGreaterOutput);

        var third = Expression.NumberConstant(3, _textRange);
        var ops1 = new ComparisonOperator[] { ComparisonOperator.Less, ComparisonOperator.Less };
        var ops2 = new ComparisonOperator[] { ComparisonOperator.Greater, ComparisonOperator.Less };
        var ops3 = new ComparisonOperator[] { ComparisonOperator.Less, ComparisonOperator.Greater };
        exprs = new Expression[] { second, third };
        var comparison1 = Expression.Comparison(first, ops1, exprs, _textRange);
        var expectedOutput1 = $"{_textRange} 1 Less 2 Less 3 = True\n";
        Add(comparison1, expectedOutput1);
        var comparison2 = Expression.Comparison(first, ops2, exprs, _textRange);
        var expectedOutput2 = $"{_textRange} 1 Greater 2 Less ? = False\n";
        Add(comparison2, expectedOutput2);
        var comparison3 = Expression.Comparison(first, ops3, exprs, _textRange);
        var expectedOutput3 = $"{_textRange} 1 Less 2 Greater 3 = False\n";
        Add(comparison3, expectedOutput3);
      }

      private void AddUnary() {
        var number = Expression.NumberConstant(1, _textRange);
        var positive = Expression.Unary(UnaryOperator.Positive, number, _textRange);
        var expectedPositiveOutput = $"{_textRange} Positive 1 = 1\n";
        Add(positive, expectedPositiveOutput);
        var negative = Expression.Unary(UnaryOperator.Negative, number, _textRange);
        var expectedOutput = $"{_textRange} Negative 1 = -1\n";
        Add(negative, expectedOutput);
        var boolean = Expression.BooleanConstant(true, _textRange);
        var not = Expression.Unary(UnaryOperator.Not, boolean, _textRange);
        var expectedNotOutput = $"{_textRange} Not True = False\n";
        Add(not, expectedNotOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestExpression(Expression expression, string expectedOutput) {
      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(expression);
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
