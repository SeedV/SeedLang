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

namespace SeedLang.Common.Tests {
  public class RangeTests {
    [Fact]
    public void TestTextRange() {
      const TextRange range = null;
      Assert.False(TextRange.Empty.Equals(range));
      Assert.True(new TextRange(1, 1, 1, 1) != range);
      Assert.True(TextRange.Empty.Equals(TextRange.Empty));
      Assert.True(TextRange.Empty.IsEmpty());
      Assert.False(new TextRange(1, 2, 3, 4).IsEmpty());
      Assert.True(new TextRange(1, 2, 3, 4).Equals(new TextRange(1, 2, 3, 4)));
      Assert.True(new TextRange(1, 2, 3, 4) == new TextRange(1, 2, 3, 4));
      Assert.False(new TextRange(1, 2, 1, 3).Equals(new TextRange(1, 2, 1, 4)));
      Assert.True(new TextRange(1, 2, 1, 3) != new TextRange(1, 2, 1, 4));
      Assert.False(new TextRange(1, 2, 1, 3).Equals(new BlockRange("123")));
      Assert.True(new TextRange(1, 2, 1, 3) != new BlockRange("123"));
      Assert.True(TextRange.Empty != BlockRange.Empty);
    }

    [Theory]
    [InlineData(1, 1, 1, 1, "[Ln 1, Col 1 - Ln 1, Col 1]")]
    [InlineData(1, 2, 3, 4, "[Ln 1, Col 2 - Ln 3, Col 4]")]
    [InlineData(65535, 255, 65535, 1023, "[Ln 65535, Col 255 - Ln 65535, Col 1023]")]
    public void TestTextRangeToString(
        int line1, int column1, int line2, int column2, string desc) {
      Assert.Equal(desc, new TextRange(line1, column1, line2, column2).ToString());
    }

    [Fact]
    public void TestBlockRange() {
      const Range range = null;
      Assert.False(BlockRange.Empty.Equals(range));
      Assert.True(new BlockRange("123") != range);
      Assert.True(BlockRange.Empty.Equals(BlockRange.Empty));
      Assert.True(BlockRange.Empty.IsEmpty());
      Assert.False(new BlockRange("someId").IsEmpty());
      Assert.True(new BlockRange("123").Equals(new BlockRange("123")));
      Assert.True(new BlockRange("123") == new BlockRange("123"));
      Assert.False(new BlockRange("123").Equals(new BlockRange("456")));
      Assert.True(new BlockRange("123") != new BlockRange("456"));
      Assert.True(new BlockRange("123") != new TextRange(1, 2, 1, 3));
      Assert.True(BlockRange.Empty != TextRange.Empty);
    }

    [Theory]
    [InlineData("001", "[Block: 001]")]
    [InlineData("someId", "[Block: someId]")]
    public void TestBlockRangeToString(string blockId, string desc) {
      Assert.Equal(desc, new BlockRange(blockId).ToString());
    }
  }
}
