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

using System.Collections.Generic;
using Antlr4.Runtime;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonDentLexerTests {
    [Fact]
    public void TestScanTokens() {
      var inputStream = new AntlrInputStream("while True:\n\tx = 1\n");
      var lexer = new SeedPythonDentLexer(inputStream);
      IList<IToken> tokens = lexer.GetAllTokens();
      var expectedTokens = new string[] {
        "[@-1,0:4='while',<7>,1:0]",
        "[@-1,6:9='True',<11>,1:6]",
        "[@-1,10:10=':',<9>,1:10]",
        "[@-1,11:12='\\n\\t',<44>,1:11]",
        "",
      };
      for (int i = 0; i < expectedTokens.Length; ++i) {
        Assert.Equal(expectedTokens[i], tokens[i].ToString());
      }
    }
  }
}
