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
  public class CodeRangeTests {
    [Fact]
    public void TestCodeRange() {
      CodeRange range1 = null;
      CodeRange range2 = null;
      Assert.Equal(range1, range2);
      Assert.True(range1 == range2);
      Assert.NotEqual(range1, CodeRange.Empty);
      Assert.True(range1 != CodeRange.Empty);
      Assert.NotEqual(range1, new CodeRange(new TextCodePosition(1, 1),
                                            new TextCodePosition(1, 1)));
      Assert.True(range1 != new CodeRange(new TextCodePosition(1, 1),
                                          new TextCodePosition(1, 1)));
      Assert.Equal(CodeRange.Empty, CodeRange.Empty);
      Assert.True(CodeRange.Empty.IsEmpty());
      Assert.False(new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)).IsEmpty());
      Assert.False(new CodeRange(new BlockCodePosition("someId")).IsEmpty());
      Assert.Equal(new CodeRange(new BlockCodePosition("123")),
                   new CodeRange(new BlockCodePosition("123")));
      Assert.True(new CodeRange(new BlockCodePosition("123")) ==
                  new CodeRange(new BlockCodePosition("123")));
      Assert.Equal(new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)),
                   new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)));
      Assert.True(new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)) ==
                  new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)));
      Assert.NotEqual(new CodeRange(new BlockCodePosition("123")),
                      new CodeRange(new BlockCodePosition("456")));
      Assert.True(new CodeRange(new BlockCodePosition("123")) !=
                  new CodeRange(new BlockCodePosition("456")));
      Assert.NotEqual(new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)),
                      new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 4)));
      Assert.True(new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 3)) !=
                  new CodeRange(new TextCodePosition(1, 2), new TextCodePosition(1, 4)));
    }

    [Theory]
    [InlineData(1, 1, 1, 1, "[Ln 1, Col 1 - Ln 1, Col 1]")]
    [InlineData(1, 2, 3, 4, "[Ln 1, Col 2 - Ln 3, Col 4]")]
    [InlineData(65535, 255, 65535, 1023, "[Ln 65535, Col 255 - Ln 65535, Col 1023]")]
    public void TestTextCodeRangeToString(
        int line1, int column1, int line2, int column2, string desc) {
      Assert.Equal(desc,
                   new CodeRange(new TextCodePosition(line1, column1),
                                 new TextCodePosition(line2, column2)).ToString());
    }

    [Theory]
    [InlineData("001", "[Block: 001]")]
    [InlineData("someId", "[Block: someId]")]
    public void TestBlockCodeRangeToString(string blockId, string desc) {
      Assert.Equal(desc, new CodeRange(new BlockCodePosition(blockId)).ToString());
    }
  }
}
