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
using SeedLang.Common;
using SeedLang.Runtime;
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

    private static TextRange _testTextRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestBinaryExpressionStatement() {
      var left = Expression.NumberConstant(1, _testTextRange);
      var right = Expression.NumberConstant(2, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _testTextRange);
      var expr = Statement.Expression(binary, _testTextRange);
      var compiler = new Compiler();
      Chunk chunk = compiler.Compile(expr);

      (var vm, var visualizer) = NewVMWithVisualizer();
      vm.Run(chunk);

      Assert.Equal(1, visualizer.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.Number);
      Assert.Equal(3, visualizer.Result.Number);
      Assert.Equal(_testTextRange, visualizer.Range);
    }

    [Fact]
    public void TestGlobalVariable() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      var identifier = Expression.Identifier("name", _testTextRange);
      var number = Expression.NumberConstant(1, _testTextRange);
      var assignment = Statement.Assignment(identifier, number, _testTextRange);
      Chunk chunk = compiler.Compile(assignment);
      vm.Run(chunk);

      var expr = Statement.Expression(identifier, _testTextRange);
      chunk = compiler.Compile(expr);
      vm.Run(chunk);

      Assert.Equal(1, visualizer.Result.Number);
      Assert.Equal(_testTextRange, visualizer.Range);
    }

    [Fact]
    public void TestUnaryExpressionStatement() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      var number = Expression.NumberConstant(1, _testTextRange);
      var unary = Expression.Unary(UnaryOperator.Negative, number, _testTextRange);
      var expr = Statement.Expression(unary, _testTextRange);
      Chunk chunk = compiler.Compile(expr);
      vm.Run(chunk);

      Assert.Equal(-1, visualizer.Result.Number);
      Assert.Equal(_testTextRange, visualizer.Range);

      var left = Expression.NumberConstant(1, _testTextRange);
      var right = Expression.NumberConstant(2, _testTextRange);
      var binary = Expression.Binary(left, BinaryOperator.Add, right, _testTextRange);
      unary = Expression.Unary(UnaryOperator.Negative, binary, _testTextRange);
      expr = Statement.Expression(unary, _testTextRange);
      chunk = compiler.Compile(expr);
      vm.Run(chunk);

      Assert.Equal(-3, visualizer.Result.Number);
      Assert.Equal(_testTextRange, visualizer.Range);
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
