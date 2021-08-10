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

using System.Linq;
using Xunit;
using SeedLang.Common;

namespace SeedLang.Block.Tests {
  public class ModuleTests {
    [Fact]
    public void TestModule() {
      var module = new Module { Name = "Main" };
      Assert.Empty(module.Blocks);
      Assert.Equal(0, module.RootBlockCount);

      var expressionBlock = new ExpressionBlock();
      var numberBlock1 = new NumberBlock { Value = "3.14" };
      var numberBlock2 = new NumberBlock { Value = "5" };
      var operatorBlock = new ArithmeticOperatorBlock { Name = "*" };
      var parenthsisBlock1 = new ParenthesisBlock(ParenthesisBlock.Type.Left);
      var parenthsisBlock2 = new ParenthesisBlock(ParenthesisBlock.Type.Right);

      module.AddStandaloneBlock(expressionBlock);
      Assert.Single(module.Blocks);
      Assert.Equal(1, module.RootBlockCount);
      module.RemoveBlocks(expressionBlock.Id);
      Assert.Empty(module.Blocks);
      Assert.Equal(0, module.RootBlockCount);
      module.AddStandaloneBlock(expressionBlock);
      module.MoveRootBlock(expressionBlock.Id, new Vector2(100, 100));
      Assert.Equal(new Vector2(100, 100), expressionBlock.Pos.CanvasPosition);
      module.Clear();

      module.AddStandaloneBlock(expressionBlock);
      module.AddStandaloneBlock(numberBlock1);
      module.AddStandaloneBlock(numberBlock2);
      module.AddStandaloneBlock(operatorBlock);
      module.AddStandaloneBlock(parenthsisBlock1);
      module.AddStandaloneBlock(parenthsisBlock2);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(6, module.RootBlockCount);

      module.DockBlock(parenthsisBlock2.Id, expressionBlock.Id, Position.DockType.Input, 0);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(5, module.RootBlockCount);

      module.UnDockBlock(parenthsisBlock2.Id, new Vector2(0, 0));
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(6, module.RootBlockCount);

      module.DockBlock(parenthsisBlock2.Id, expressionBlock.Id, Position.DockType.Input, 0);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(5, module.RootBlockCount);

      module.DockBlock(numberBlock2.Id, expressionBlock.Id, Position.DockType.Input, 0);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(4, module.RootBlockCount);

      module.DockBlock(operatorBlock.Id, expressionBlock.Id, Position.DockType.Input, 0);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(3, module.RootBlockCount);

      module.DockBlock(numberBlock1.Id, expressionBlock.Id, Position.DockType.Input, 0);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(2, module.RootBlockCount);

      module.DockBlock(parenthsisBlock1.Id, expressionBlock.Id, Position.DockType.Input, 0);
      Assert.Equal(6, module.Blocks.Count);
      Assert.Equal(1, module.RootBlockCount);

      var rootBlockList = module.RootBlockIterator.ToList();
      Assert.Equal(expressionBlock, rootBlockList[0]);

      module.RemoveBlocks(expressionBlock.Id);
      Assert.Equal(0, module.Blocks.Count);
      Assert.Equal(0, module.RootBlockCount);
    }
  }
}
