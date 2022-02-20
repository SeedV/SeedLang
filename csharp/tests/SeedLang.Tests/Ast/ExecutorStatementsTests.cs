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
    [Fact]
    public void TestSingleAssignment() {
      string name = "id";
      var assignment = AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)),
                                        AstHelper.NumberConstant(1));
      var expectedOutput = $"{AstHelper.TextRange} {name} = 1\n";
      MockupVisualizer visualizer = Run(assignment);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestMultipleAssignment() {
      string a = "a";
      string b = "b";
      var assignment = AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b)),
                                        AstHelper.NumberConstant(1),
                                        AstHelper.NumberConstant(2));
      var expectedOutput = $"{AstHelper.TextRange} {a} = 1\n" +
                           $"{AstHelper.TextRange} {b} = 2\n";
      MockupVisualizer visualizer = Run(assignment);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestPackAssignment() {
      string name = "id";
      var assignment = AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)),
                                        AstHelper.NumberConstant(1),
                                        AstHelper.NumberConstant(2));
      var expectedOutput = $"{AstHelper.TextRange} {name} = (1, 2)\n";
      MockupVisualizer visualizer = Run(assignment);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestUnpackAssignment() {
      string a = "a";
      string b = "b";
      var assignment = AstHelper.Assign(
        AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b)),
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2))
      );
      var expectedOutput = $"{AstHelper.TextRange} {a} = 1\n" +
                           $"{AstHelper.TextRange} {b} = 2\n";
      MockupVisualizer visualizer = Run(assignment);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestBlock() {
      string name = "id";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)), AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.Id(name), BinaryOperator.Add,
                                                  AstHelper.NumberConstant(2)))
      );
      var expectedOutput = $"{AstHelper.TextRange} {name} = 1\n" +
                           $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                           $"{AstHelper.TextRange} Eval 3\n";
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestExpression() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(
        AstHelper.Binary(AstHelper.NumberConstant(1), BinaryOperator.Add,
                         AstHelper.NumberConstant(2)),
        BinaryOperator.Multiply,
        AstHelper.NumberConstant(3)
      ));
      var expectedOutput = $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                           $"{AstHelper.TextRange} 3 Multiply 3 = 9\n" +
                           $"{AstHelper.TextRange} Eval 9\n";
      MockupVisualizer visualizer = Run(expr);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestIf() {
      var ifTrue = AstHelper.If(AstHelper.BooleanConstant(true),
                                AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)), null);
      var expectedTrueOutput = $"{AstHelper.TextRange} Eval 1\n";
      MockupVisualizer visualizer = Run(ifTrue);
      Assert.Equal(expectedTrueOutput, visualizer.ToString());

      var ifFalse = AstHelper.If(AstHelper.BooleanConstant(false),
                                 AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)), null);
      visualizer = Run(ifFalse);
      Assert.Equal("", visualizer.ToString());
    }

    [Fact]
    public void TestIfElse() {
      var ifTrue = AstHelper.If(
        AstHelper.BooleanConstant(true),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var expectedTrueOutput = $"{AstHelper.TextRange} Eval 1\n";
      MockupVisualizer visualizer = Run(ifTrue);
      Assert.Equal(expectedTrueOutput, visualizer.ToString());

      var ifFalse = AstHelper.If(
        AstHelper.BooleanConstant(false),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.NumberConstant(2))
      );
      var expectedFalseOutput = $"{AstHelper.TextRange} Eval 2\n";
      visualizer = Run(ifFalse);
      Assert.Equal(expectedFalseOutput, visualizer.ToString());
    }

    [Fact]
    public void TestList() {
      var eval = AstHelper.ExpressionStmt(
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3))
      );
      var expectedOutput = $"{AstHelper.TextRange} Eval [1, 2, 3]\n";
      MockupVisualizer visualizer = Run(eval);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestTuple() {
      var eval = AstHelper.ExpressionStmt(
        AstHelper.Tuple(AstHelper.NumberConstant(1),
                        AstHelper.NumberConstant(2),
                        AstHelper.NumberConstant(3))
      );
      var expectedOutput = $"{AstHelper.TextRange} Eval (1, 2, 3)\n";
      MockupVisualizer visualizer = Run(eval);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestSubscript() {
      var eval = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(1)
      ));
      var expectedOutput = $"{AstHelper.TextRange} Eval 2\n";
      MockupVisualizer visualizer = Run(eval);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestSubscriptAssignment() {
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
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestWhile() {
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
      var expectedOutput = $"{AstHelper.TextRange} Eval 55\n";
      MockupVisualizer visualizer = Run(program);
      Assert.Equal(expectedOutput, visualizer.EvalEventsToString());
    }

    [Fact]
    public void TestNativeFunctionCall() {
      var eval = AstHelper.ExpressionStmt(AstHelper.Call(
        AstHelper.Id(NativeFunctions.Len),
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3))
      ));
      var expectedOutput = $"{AstHelper.TextRange} Eval 3\n";
      MockupVisualizer visualizer = Run(eval);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestVoidFunctionCall() {
      string a = "a";
      string func = "func";
      var block = AstHelper.Block(
        AstHelper.FuncDef(func, AstHelper.Params(), AstHelper.Block(
          AstHelper.ExpressionStmt(AstHelper.NumberConstant(1)),
          AstHelper.Return()
        )),
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)), AstHelper.Call(AstHelper.Id(func)))
      );
      var expectedOutput = $"{AstHelper.TextRange} {a} = None\n" +
                           $"{AstHelper.TextRange} Eval 1\n";
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestFunctionCall() {
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
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestForIn() {
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
      var expectedOutput = $"{AstHelper.TextRange} {sum} = 0\n" +
                           $"{AstHelper.TextRange} {sum} = 1\n" +
                           $"{AstHelper.TextRange} {sum} = 3\n" +
                           $"{AstHelper.TextRange} 0 Add 1 = 1\n" +
                           $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                           $"{AstHelper.TextRange} Eval 3\n";
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    private static MockupVisualizer Run(Statement program) {
      var visualizer = new MockupVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(visualizer);
      var executor = new Executor(visualizerCenter);
      executor.Run(program);
      return visualizer;
    }
  }
}
