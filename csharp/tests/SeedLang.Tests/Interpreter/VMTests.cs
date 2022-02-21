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

using SeedLang.Ast;
using SeedLang.Ast.Tests;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMTests {
    [Fact]
    public void TestBinaryExpression() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.NumberConstant(1),
                                                           BinaryOperator.Add,
                                                           AstHelper.NumberConstant(2)));

      MockupVisualizer visualizer = Run(expr);
      var expectedOutput = $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                           $"{AstHelper.TextRange} Eval 3\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestUnaryExpression() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
                                                          AstHelper.NumberConstant(1)));

      MockupVisualizer visualizer = Run(expr);
      var expectedOutput = $"{AstHelper.TextRange} Eval -1\n";
      Assert.Equal(expectedOutput, visualizer.ToString());

      expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
        AstHelper.Binary(AstHelper.NumberConstant(1),
                         BinaryOperator.Add,
                         AstHelper.NumberConstant(2))
      ));
      visualizer = Run(expr);
      expectedOutput = $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                       $"{AstHelper.TextRange} Eval -3\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestFunctionCall() {
      string name = "eval";
      string a = "a";
      string b = "b";
      var program = AstHelper.Block(
        AstHelper.FuncDef(name, AstHelper.Params(a, b),
          AstHelper.Return(AstHelper.Binary(AstHelper.Id(a),
                                            BinaryOperator.Add,
                                            AstHelper.Id(b))
        )),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(name),
                                                AstHelper.NumberConstant(1),
                                                AstHelper.NumberConstant(2)))
      );
      MockupVisualizer visualizer = Run(program);
      var expectedOutput = $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
                           $"{AstHelper.TextRange} Eval 3\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestRecursiveFib() {
      string fib = "fib";
      string n = "n";
      var test = AstHelper.Boolean(BooleanOperator.Or,
        AstHelper.Comparison(AstHelper.Id(n), AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(1)),
        AstHelper.Comparison(AstHelper.Id(n), AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(2))
      );
      var trueBlock = AstHelper.Return(AstHelper.NumberConstant(1));
      var falseBlock = AstHelper.Return(AstHelper.Binary(
        AstHelper.Call(AstHelper.Id(fib), AstHelper.Binary(AstHelper.Id(n), BinaryOperator.Subtract,
                                                           AstHelper.NumberConstant(1))),
        BinaryOperator.Add,
        AstHelper.Call(AstHelper.Id(fib), AstHelper.Binary(AstHelper.Id(n), BinaryOperator.Subtract,
                                                           AstHelper.NumberConstant(2)))
      ));
      var program = AstHelper.Block(
        AstHelper.FuncDef(fib, new string[] { n }, AstHelper.Block(
          AstHelper.If(test, trueBlock, falseBlock)
        )),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(fib), AstHelper.NumberConstant(10)))
      );
      MockupVisualizer visualizer = Run(program);
      var expectedOutput = $"{AstHelper.TextRange} Eval 55\n";
      Assert.Equal(expectedOutput, visualizer.EvalEventsToString());
    }

    [Fact]
    public void TestSubscript() {
      var program = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(1)
      ));
      MockupVisualizer visualizer = Run(program);
      var expectedOutput = $"{AstHelper.TextRange} Eval 2\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestSubscriptAssignment() {
      string a = "a";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a)),
                         AstHelper.List(AstHelper.NumberConstant(1),
                                        AstHelper.NumberConstant(2),
                                        AstHelper.NumberConstant(3))),
        AstHelper.Assign(AstHelper.Targets(AstHelper.Subscript(AstHelper.Id(a),
                                                               AstHelper.NumberConstant(1))),
                         AstHelper.NumberConstant(5)),
        AstHelper.ExpressionStmt(AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(1)))
      );
      MockupVisualizer visualizer = Run(program);
      var expectedOutput = $"{AstHelper.TextRange} Eval 5\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestTuple() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Tuple(AstHelper.NumberConstant(1),
                        AstHelper.NumberConstant(2),
                        AstHelper.NumberConstant(3))
      );
      MockupVisualizer visualizer = Run(program);
      var expectedOutput = $"{AstHelper.TextRange} Eval (1, 2, 3)\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestAssignment() {
      string name = "name";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)), AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.Id(name))
      );
      MockupVisualizer visualizer = Run(program);
      var expectedOutput = $"{AstHelper.TextRange} Eval 1\n";
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestMultipleAssignment() {
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b)),
                         AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.Id(a)),
        AstHelper.ExpressionStmt(AstHelper.Id(b))
      );
      var expectedOutput = $"{AstHelper.TextRange} Eval 1\n" +
                           $"{AstHelper.TextRange} Eval 2\n";
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestPackAssignment() {
      string name = "id";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)),
                         AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.Id(name))
      );
      var expectedOutput = $"{AstHelper.TextRange} Eval (1, 2)\n";
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestUnpackAssignment() {
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b)),
                         AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2))),
        AstHelper.ExpressionStmt(AstHelper.Id(a)),
        AstHelper.ExpressionStmt(AstHelper.Id(b))
      );
      var expectedOutput = $"{AstHelper.TextRange} Eval 1\n" +
                           $"{AstHelper.TextRange} Eval 2\n";
      MockupVisualizer visualizer = Run(block);
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    private static MockupVisualizer Run(Statement program) {
      var visualizer = new MockupVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(visualizer);
      var vm = new VM(visualizerCenter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);
      return visualizer;
    }
  }
}
