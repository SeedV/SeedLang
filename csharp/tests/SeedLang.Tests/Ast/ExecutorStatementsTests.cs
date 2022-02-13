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

using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class ExecutorStatementsTests {
    internal class TestData : TheoryData<Statement, string> {
      public TestData() {
        AddAssignment();
        AddBlock();
        AddExpression();
        AddIf();
        AddIfElse();
        AddList();
        AddSubscript();
        AddSubscriptAssignment();
        AddWhile();
        AddNativeFunctionCall();
        AddVoidFunctionCall();
        AddFunctionCall();
        AddForIn();
      }

      private void AddAssignment() {
        string name = "id";
        var assignment = AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)),
                                          AstHelper.NumberConstant(1));
        var expectedOutput = $"{AstHelper.TextRange} {name} = 1\n";
        Add(assignment, expectedOutput);
      }

      private void AddBlock() {
        string name = "id";
        var block = AstHelper.Block(
          AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)), AstHelper.NumberConstant(1)),
          AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.Id(name), BinaryOperator.Add,
                                                    AstHelper.NumberConstant(2)))
        );
        var expectedOutput = $"{AstHelper.TextRange} {name} = 1\n" +
                             $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                             $"{AstHelper.TextRange} Eval 3\n";
        Add(block, expectedOutput);
      }

      private void AddExpression() {
        var expr = AstHelper.ExpressionStmt(AstHelper.Binary(
          AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                           AstHelper.NumberConstant(2)),
          BinaryOperator.Multiply,
          AstHelper.NumberConstant(3)
        ));
        var expectedOutput = $"{AstHelper.TextRange} 3 Multiply 3 = 9\n" +
                             $"{AstHelper.TextRange} Eval 9\n";
        Add(expr, expectedOutput);
      }

      private void AddIf() {
        var ifTrue = AstHelper.If(AstHelper.BooleanConstant(true),
                                  AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)), null);
        var expectedTrueOutput = $"{AstHelper.TextRange} Eval 1\n";
        Add(ifTrue, expectedTrueOutput);
        var ifFalse = AstHelper.If(AstHelper.BooleanConstant(false),
                                   AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)), null);
        Add(ifFalse, "");
      }

      private void AddIfElse() {
        var ifTrue = AstHelper.If(
          AstHelper.BooleanConstant(true),
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
        );
        var expectedTrueOutput = $"{AstHelper.TextRange} Eval 1\n";
        Add(ifTrue, expectedTrueOutput);
        var ifFalse = AstHelper.If(
          AstHelper.BooleanConstant(false),
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
        );
        var expectedFalseOutput = $"{AstHelper.TextRange} Eval 2\n";
        Add(ifFalse, expectedFalseOutput);
      }

      private void AddList() {
        var eval = AstHelper.ExpressionStmt(
          AstHelper.List(AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2),
                         AstHelper.NumberConstant(3))
        );
        var expectedOutput = $"{AstHelper.TextRange} Eval [1, 2, 3]\n";
        Add(eval, expectedOutput);
      }

      private void AddSubscript() {
        var eval = AstHelper.ExpressionStmt(AstHelper.Subscript(
          AstHelper.List(AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2),
                         AstHelper.NumberConstant(3)),
          AstHelper.NumberConstant(1)
        ));
        var expectedOutput = $"{AstHelper.TextRange} Eval 2\n";
        Add(eval, expectedOutput);
      }

      private void AddSubscriptAssignment() {
        string a = "a";
        var block = AstHelper.Block(
          AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)),
                           AstHelper.List(AstHelper.NumberConstant(1),
                                          AstHelper.NumberConstant(2),
                                          AstHelper.NumberConstant(3))),
          AstHelper.Assign(
            AstHelper.Targets(AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(1))),
            AstHelper.NumberConstant(3)
          )
        );
        var expectedOutput = $"{AstHelper.TextRange} a = [1, 3, 3]\n";
        Add(block, expectedOutput);
      }

      private void AddWhile() {
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
                               AstHelper.Binary(AstHelper.Id(sum),
                                                BinaryOperator.Add,
                                                AstHelper.Id(i))),
              AstHelper.Assign(AstHelper.Targets(AstHelper.Id(i)),
                               AstHelper.Binary(AstHelper.Id(i),
                                                BinaryOperator.Add,
                                                AstHelper.NumberConstant(1)))
            )
          ),
          AstHelper.ExpressionStmt(AstHelper.Id(sum))
        );
        var expectedOutput = $"{AstHelper.TextRange} i = 11\n" +
                             $"{AstHelper.TextRange} 10 Add 1 = 11\n" +
                             $"{AstHelper.TextRange} 11 LessEqual 10 = False\n" +
                             $"{AstHelper.TextRange} Eval 55\n";
        Add(program, expectedOutput);
      }

      private void AddNativeFunctionCall() {
        var eval = AstHelper.ExpressionStmt(AstHelper.Call(
          AstHelper.Id(NativeFunctions.Len),
          AstHelper.List(AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2),
                         AstHelper.NumberConstant(3))
        ));
        var expectedOutput = $"{AstHelper.TextRange} Eval 3\n";
        Add(eval, expectedOutput);
      }

      private void AddVoidFunctionCall() {
        string a = "a";
        string func = "func";
        var block = AstHelper.Block(
          AstHelper.FuncDef(func, AstHelper.Params(), AstHelper.Block(
            AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
            AstHelper.Return(null)
          )),
          AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)), AstHelper.Call(AstHelper.Id(func)))
        );
        var expectedOutput = $"{AstHelper.TextRange} {a} = None\n" +
                             $"{AstHelper.TextRange} Eval 1\n";
        Add(block, expectedOutput);
      }

      private void AddFunctionCall() {
        string a = "a";
        string b = "b";
        string add = "add";
        var block = AstHelper.Block(
          AstHelper.FuncDef(add, AstHelper.Params(a, b), AstHelper.Block(
            AstHelper.Return(AstHelper.Binary(AstHelper.Id(a), BinaryOperator.Add, AstHelper.Id(b)))
          )),
          AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)),
                           AstHelper.Call(AstHelper.Id(add),
                                          AstHelper.NumberConstant(1),
                                          AstHelper.NumberConstant(2)))
        );
        var expectedOutput = $"{AstHelper.TextRange} {a} = 3\n" +
                             $"{AstHelper.TextRange} 1 Add 2 = 3\n";
        Add(block, expectedOutput);
      }

      private void AddForIn() {
        string sum = "sum";
        string i = "i";
        var block = AstHelper.Block(
          AstHelper.Assign(AstHelper.Targets(AstHelper.Id(sum)), AstHelper.NumberConstant(0)),
          AstHelper.ForIn(
            AstHelper.Id(i),
            AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2)),
            AstHelper.Block(
              AstHelper.Assign(
                AstHelper.Targets(AstHelper.Id(sum)),
                AstHelper.Binary(AstHelper.Id(sum), BinaryOperator.Add, AstHelper.Id(i))
              )
            )
          ),
          AstHelper.ExpressionStmt(AstHelper.Id(sum))
        );
        var expectedOutput = $"{AstHelper.TextRange} {sum} = 3\n" +
                             $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                             $"{AstHelper.TextRange} Eval 3\n";
        Add(block, expectedOutput);
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
