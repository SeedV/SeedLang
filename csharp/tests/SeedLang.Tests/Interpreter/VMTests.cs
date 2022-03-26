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
using System.IO;
using SeedLang.Ast;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMTests {
    [Fact]
    public void TestBinary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.NumberConstant(1),
                                                           BinaryOperator.Add,
                                                           AstHelper.NumberConstant(2)));

      (string output, MockupVisualizer visualizer) = Run(expr);
      Assert.Equal("3" + Environment.NewLine, output);
      var expectedOutput = $"{AstHelper.TextRange} 1 Add 2 = 3" + Environment.NewLine;
      Assert.Equal(expectedOutput, visualizer.ToString());
    }

    [Fact]
    public void TestInComparison() {
      var expr = AstHelper.ExpressionStmt(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.In),
                             AstHelper.Tuple(AstHelper.NumberConstant(1),
                                             AstHelper.NumberConstant(2)))
      );

      (string output, MockupVisualizer visualizer) = Run(expr);
      Assert.Equal("True" + Environment.NewLine, output);
      Assert.Equal($"{AstHelper.TextRange} 1 In (1, 2) = True\n", visualizer.ToString());
    }

    [Fact]
    public void TestUnary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
                                                          AstHelper.NumberConstant(1)));

      (string output, MockupVisualizer visualizer) = Run(expr);
      Assert.Equal("-1" + Environment.NewLine, output);
      var expected = $"{AstHelper.TextRange} Negative 1 = -1" + Environment.NewLine;
      Assert.Equal(expected, visualizer.ToString());

      expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
        AstHelper.Binary(AstHelper.NumberConstant(1),
                         BinaryOperator.Add,
                         AstHelper.NumberConstant(2))
      ));
      (output, visualizer) = Run(expr);
      Assert.Equal("-3" + Environment.NewLine, output);
      expected = (
        $"{AstHelper.TextRange} 1 Add 2 = 3\n" +
        $"{AstHelper.TextRange} Negative 3 = -3\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, visualizer.ToString());
    }

    [Fact]
    public void TestFunctionCall() {
      string name = "add";
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
      (string output, MockupVisualizer visualizer) = Run(program);
      Assert.Equal("3" + Environment.NewLine, output);
      var expectedOutput = $"{AstHelper.TextRange} 1 Add 2 = 3" + Environment.NewLine;
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
      (string output, MockupVisualizer _) = Run(program);
      Assert.Equal("55" + Environment.NewLine, output);
    }

    [Fact]
    public void TestSubscript() {
      var program = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(1)
      ));
      (string output, MockupVisualizer _) = Run(program);
      Assert.Equal("2" + Environment.NewLine, output);
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
      (string output, MockupVisualizer _) = Run(program);
      Assert.Equal("5" + Environment.NewLine, output);
    }

    [Fact]
    public void TestDict() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Dict(
          AstHelper.KeyValue(AstHelper.NumberConstant(1), AstHelper.NumberConstant(1)),
          AstHelper.KeyValue(AstHelper.StringConstant("a"), AstHelper.NumberConstant(2))
        )
      );
      (string output, MockupVisualizer _) = Run(program);
      Assert.Equal("{1: 1, 'a': 2}" + Environment.NewLine, output);
    }

    [Fact]
    public void TestTuple() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Tuple(AstHelper.NumberConstant(1),
                        AstHelper.NumberConstant(2),
                        AstHelper.NumberConstant(3))
      );
      (string output, MockupVisualizer _) = Run(program);
      Assert.Equal("(1, 2, 3)" + Environment.NewLine, output);
    }

    [Fact]
    public void TestAssignment() {
      string name = "name";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)), AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.Id(name))
      );
      (string output, MockupVisualizer visualizer) = Run(program);
      Assert.Equal("1" + Environment.NewLine, output);
      var expected = $"{AstHelper.TextRange} {name} = 1" + Environment.NewLine;
      Assert.Equal(expected, visualizer.ToString());
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
      (string output, MockupVisualizer visualizer) = Run(block);
      var expectedOutput = (
        $"1\n" +
        $"2\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expectedOutput, output);
      var expected = (
        $"{AstHelper.TextRange} {a} = 1\n" +
        $"{AstHelper.TextRange} {b} = 2\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, visualizer.ToString());
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
      (string output, MockupVisualizer visualizer) = Run(block);
      Assert.Equal("(1, 2)" + Environment.NewLine, output);
      var expected = $"{AstHelper.TextRange} {name} = (1, 2)" + Environment.NewLine;
      Assert.Equal(expected, visualizer.ToString());
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
      (string output, MockupVisualizer _) = Run(block);
      var expectedOutput = (
        $"1\n" +
        $"2\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expectedOutput, output);
    }

    private static (string, MockupVisualizer) Run(Statement program) {
      var visualizer = new MockupVisualizer();
      var vc = new VisualizerCenter();
      vc.Register(visualizer);
      var vm = new VM(vc);
      var stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vc, RunMode.Interactive);
      vm.Run(func);
      return (stringWriter.ToString(), visualizer);
    }
  }
}
