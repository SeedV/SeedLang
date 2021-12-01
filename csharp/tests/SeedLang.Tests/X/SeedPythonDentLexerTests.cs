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
using System.Runtime.InteropServices;
using Antlr4.Runtime;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonDentLexerTests {
    [Theory]
    [InlineData("while True:\n" +
                "  if False:\n" +
                "    x = 1\n" +
                "  else:\n" +
                "      y = 2",

                new string[] {
                  @"[@-1,0:4='while',<7>,1:0]",
                  @"[@-1,6:9='True',<11>,1:6]",
                  @"[@-1,10:10=':',<9>,1:10]",
                  @"[@-1,11:11='\n',<44>,1:11]",
                  @"[@-1,12:13='  ',<47>,2:0]",
                  @"[@-1,14:15='if',<4>,2:2]",
                  @"[@-1,17:21='False',<12>,2:5]",
                  @"[@-1,22:22=':',<9>,2:10]",
                  @"[@-1,23:23='\n',<44>,2:11]",
                  @"[@-1,24:27='    ',<47>,3:0]",
                  @"[@-1,28:28='x',<38>,3:4]",
                  @"[@-1,30:30='=',<17>,3:6]",
                  @"[@-1,32:32='1',<39>,3:8]",
                  @"[@-1,33:33='\n',<44>,3:9]",
                  @"[@-1,34:35='  ',<48>,4:0]",
                  @"[@-1,36:39='else',<6>,4:2]",
                  @"[@-1,40:40=':',<9>,4:6]",
                  @"[@-1,41:41='\n',<44>,4:7]",
                  @"[@-1,42:47='      ',<47>,5:0]",
                  @"[@-1,48:48='y',<38>,5:6]",
                  @"[@-1,50:50='=',<17>,5:8]",
                  @"[@-1,52:52='2',<39>,5:10]",
                  @"[@-1,53:53='\n',<44>,5:11]",
                  @"[@-1,53:53='',<48>,6:0]",
                  @"[@-1,53:53='',<48>,6:0]",})]

    [InlineData("  while True:\n \tx = 1",

                new string[] {
                  @"[@-1,0:0='\n',<44>,1:0]",
                  @"[@-1,0:1='  ',<47>,1:0]",
                  @"[@-1,2:6='while',<7>,1:2]",
                  @"[@-1,8:11='True',<11>,1:8]",
                  @"[@-1,12:12=':',<9>,1:12]",
                  @"[@-1,13:13='\n',<44>,1:13]",
                  @"[@-1,14:15=' \t',<47>,2:0]",
                  @"[@-1,16:16='x',<38>,2:2]",
                  @"[@-1,18:18='=',<17>,2:4]",
                  @"[@-1,20:20='1',<39>,2:6]",
                  @"[@-1,21:21='\n',<44>,2:7]",
                  @"[@-1,21:21='',<48>,3:0]",
                  @"[@-1,21:21='',<48>,3:0]",})]

    [InlineData("while True:\n" +
                "  x = 1\n" +
                "  y = 2",

                new string[] {
                  @"[@-1,0:4='while',<7>,1:0]",
                  @"[@-1,6:9='True',<11>,1:6]",
                  @"[@-1,10:10=':',<9>,1:10]",
                  @"[@-1,11:11='\n',<44>,1:11]",
                  @"[@-1,12:13='  ',<47>,2:0]",
                  @"[@-1,14:14='x',<38>,2:2]",
                  @"[@-1,16:16='=',<17>,2:4]",
                  @"[@-1,18:18='1',<39>,2:6]",
                  @"[@-1,19:21='\n  ',<44>,2:7]",
                  @"[@-1,22:22='y',<38>,3:2]",
                  @"[@-1,24:24='=',<17>,3:4]",
                  @"[@-1,26:26='2',<39>,3:6]",
                  @"[@-1,27:27='\n',<44>,3:7]",
                  @"[@-1,27:27='',<48>,4:0]",})]

    [InlineData("while True",

                new string[] {
                  @"[@-1,0:4='while',<7>,1:0]",
                  @"[@-1,6:9='True',<11>,1:6]",
                  @"[@-1,10:10='\n',<44>,1:10]",})]
    public void TestScanTokens(string source, string[] expectedTokens) {
      var inputStream = new AntlrInputStream(source);
      var lexer = new SeedPythonDentLexer(inputStream);
      IList<IToken> tokens = lexer.GetAllTokens();
      Assert.Equal(expectedTokens.Length, tokens.Count);
      for (int i = 0; i < expectedTokens.Length; ++i) {
        Assert.Equal(expectedTokens[i], tokens[i].ToString());
      }
    }
  }
}
