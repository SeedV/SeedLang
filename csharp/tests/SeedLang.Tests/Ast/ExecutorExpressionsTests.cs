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
  public class ExecutorExpresionsTests {
    internal class TestData : TheoryData<Expression, string> {
      public TestData() {
        AddBinary();
        AddBoolean();
        AddComparison();
        AddUnary();
      }

      private void AddBinary() {
        var add = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.Add,
                                   AstHelper.NumberConstant(2));
        var expectedAddOutput = $"{AstHelper.TextRange} 3 Add 2 = 5" + Environment.NewLine;
        Add(add, expectedAddOutput);
        var sub = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.Subtract,
                                   AstHelper.NumberConstant(2));
        var expectedSubOutput = $"{AstHelper.TextRange} 3 Subtract 2 = 1" + Environment.NewLine;
        Add(sub, expectedSubOutput);
        var mul = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.Multiply,
                                   AstHelper.NumberConstant(2));
        var expectedMulOutput = $"{AstHelper.TextRange} 3 Multiply 2 = 6" + Environment.NewLine;
        Add(mul, expectedMulOutput);
        var div = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.Divide,
                                   AstHelper.NumberConstant(2));
        var expectedDivOutput = $"{AstHelper.TextRange} 3 Divide 2 = 1.5" + Environment.NewLine;
        Add(div, expectedDivOutput);
        var floorDiv = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.FloorDivide,
                                        AstHelper.NumberConstant(2));
        var expectedFloorDivOutput = $"{AstHelper.TextRange} 3 FloorDivide 2 = 1" +
                                     Environment.NewLine;
        Add(floorDiv, expectedFloorDivOutput);
        var power = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.Power,
                                     AstHelper.NumberConstant(2));
        var expectedPowerOutput = $"{AstHelper.TextRange} 3 Power 2 = 9" + Environment.NewLine;
        Add(power, expectedPowerOutput);
        var modulo = AstHelper.Binary(AstHelper.NumberConstant(3), BinaryOperator.Modulo,
                                      AstHelper.NumberConstant(2));
        var expectedModuloOutput = $"{AstHelper.TextRange} 3 Modulo 2 = 1" + Environment.NewLine;
        Add(modulo, expectedModuloOutput);
      }

      private void AddBoolean() {
        var andFalse = AstHelper.Boolean(BooleanOperator.And, AstHelper.BooleanConstant(false),
                                         AstHelper.BooleanConstant(true));
        var expectedAndFalseOutput = $"{AstHelper.TextRange} False And ? = False" +
                                     Environment.NewLine;
        Add(andFalse, expectedAndFalseOutput);
        var andTrue = AstHelper.Boolean(BooleanOperator.And, AstHelper.BooleanConstant(true),
                                        AstHelper.BooleanConstant(true));
        var expectedAndTrueOutput = $"{AstHelper.TextRange} True And True = True" +
                                    Environment.NewLine;
        Add(andTrue, expectedAndTrueOutput);
        var orFalse = AstHelper.Boolean(BooleanOperator.Or, AstHelper.BooleanConstant(false),
                                        AstHelper.BooleanConstant(false));
        var expectedOrFalseOutput = $"{AstHelper.TextRange} False Or False = False" +
                                    Environment.NewLine;
        Add(orFalse, expectedOrFalseOutput);
        var orTrue = AstHelper.Boolean(BooleanOperator.Or, AstHelper.BooleanConstant(true),
                                       AstHelper.BooleanConstant(false));
        var expectedOrTrueOutput = $"{AstHelper.TextRange} True Or ? = True" + Environment.NewLine;
        Add(orTrue, expectedOrTrueOutput);
      }

      private void AddComparison() {
        var comparisonLess = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                                  AstHelper.CompOps(ComparisonOperator.Less),
                                                  AstHelper.NumberConstant(2));
        var expectedLessOutput = $"{AstHelper.TextRange} 1 Less 2 = True" + Environment.NewLine;
        Add(comparisonLess, expectedLessOutput);
        var comparisonGreater = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                                     AstHelper.CompOps(ComparisonOperator.Greater),
                                                     AstHelper.NumberConstant(2));
        var expectedGreaterOutput = $"{AstHelper.TextRange} 1 Greater 2 = False" +
                                    Environment.NewLine;
        Add(comparisonGreater, expectedGreaterOutput);

        var comparison1 = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                               AstHelper.CompOps(ComparisonOperator.Less,
                                                                 ComparisonOperator.Less),
                                               AstHelper.NumberConstant(2),
                                               AstHelper.NumberConstant(3));
        var expectedOutput1 = $"{AstHelper.TextRange} 1 Less 2 Less 3 = True" + Environment.NewLine;
        Add(comparison1, expectedOutput1);
        var comparison2 = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                               AstHelper.CompOps(ComparisonOperator.Greater,
                                                                 ComparisonOperator.Less),
                                               AstHelper.NumberConstant(2),
                                               AstHelper.NumberConstant(3));
        var expectedOutput2 = $"{AstHelper.TextRange} 1 Greater 2 Less ? = False" +
                              Environment.NewLine;
        Add(comparison2, expectedOutput2);
        var comparison3 = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                               AstHelper.CompOps(ComparisonOperator.Less,
                                                                 ComparisonOperator.Greater),
                                               AstHelper.NumberConstant(2),
                                               AstHelper.NumberConstant(3));
        var expectedOutput3 = $"{AstHelper.TextRange} 1 Less 2 Greater 3 = False" +
                              Environment.NewLine;
        Add(comparison3, expectedOutput3);
        var comparison4 = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                               AstHelper.CompOps(ComparisonOperator.In),
                                               AstHelper.Tuple(AstHelper.NumberConstant(1),
                                                               AstHelper.NumberConstant(2)));
        var expectedOutput4 = $"{AstHelper.TextRange} 1 In (1, 2) = True" + Environment.NewLine;
        Add(comparison4, expectedOutput4);
      }

      private void AddUnary() {
        var positive = AstHelper.Unary(UnaryOperator.Positive, AstHelper.NumberConstant(1));
        var expectedPositiveOutput = $"{AstHelper.TextRange} Positive 1 = 1" + Environment.NewLine;
        Add(positive, expectedPositiveOutput);
        var negative = AstHelper.Unary(UnaryOperator.Negative, AstHelper.NumberConstant(1));
        var expectedNegativeOutput = $"{AstHelper.TextRange} Negative 1 = -1" + Environment.NewLine;
        Add(negative, expectedNegativeOutput);
        var not = AstHelper.Unary(UnaryOperator.Not, AstHelper.BooleanConstant(true));
        var expectedNotOutput = $"{AstHelper.TextRange} Not True = False" + Environment.NewLine;
        Add(not, expectedNotOutput);
      }
    }

    [Theory]
    [ClassData(typeof(TestData))]
    internal void TestExpression(Expression expression, string expectedOutput) {
      (var executor, var visualizer) = NewExecutorWithVisualizer();
      executor.Run(expression, RunMode.Interactive);
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
