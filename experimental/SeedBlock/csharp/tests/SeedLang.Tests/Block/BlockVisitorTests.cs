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

using System.Text;
using Xunit;

namespace SeedLang.Block.Tests {
  public class BlockVisitorTests {
    private class MyVisitor : AbstractBlockVisitor {
      public int Count { get; set; } = 0;
      public StringBuilder Preorder { get; set; } = new StringBuilder();
      public StringBuilder Postorder { get; set; } = new StringBuilder();

      public override void VisitEnter(BaseBlock block) {
        Count++;
        Preorder.AppendFormat("{0} ", block.GetType().Name);
      }

      public override void VisitExit(BaseBlock block) {
        Postorder.AppendFormat("{0} ", block.GetType().Name);
      }
    }

    [Fact]
    public void TestBlockVisitor() {
      var expressionBlock = new ExpressionBlock();
      var numberBlock1 = new NumberBlock { Id = "001", Value = "3.14" };
      var numberBlock2 = new NumberBlock { Id = "002", Value = "5" };
      var operatorBlock = new ArithmeticOperatorBlock { Id = "003", Name = "*" };
      var parenthsisBlock1 = new ParenthesisBlock(ParenthesisBlock.Type.Left) { Id = "004" };
      var parenthsisBlock2 = new ParenthesisBlock(ParenthesisBlock.Type.Right) { Id = "005" };
      expressionBlock.Dock(parenthsisBlock2, Position.DockType.Input, 0);
      expressionBlock.Dock(numberBlock1, Position.DockType.Input, 0);
      expressionBlock.Dock(operatorBlock, Position.DockType.Input, 0);
      expressionBlock.Dock(numberBlock2, Position.DockType.Input, 0);
      expressionBlock.Dock(parenthsisBlock1, Position.DockType.Input, 0);

      var visitor = new MyVisitor();
      expressionBlock.Accept(visitor);
      Assert.Equal(6, visitor.Count);
      Assert.Equal("ExpressionBlock ParenthesisBlock NumberBlock ArithmeticOperatorBlock " +
                   "NumberBlock ParenthesisBlock ", visitor.Preorder.ToString());
      Assert.Equal("ParenthesisBlock NumberBlock ArithmeticOperatorBlock NumberBlock " +
                   "ParenthesisBlock ExpressionBlock ", visitor.Postorder.ToString());
    }
  }
}
