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
  public class CodePositionTests {
    [Fact]
    public void TestTextCodePositions() {
      Assert.Equal(new TextCodePosition(1, 3), new TextCodePosition(1, 3));
      Assert.True(new TextCodePosition(1, 3) == new TextCodePosition(1, 3));
      Assert.NotEqual(new TextCodePosition(1, 3), new TextCodePosition(1, 4));
      Assert.NotEqual(new TextCodePosition(1, 3), new TextCodePosition(2, 3));
      Assert.True(new TextCodePosition(1, 3) != new TextCodePosition(1, 4));
      Assert.True(new TextCodePosition(1, 3) != new TextCodePosition(2, 3));
      Assert.True(new TextCodePosition(2, 3) > new TextCodePosition(1, 3));
      Assert.True(new TextCodePosition(2, 3) >= new TextCodePosition(1, 3));
      Assert.True(new TextCodePosition(2, 3) >= new TextCodePosition(2, 3));
      Assert.True(new TextCodePosition(2, 3) < new TextCodePosition(2, 4));
      Assert.True(new TextCodePosition(2, 3) < new TextCodePosition(3, 1));
      Assert.True(new TextCodePosition(2, 3) <= new TextCodePosition(3, 1));
      Assert.True(new TextCodePosition(2, 3) <= new TextCodePosition(2, 3));
    }

    [Theory]
    [InlineData(0, 0, "Ln 0, Col 0")]
    [InlineData(1, 3, "Ln 1, Col 3")]
    [InlineData(65535, 255, "Ln 65535, Col 255")]
    public void TestTextCodePositionToString(int line, int column, string desc) {
      Assert.Equal(desc, new TextCodePosition(line, column).ToString());
    }

    [Fact]
    public void TestBlockCodePositions() {
      Assert.Equal(new BlockCodePosition("001"), new BlockCodePosition("001"));
      Assert.True(new BlockCodePosition("001") == new BlockCodePosition("001"));
      Assert.NotEqual(new BlockCodePosition("001"), new BlockCodePosition("002"));
      Assert.True(new BlockCodePosition("001") != new BlockCodePosition("002"));

      Assert.Throws<NotSupportedException>(
          () => new BlockCodePosition("001").CompareTo(new BlockCodePosition("002")));
      Assert.Throws<NotSupportedException>(
          () => new BlockCodePosition("001") > new BlockCodePosition("002"));
      Assert.Throws<NotSupportedException>(
          () => new BlockCodePosition("001") < new BlockCodePosition("002"));
    }

    [Theory]
    [InlineData("001", "Block 001")]
    [InlineData("someId", "Block someId")]
    public void TestBlockCodePositionToString(string blockId, string desc) {
      Assert.Equal(desc, new BlockCodePosition(blockId).ToString());
    }
  }
}
