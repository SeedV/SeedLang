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

using Xunit;
using SeedLang.Common;

namespace SeedLang.Block.Tests {
  public class ExpressionBlockTests {
    [Fact]
    public void TestExpressionBlock() {
      var expressionBlock = new ExpressionBlock { Id = "000" };
      var numberBlock1 = new NumberBlock { Id = "001", Value = "3.14" };
      var numberBlock2 = new NumberBlock { Id = "002", Value = "5" };
      var operatorBlock = new ArithmeticOperatorBlock { Id = "003", Name = "*" };
      var parenthsisBlock1 = new ParenthesisBlock(ParenthesisBlock.Type.Left) { Id = "004" };
      var parenthsisBlock2 = new ParenthesisBlock(ParenthesisBlock.Type.Right) { Id = "005" };

      Assert.Empty(expressionBlock.Inputs);
      Assert.True(expressionBlock.CanDock(numberBlock1, Position.DockType.Input, 0));
      Assert.True(expressionBlock.CanDock(operatorBlock, Position.DockType.Input, 0));
      Assert.False(expressionBlock.CanDock(numberBlock1, Position.DockType.ChildStatement, 0));
      Assert.False(expressionBlock.CanDock(numberBlock1, Position.DockType.Input, -1));
      Assert.False(expressionBlock.CanDock(numberBlock1, Position.DockType.Input, 1));

      expressionBlock.Dock(numberBlock1, Position.DockType.Input, 0);
      Assert.Single(expressionBlock.Inputs);
      Assert.Equal(Position.DockType.Input, numberBlock1.Pos.Type);
      Assert.Equal(0, numberBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock1.Pos.TargetBlockId);

      expressionBlock.Dock(operatorBlock, Position.DockType.Input, 0);
      Assert.Equal(2, expressionBlock.Inputs.Count);
      Assert.Equal(Position.DockType.Input, operatorBlock.Pos.Type);
      Assert.Equal(0, operatorBlock.Pos.DockSlotIndex);
      Assert.Equal("000", operatorBlock.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, numberBlock1.Pos.Type);
      Assert.Equal(1, numberBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock1.Pos.TargetBlockId);

      expressionBlock.Dock(numberBlock2, Position.DockType.Input, 0);
      Assert.Equal(3, expressionBlock.Inputs.Count);
      Assert.Equal(Position.DockType.Input, numberBlock2.Pos.Type);
      Assert.Equal(0, numberBlock2.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock2.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, operatorBlock.Pos.Type);
      Assert.Equal(1, operatorBlock.Pos.DockSlotIndex);
      Assert.Equal("000", operatorBlock.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, numberBlock1.Pos.Type);
      Assert.Equal(2, numberBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock1.Pos.TargetBlockId);

      expressionBlock.Dock(parenthsisBlock1, Position.DockType.Input, 0);
      Assert.Equal(4, expressionBlock.Inputs.Count);
      Assert.Equal(Position.DockType.Input, parenthsisBlock1.Pos.Type);
      Assert.Equal(0, parenthsisBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", parenthsisBlock1.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, numberBlock2.Pos.Type);
      Assert.Equal(1, numberBlock2.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock2.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, operatorBlock.Pos.Type);
      Assert.Equal(2, operatorBlock.Pos.DockSlotIndex);
      Assert.Equal("000", operatorBlock.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, numberBlock1.Pos.Type);
      Assert.Equal(3, numberBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock1.Pos.TargetBlockId);

      expressionBlock.Dock(parenthsisBlock2, Position.DockType.Input, 4);
      Assert.Equal(5, expressionBlock.Inputs.Count);
      Assert.Equal(Position.DockType.Input, parenthsisBlock1.Pos.Type);
      Assert.Equal(0, parenthsisBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", parenthsisBlock1.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, numberBlock2.Pos.Type);
      Assert.Equal(1, numberBlock2.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock2.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, operatorBlock.Pos.Type);
      Assert.Equal(2, operatorBlock.Pos.DockSlotIndex);
      Assert.Equal("000", operatorBlock.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, numberBlock1.Pos.Type);
      Assert.Equal(3, numberBlock1.Pos.DockSlotIndex);
      Assert.Equal("000", numberBlock1.Pos.TargetBlockId);
      Assert.Equal(Position.DockType.Input, parenthsisBlock2.Pos.Type);
      Assert.Equal(4, parenthsisBlock2.Pos.DockSlotIndex);
      Assert.Equal("000", parenthsisBlock2.Pos.TargetBlockId);

      Assert.Equal("(5*3.14)", expressionBlock.GetEditableText());

      expressionBlock.UnDock(parenthsisBlock1, new Vector2());
      expressionBlock.UnDock(parenthsisBlock2, new Vector2());
      expressionBlock.UnDock(numberBlock1, new Vector2());
      expressionBlock.UnDock(numberBlock2, new Vector2());
      expressionBlock.UnDock(operatorBlock, new Vector2());
      Assert.Empty(expressionBlock.Inputs);
    }
  }
}
