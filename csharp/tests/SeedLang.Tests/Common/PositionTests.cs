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

using System;
using Xunit;

namespace SeedLang.Common.Tests {
  public class PositionTests {
    [Fact]
    public void TestTextPositions() {
      Assert.True(new TextPosition(1, 3).Equals(new TextPosition(1, 3)));
      Assert.True(new TextPosition(1, 3) == new TextPosition(1, 3));
      Assert.False(new TextPosition(1, 3).Equals(new TextPosition(1, 4)));
      Assert.False(new TextPosition(1, 3).Equals(new TextPosition(2, 3)));
      Assert.True(new TextPosition(1, 3) != new TextPosition(1, 4));
      Assert.True(new TextPosition(1, 3) != new TextPosition(2, 3));
      Assert.True(new TextPosition(2, 3) > new TextPosition(1, 3));
      Assert.True(new TextPosition(2, 3) >= new TextPosition(1, 3));
      Assert.True(new TextPosition(2, 3) >= new TextPosition(2, 3));
      Assert.True(new TextPosition(2, 3) < new TextPosition(2, 4));
      Assert.True(new TextPosition(2, 3) < new TextPosition(3, 1));
      Assert.True(new TextPosition(2, 3) <= new TextPosition(3, 1));
      Assert.True(new TextPosition(2, 3) <= new TextPosition(2, 3));
    }

    [Theory]
    [InlineData(0, 0, "Ln 0, Col 0")]
    [InlineData(1, 3, "Ln 1, Col 3")]
    [InlineData(65535, 255, "Ln 65535, Col 255")]
    public void TestTextPositionToString(int line, int column, string desc) {
      Assert.Equal(desc, new TextPosition(line, column).ToString());
    }

    [Fact]
    public void TestBlockPositions() {
      Assert.True(new BlockPosition("001").Equals(new BlockPosition("001")));
      Assert.True(new BlockPosition("001") == new BlockPosition("001"));
      Assert.False(new BlockPosition("001").Equals(new BlockPosition("002")));
      Assert.True(new BlockPosition("001") != new BlockPosition("002"));
    }

    [Theory]
    [InlineData("001", "Block: 001")]
    [InlineData("someId", "Block: someId")]
    public void TestBlockPositionToString(string blockId, string desc) {
      Assert.Equal(desc, new BlockPosition(blockId).ToString());
    }
  }
}
