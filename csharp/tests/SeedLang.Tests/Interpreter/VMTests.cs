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
      public Value Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public Value Right { get; private set; }
      public Value Result { get; private set; }
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
    public void TestVM() {
      var left = Expression.Number(1, NewTextRange());
      var right = Expression.Number(2, new TextRange(0, 1, 2, 3));
      var binary = Expression.Binary(left, BinaryOperator.Add, right, new TextRange(0, 1, 2, 3));
      var eval = Statement.Eval(binary, new TextRange(0, 1, 2, 3));
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);

      (var vm, var visualizer) = NewVMWithVisualizer();
      vm.Run(chunk);

      Assert.Equal(1, visualizer.Left.ToNumber());
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.ToNumber());
      Assert.Equal(3, visualizer.Result.ToNumber());
      Assert.Equal(NewTextRange(), visualizer.Range);
    }

    private static TextRange NewTextRange() {
      return new TextRange(0, 1, 2, 3);
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
