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

using System.Collections.Generic;
using Antlr4.Runtime;
using Xunit;

namespace SeedLang.X.Tests {
  public class SeedPythonDentLexerTests {
    [Fact]
    public void TestComments() {
      string source = "# comment\nprint(1)";
      var expectedTokens = new string[] {
          $"[@-1,9:9='\\n',<{SeedPythonParser.NEWLINE}>,1:9]",
          $"[@-1,10:14='print',<{SeedPythonParser.NAME}>,2:0]",
          $"[@-1,15:15='(',<{SeedPythonParser.OPEN_PAREN}>,2:5]",
          $"[@-1,16:16='1',<{SeedPythonParser.NUMBER}>,2:6]",
          $"[@-1,17:17=')',<{SeedPythonParser.CLOSE_PAREN}>,2:7]",
          $"[@-1,18:18='\\n',<{SeedPythonParser.NEWLINE}>,2:8]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestVTag() {
      string source = "# [[ Print ]]\nprint(1)";
      var expectedTokens = new string[] {
          $"[@-1,0:3='# [[',<{SeedPythonParser.VTAG_START}>,1:0]",
          $"[@-1,5:9='Print',<{SeedPythonParser.NAME}>,1:5]",
          $"[@-1,11:12=']]',<{SeedPythonParser.VTAG_END}>,1:11]",
          $"[@-1,13:13='\\n',<{SeedPythonParser.NEWLINE}>,1:13]",
          $"[@-1,14:18='print',<{SeedPythonParser.NAME}>,2:0]",
          $"[@-1,19:19='(',<{SeedPythonParser.OPEN_PAREN}>,2:5]",
          $"[@-1,20:20='1',<{SeedPythonParser.NUMBER}>,2:6]",
          $"[@-1,21:21=')',<{SeedPythonParser.CLOSE_PAREN}>,2:7]",
          $"[@-1,22:22='\\n',<{SeedPythonParser.NEWLINE}>,2:8]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestVTagWithSpaces() {
      string source = "# \t\t[[ Print ]]\nprint(1)";
      var expectedTokens = new string[] {
          $"[@-1,0:5='# \\t\\t[[',<{SeedPythonParser.VTAG_START}>,1:0]",
          $"[@-1,7:11='Print',<{SeedPythonParser.NAME}>,1:7]",
          $"[@-1,13:14=']]',<{SeedPythonParser.VTAG_END}>,1:13]",
          $"[@-1,15:15='\\n',<{SeedPythonParser.NEWLINE}>,1:15]",
          $"[@-1,16:20='print',<{SeedPythonParser.NAME}>,2:0]",
          $"[@-1,21:21='(',<{SeedPythonParser.OPEN_PAREN}>,2:5]",
          $"[@-1,22:22='1',<{SeedPythonParser.NUMBER}>,2:6]",
          $"[@-1,23:23=')',<{SeedPythonParser.CLOSE_PAREN}>,2:7]",
          $"[@-1,24:24='\\n',<{SeedPythonParser.NEWLINE}>,2:8]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestVTagWithArguments() {
      string source = "# [[ Print(a, b) ]]\n" +
                      "print(1)";
      var expectedTokens = new string[] {
          $"[@-1,0:3='# [[',<{SeedPythonParser.VTAG_START}>,1:0]",
          $"[@-1,5:9='Print',<{SeedPythonParser.NAME}>,1:5]",
          $"[@-1,10:10='(',<{SeedPythonParser.OPEN_PAREN}>,1:10]",
          $"[@-1,11:11='a',<{SeedPythonParser.NAME}>,1:11]",
          $"[@-1,12:12=',',<{SeedPythonParser.COMMA}>,1:12]",
          $"[@-1,14:14='b',<{SeedPythonParser.NAME}>,1:14]",
          $"[@-1,15:15=')',<{SeedPythonParser.CLOSE_PAREN}>,1:15]",
          $"[@-1,17:18=']]',<{SeedPythonParser.VTAG_END}>,1:17]",
          $"[@-1,19:19='\\n',<{SeedPythonParser.NEWLINE}>,1:19]",
          $"[@-1,20:24='print',<{SeedPythonParser.NAME}>,2:0]",
          $"[@-1,25:25='(',<{SeedPythonParser.OPEN_PAREN}>,2:5]",
          $"[@-1,26:26='1',<{SeedPythonParser.NUMBER}>,2:6]",
          $"[@-1,27:27=')',<{SeedPythonParser.CLOSE_PAREN}>,2:7]",
          $"[@-1,28:28='\\n',<{SeedPythonParser.NEWLINE}>,2:8]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestMultipleLineVTag() {
      string source = "# [[ Assign(x, y), Initialize\n" +
                      "x = 1\n" +
                      "y = 2\n" +
                      "# ]]";
      var expectedTokens = new string[] {
          $"[@-1,0:3='# [[',<{SeedPythonParser.VTAG_START}>,1:0]",
          $"[@-1,5:10='Assign',<{SeedPythonParser.NAME}>,1:5]",
          $"[@-1,11:11='(',<{SeedPythonParser.OPEN_PAREN}>,1:11]",
          $"[@-1,12:12='x',<{SeedPythonParser.NAME}>,1:12]",
          $"[@-1,13:13=',',<{SeedPythonParser.COMMA}>,1:13]",
          $"[@-1,15:15='y',<{SeedPythonParser.NAME}>,1:15]",
          $"[@-1,16:16=')',<{SeedPythonParser.CLOSE_PAREN}>,1:16]",
          $"[@-1,17:17=',',<{SeedPythonParser.COMMA}>,1:17]",
          $"[@-1,19:28='Initialize',<{SeedPythonParser.NAME}>,1:19]",
          $"[@-1,29:29='\\n',<{SeedPythonParser.NEWLINE}>,1:29]",
          $"[@-1,30:30='x',<{SeedPythonParser.NAME}>,2:0]",
          $"[@-1,32:32='=',<{SeedPythonParser.EQUAL}>,2:2]",
          $"[@-1,34:34='1',<{SeedPythonParser.NUMBER}>,2:4]",
          $"[@-1,35:35='\\n',<{SeedPythonParser.NEWLINE}>,2:5]",
          $"[@-1,36:36='y',<{SeedPythonParser.NAME}>,3:0]",
          $"[@-1,38:38='=',<{SeedPythonParser.EQUAL}>,3:2]",
          $"[@-1,40:40='2',<{SeedPythonParser.NUMBER}>,3:4]",
          $"[@-1,41:41='\\n',<{SeedPythonParser.NEWLINE}>,3:5]",
          $"[@-1,42:45='# ]]',<{SeedPythonParser.VTAG_END}>,4:0]",
          $"[@-1,46:46='\\n',<{SeedPythonParser.NEWLINE}>,4:4]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestNestedDict() {
      string source = "[[1]]";
      var expectedTokens = new string[] {
          $"[@-1,0:0='[',<{SeedPythonParser.OPEN_BRACK}>,1:0]",
          $"[@-1,1:1='[',<{SeedPythonParser.OPEN_BRACK}>,1:1]",
          $"[@-1,2:2='1',<{SeedPythonParser.NUMBER}>,1:2]",
          $"[@-1,3:3=']',<{SeedPythonParser.CLOSE_BRACK}>,1:3]",
          $"[@-1,4:4=']',<{SeedPythonParser.CLOSE_BRACK}>,1:4]",
          $"[@-1,5:5='\\n',<{SeedPythonParser.NEWLINE}>,1:5]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestVTagWithIndent() {
      string source = "while True:\n" +
                      "  # [[ Assign ]]\n" +
                      "  x = 1";
      var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{SeedPythonParser.WHILE}>,1:0]",
          $"[@-1,6:9='True',<{SeedPythonParser.TRUE}>,1:6]",
          $"[@-1,10:10=':',<{SeedPythonParser.COLON}>,1:10]",
          $"[@-1,11:11='\\n',<{SeedPythonParser.NEWLINE}>,1:11]",
          $"[@-1,12:13='  ',<{SeedPythonParser.INDENT}>,2:0]",
          $"[@-1,14:17='# [[',<{SeedPythonParser.VTAG_START}>,2:2]",
          $"[@-1,19:24='Assign',<{SeedPythonParser.NAME}>,2:7]",
          $"[@-1,26:27=']]',<{SeedPythonParser.VTAG_END}>,2:14]",
          $"[@-1,28:30='\\n  ',<{SeedPythonParser.NEWLINE}>,2:16]",
          $"[@-1,31:31='x',<{SeedPythonParser.NAME}>,3:2]",
          $"[@-1,33:33='=',<{SeedPythonParser.EQUAL}>,3:4]",
          $"[@-1,35:35='1',<{SeedPythonParser.NUMBER}>,3:6]",
          $"[@-1,36:36='\\n',<{SeedPythonParser.NEWLINE}>,3:7]",
          $"[@-1,36:36='',<{SeedPythonParser.DEDENT}>,4:0]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestMultipleVTagWithIndent() {
      string source = "while True:\n" +
                      "  # [[ Assign\n" +
                      "  x = 1\n" +
                      "  # ]]";
      var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{SeedPythonParser.WHILE}>,1:0]",
          $"[@-1,6:9='True',<{SeedPythonParser.TRUE}>,1:6]",
          $"[@-1,10:10=':',<{SeedPythonParser.COLON}>,1:10]",
          $"[@-1,11:11='\\n',<{SeedPythonParser.NEWLINE}>,1:11]",
          $"[@-1,12:13='  ',<{SeedPythonParser.INDENT}>,2:0]",
          $"[@-1,14:17='# [[',<{SeedPythonParser.VTAG_START}>,2:2]",
          $"[@-1,19:24='Assign',<{SeedPythonParser.NAME}>,2:7]",
          $"[@-1,25:27='\\n  ',<{SeedPythonParser.NEWLINE}>,2:13]",
          $"[@-1,28:28='x',<{SeedPythonParser.NAME}>,3:2]",
          $"[@-1,30:30='=',<{SeedPythonParser.EQUAL}>,3:4]",
          $"[@-1,32:32='1',<{SeedPythonParser.NUMBER}>,3:6]",
          $"[@-1,33:35='\\n  ',<{SeedPythonParser.NEWLINE}>,3:7]",
          $"[@-1,36:39='# ]]',<{SeedPythonParser.VTAG_END}>,4:2]",
          $"[@-1,40:40='\\n',<{SeedPythonParser.NEWLINE}>,4:6]",
          $"[@-1,40:40='',<{SeedPythonParser.DEDENT}>,5:0]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestExpressionStatement() {
      string source = "1 + 2\n";
      var expectedTokens = new string[] {
          $"[@-1,0:0='1',<{SeedPythonParser.NUMBER}>,1:0]",
          $"[@-1,2:2='+',<{SeedPythonParser.ADD}>,1:2]",
          $"[@-1,4:4='2',<{SeedPythonParser.NUMBER}>,1:4]",
          $"[@-1,5:5='\\n',<{SeedPythonParser.NEWLINE}>,1:5]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestWhileTrue() {
      string source = "while True";
      var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{SeedPythonParser.WHILE}>,1:0]",
          $"[@-1,6:9='True',<{SeedPythonParser.TRUE}>,1:6]",
          $"[@-1,10:10='\\n',<{SeedPythonParser.NEWLINE}>,1:10]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestSingleWhile() {
      string source = "  while True:\n \tx = 1";
      var expectedTokens = new string[] {
          $"[@-1,0:0='\\n',<{SeedPythonParser.NEWLINE}>,1:0]",
          $"[@-1,0:1='  ',<{SeedPythonParser.INDENT}>,1:0]",
          $"[@-1,2:6='while',<{SeedPythonParser.WHILE}>,1:2]",
          $"[@-1,8:11='True',<{SeedPythonParser.TRUE}>,1:8]",
          $"[@-1,12:12=':',<{SeedPythonParser.COLON}>,1:12]",
          $"[@-1,13:13='\\n',<{SeedPythonParser.NEWLINE}>,1:13]",
          $"[@-1,14:15=' \\t',<{SeedPythonParser.INDENT}>,2:0]",
          $"[@-1,16:16='x',<{SeedPythonParser.NAME}>,2:2]",
          $"[@-1,18:18='=',<{SeedPythonParser.EQUAL}>,2:4]",
          $"[@-1,20:20='1',<{SeedPythonParser.NUMBER}>,2:6]",
          $"[@-1,21:21='\\n',<{SeedPythonParser.NEWLINE}>,2:7]",
          $"[@-1,21:21='',<{SeedPythonParser.DEDENT}>,3:0]",
          $"[@-1,21:21='',<{SeedPythonParser.DEDENT}>,3:0]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestWhileBlock() {
      string source = "while True:\n" +
                      "  x = 1\n" +
                      "  y = 2";
      var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{SeedPythonParser.WHILE}>,1:0]",
          $"[@-1,6:9='True',<{SeedPythonParser.TRUE}>,1:6]",
          $"[@-1,10:10=':',<{SeedPythonParser.COLON}>,1:10]",
          $"[@-1,11:11='\\n',<{SeedPythonParser.NEWLINE}>,1:11]",
          $"[@-1,12:13='  ',<{SeedPythonParser.INDENT}>,2:0]",
          $"[@-1,14:14='x',<{SeedPythonParser.NAME}>,2:2]",
          $"[@-1,16:16='=',<{SeedPythonParser.EQUAL}>,2:4]",
          $"[@-1,18:18='1',<{SeedPythonParser.NUMBER}>,2:6]",
          $"[@-1,19:21='\\n  ',<{SeedPythonParser.NEWLINE}>,2:7]",
          $"[@-1,22:22='y',<{SeedPythonParser.NAME}>,3:2]",
          $"[@-1,24:24='=',<{SeedPythonParser.EQUAL}>,3:4]",
          $"[@-1,26:26='2',<{SeedPythonParser.NUMBER}>,3:6]",
          $"[@-1,27:27='\\n',<{SeedPythonParser.NEWLINE}>,3:7]",
          $"[@-1,27:27='',<{SeedPythonParser.DEDENT}>,4:0]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestWhileWithIf() {
      string source = "while True:\n" +
                      "  if False:\n" +
                      "    x = 1\n" +
                      "  else:\n" +
                      "      y = 2";
      var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{SeedPythonParser.WHILE}>,1:0]",
          $"[@-1,6:9='True',<{SeedPythonParser.TRUE}>,1:6]",
          $"[@-1,10:10=':',<{SeedPythonParser.COLON}>,1:10]",
          $"[@-1,11:11='\\n',<{SeedPythonParser.NEWLINE}>,1:11]",
          $"[@-1,12:13='  ',<{SeedPythonParser.INDENT}>,2:0]",
          $"[@-1,14:15='if',<{SeedPythonParser.IF}>,2:2]",
          $"[@-1,17:21='False',<{SeedPythonParser.FALSE}>,2:5]",
          $"[@-1,22:22=':',<{SeedPythonParser.COLON}>,2:10]",
          $"[@-1,23:23='\\n',<{SeedPythonParser.NEWLINE}>,2:11]",
          $"[@-1,24:27='    ',<{SeedPythonParser.INDENT}>,3:0]",
          $"[@-1,28:28='x',<{SeedPythonParser.NAME}>,3:4]",
          $"[@-1,30:30='=',<{SeedPythonParser.EQUAL}>,3:6]",
          $"[@-1,32:32='1',<{SeedPythonParser.NUMBER}>,3:8]",
          $"[@-1,33:33='\\n',<{SeedPythonParser.NEWLINE}>,3:9]",
          $"[@-1,34:35='  ',<{SeedPythonParser.DEDENT}>,4:0]",
          $"[@-1,36:39='else',<{SeedPythonParser.ELSE}>,4:2]",
          $"[@-1,40:40=':',<{SeedPythonParser.COLON}>,4:6]",
          $"[@-1,41:41='\\n',<{SeedPythonParser.NEWLINE}>,4:7]",
          $"[@-1,42:47='      ',<{SeedPythonParser.INDENT}>,5:0]",
          $"[@-1,48:48='y',<{SeedPythonParser.NAME}>,5:6]",
          $"[@-1,50:50='=',<{SeedPythonParser.EQUAL}>,5:8]",
          $"[@-1,52:52='2',<{SeedPythonParser.NUMBER}>,5:10]",
          $"[@-1,53:53='\\n',<{SeedPythonParser.NEWLINE}>,5:11]",
          $"[@-1,53:53='',<{SeedPythonParser.DEDENT}>,6:0]",
          $"[@-1,53:53='',<{SeedPythonParser.DEDENT}>,6:0]",
      };
      TestScanTokens(source, expectedTokens);
    }

    [Fact]
    public void TestAdditionalTab() {
      string source = "a = 0\n" +
                      "for i in range(10):\n" +
                      "\ta = a + i\n" +
                      "\t";
      var expectedTokens = new string[] {
          $"[@-1,0:0='a',<{SeedPythonParser.NAME}>,1:0]",
          $"[@-1,2:2='=',<{SeedPythonParser.EQUAL}>,1:2]",
          $"[@-1,4:4='0',<{SeedPythonParser.NUMBER}>,1:4]",
          $"[@-1,5:5='\\n',<{SeedPythonParser.NEWLINE}>,1:5]",
          $"[@-1,6:8='for',<{SeedPythonParser.FOR}>,2:0]",
          $"[@-1,10:10='i',<{SeedPythonParser.NAME}>,2:4]",
          $"[@-1,12:13='in',<{SeedPythonParser.IN}>,2:6]",
          $"[@-1,15:19='range',<{SeedPythonParser.NAME}>,2:9]",
          $"[@-1,20:20='(',<{SeedPythonParser.OPEN_PAREN}>,2:14]",
          $"[@-1,21:22='10',<{SeedPythonParser.NUMBER}>,2:15]",
          $"[@-1,23:23=')',<{SeedPythonParser.CLOSE_PAREN}>,2:17]",
          $"[@-1,24:24=':',<{SeedPythonParser.COLON}>,2:18]",
          $"[@-1,25:25='\\n',<{SeedPythonParser.NEWLINE}>,2:19]",
          $"[@-1,26:26='\\t',<{SeedPythonParser.INDENT}>,3:0]",
          $"[@-1,27:27='a',<{SeedPythonParser.NAME}>,3:1]",
          $"[@-1,29:29='=',<{SeedPythonParser.EQUAL}>,3:3]",
          $"[@-1,31:31='a',<{SeedPythonParser.NAME}>,3:5]",
          $"[@-1,33:33='+',<{SeedPythonParser.ADD}>,3:7]",
          $"[@-1,35:35='i',<{SeedPythonParser.NAME}>,3:9]",
          $"[@-1,36:37='\\n\\t',<{SeedPythonParser.NEWLINE}>,3:10]",
          $"[@-1,38:38='\\n',<{SeedPythonParser.NEWLINE}>,4:1]",
          $"[@-1,38:38='',<{SeedPythonParser.DEDENT}>,5:0]",
      };
      TestScanTokens(source, expectedTokens);
    }

    private static void TestScanTokens(string source, string[] expectedTokens) {
      var inputStream = new AntlrInputStream(source);
      var lexer = new SeedPythonDentLexer(inputStream);
      IList<IToken> tokens = lexer.GetAllTokens();
      Assert.Equal(expectedTokens.Length, tokens.Count);
      for (int i = 0; i < expectedTokens.Length; ++i) {
        string token = tokens[i].ToString().Replace(@"\r\n", @"\n");
        Assert.Equal(expectedTokens[i], token);
      }
    }
  }
}
