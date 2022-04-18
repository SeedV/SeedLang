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

namespace SeedLang.Common.Tests {
  public class RangeTests {
    [Fact]
    public void TestRanges() {
      Range range = null;
      TextRange textRange = null;

      Assert.False(new TextRange(1, 1, 1, 1).Equals(null));
      Assert.False(new TextRange(1, 1, 1, 1).Equals(range));
      Assert.False(new TextRange(1, 1, 1, 1).Equals(textRange));

      textRange = new TextRange(1, 1, 1, 1);
      Assert.True(textRange.Equals(textRange));
      Assert.True(new TextRange(1, 1, 1, 1).Equals(textRange));
      Assert.True(new TextRange(1, 1, 1, 1).Equals(new TextRange(1, 1, 1, 1)));

      Assert.True(textRange != null);
      Assert.True(null != textRange);

      Assert.True(new TextRange(1, 2, 3, 4).Equals(new TextRange(1, 2, 3, 4)));
      Assert.True(new TextRange(1, 2, 3, 4) == new TextRange(1, 2, 3, 4));
      Assert.False(new TextRange(1, 2, 3, 4).Equals(new TextRange(1, 2, 3, 5)));
      Assert.True(new TextRange(1, 2, 3, 4) != new TextRange(1, 2, 3, 5));
    }

    [Theory]
    [InlineData(1, 1, 1, 1, "[Ln 1, Col 1 - Ln 1, Col 1]")]
    [InlineData(1, 2, 3, 4, "[Ln 1, Col 2 - Ln 3, Col 4]")]
    [InlineData(65535, 255, 65535, 1023, "[Ln 65535, Col 255 - Ln 65535, Col 1023]")]
    public void TestTextRangeToString(
        int line1, int column1, int line2, int column2, string desc) {
      Assert.Equal(desc, new TextRange(line1, column1, line2, column2).ToString());
    }
  }
}
