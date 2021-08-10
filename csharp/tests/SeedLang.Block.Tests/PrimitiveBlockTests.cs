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

using Xunit;

namespace SeedLang.Block.Tests {
  public class PrimitiveBlockTests {
    [Fact]
    public void TestBlockEquality() {
      // In the SeedBlock layer, two blocks are equal if and only if they share the same type and
      // the same ID. Other states of the blocks do not affect their equality.
      Assert.Equal(new NumberBlock(), new NumberBlock());
      var block1 = new NumberBlock { Id = "123", Value = "1" };
      var block2 = new NumberBlock { Id = "123", Value = "3" };
      var block3 = new NumberBlock { Id = "456", Value = "1" };
      var block4 = new ArithmeticOperatorBlock { Id = "123", Name = "+" };
      var block5 = new ArithmeticOperatorBlock { Id = "123", Name = "-" };
      var block6 = new ArithmeticOperatorBlock { Id = "456", Name = "+" };
      Assert.True(block1 != null);
      Assert.True(block1 != block4);
      Assert.Equal(block1, block2);
      Assert.True(block1 == block2);
      Assert.NotEqual(block1, block3);
      Assert.True(block1 != block3);
      Assert.Equal(block4, block5);
      Assert.True(block4 == block5);
      Assert.NotEqual(block4, block6);
      Assert.True(block4 != block6);
    }

    [Fact]
    public void TestNumberBlock() {
      var block = new NumberBlock();
      Assert.Equal("", block.Id);
      Assert.Equal("", block.Doc);
      Assert.Equal(Position.DockType.UnDocked, block.Pos.Type);
      Assert.Equal("", block.GetEditableText());
      block = new NumberBlock() { Value = "3.14" };
      Assert.Equal("3.14", block.GetEditableText());
      block.UpdateText("10000");
      Assert.Equal("10000", block.GetEditableText());
    }

    [Fact]
    public void TestArithmeticOperatorBlock() {
      var block = new ArithmeticOperatorBlock();
      Assert.Equal("", block.Id);
      Assert.Equal("", block.Doc);
      Assert.Equal(Position.DockType.UnDocked, block.Pos.Type);
      Assert.Equal("+", block.GetEditableText());
      Assert.Equal("+", block.Name);
      block = new ArithmeticOperatorBlock() { Name = "*" };
      Assert.Equal("*", block.GetEditableText());
      Assert.Equal("*", block.Name);
      block.UpdateText("/");
      Assert.Equal("/", block.GetEditableText());
      Assert.Equal("/", block.Name);
    }

    [Fact]
    public void TestParenthesisBlock() {
      var block = new ParenthesisBlock(ParenthesisBlock.Type.Left);
      Assert.Equal("(", block.GetEditableText());
      Assert.Equal("(", block.Name);
      block.UpdateText(")");
      Assert.Equal(")", block.GetEditableText());
      Assert.Equal(")", block.Name);
    }
  }
}
