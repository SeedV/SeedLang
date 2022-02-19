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

using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMTests {
    private class MockupVisualizer : IVisualizer<AssignmentEvent>,
                                     IVisualizer<BinaryEvent>,
                                     IVisualizer<EvalEvent> {
      public string Identifier { get; private set; }
      public IValue Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public IValue Right { get; private set; }
      public IValue Result { get; private set; }
      public Range Range { get; private set; }

      public void On(AssignmentEvent ae) {
        Identifier = ae.Identifier;
        Result = ae.Value;
        Range = ae.Range;
      }

      public void On(BinaryEvent be) {
        Left = be.Left;
        Op = be.Op;
        Right = be.Right;
        Result = be.Result;
        Range = be.Range;
      }

      public void On(EvalEvent ee) {
        Result = ee.Value;
        Range = ee.Range;
      }
    }

    [Fact]
    public void TestBinaryExpressionStatement() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.NumberConstant(1),
                                                           BinaryOperator.Add,
                                                           AstHelper.NumberConstant(2)));

      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();
      Function func = compiler.Compile(expr, vm.Env);
      vm.Run(func);

      Assert.Equal(1, visualizer.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.Number);
      Assert.Equal(3, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestAssignmentStatement() {
      string name = "name";
      var block = AstHelper.Block(AstHelper.Assign(AstHelper.Targets(AstHelper.Id(name)),
                                                   AstHelper.NumberConstant(1)),
                                  AstHelper.ExpressionStmt(AstHelper.Id(name)));

      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();
      Function func = compiler.Compile(block, vm.Env);
      vm.Run(func);

      Assert.Equal(1, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestUnaryExpressionStatement() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
                                                          AstHelper.NumberConstant(1)));

      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();
      Function func = compiler.Compile(expr, vm.Env);
      vm.Run(func);

      Assert.Equal(-1, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);

      expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
        AstHelper.Binary(AstHelper.NumberConstant(1),
                         BinaryOperator.Add,
                         AstHelper.NumberConstant(2))
      ));
      func = compiler.Compile(expr, vm.Env);
      vm.Run(func);

      Assert.Equal(-3, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestFunctionCall() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      string name = "eval";
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.FuncDef(name, AstHelper.Params(a, b),
          AstHelper.Return(AstHelper.Binary(AstHelper.Id(a),
                                            BinaryOperator.Add,
                                            AstHelper.Id(b))
        )),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(name),
                                                AstHelper.NumberConstant(1),
                                                AstHelper.NumberConstant(2)))
      );

      Function func = compiler.Compile(block, vm.Env);
      vm.Run(func);

      Assert.Equal(1, visualizer.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.Number);
      Assert.Equal(3, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestRecursiveFib() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

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

      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);

      Assert.Equal(55, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestSubscript() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      var program = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(1)
      ));

      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);

      Assert.Equal(2, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestSubscriptAssignment() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

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

      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);

      Assert.Equal(5, visualizer.Result.Number);
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    [Fact]
    public void TestTuple() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      var program = AstHelper.ExpressionStmt(
        AstHelper.Tuple(AstHelper.NumberConstant(1),
                        AstHelper.NumberConstant(2),
                        AstHelper.NumberConstant(3))
      );

      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);

      Assert.Equal("(1, 2, 3)", visualizer.Result.ToString());
      Assert.Equal(AstHelper.TextRange, visualizer.Range);
    }

    private static (VM, MockupVisualizer) NewVMWithVisualizer() {
      var visualizer = new MockupVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(visualizer);
      var vm = new VM(visualizerCenter);
      return (vm, visualizer);
    }
  }
}
