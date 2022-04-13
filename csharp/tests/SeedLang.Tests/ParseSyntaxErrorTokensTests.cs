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

namespace SeedLang.Tests {
  public class ParseSyntaxErrorTokensTests {
    [Theory]
    [InlineData("5 + (",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "OpenParenthesis [Ln 1, Col 4 - Ln 1, Col 4]")]

    [InlineData("5 + (3 * 2",

                "Number [Ln 1, Col 0 - Ln 1, Col 0]," +
                "Operator [Ln 1, Col 2 - Ln 1, Col 2]," +
                "OpenParenthesis [Ln 1, Col 4 - Ln 1, Col 4]," +
                "Number [Ln 1, Col 5 - Ln 1, Col 5]," +
                "Operator [Ln 1, Col 7 - Ln 1, Col 7]," +
                "Number [Ln 1, Col 9 - Ln 1, Col 9]")]
    public void TestParseSyntaxTokens(string source, string expectedTokens) {
      var engine = new Engine(SeedXLanguage.SeedCalc, RunMode.Interactive);
      var tokens = engine.ParseSyntaxTokens(source, "");
      Assert.Equal(expectedTokens, string.Join(",", tokens));
    }
  }
}
