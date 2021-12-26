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
    public class TestData : TheoryData<string, string[]> {
      private const int _add = SeedPythonParser.ADD;
      private const int _colon = SeedPythonParser.COLON;
      private const int _dedent = SeedPythonParser.DEDENT;
      private const int _else = SeedPythonParser.ELSE;
      private const int _equal = SeedPythonParser.EQUAL;
      private const int _false = SeedPythonParser.FALSE;
      private const int _if = SeedPythonParser.IF;
      private const int _indent = SeedPythonParser.INDENT;
      private const int _name = SeedPythonParser.NAME;
      private const int _newline = SeedPythonParser.NEWLINE;
      private const int _number = SeedPythonParser.NUMBER;
      private const int _true = SeedPythonParser.TRUE;
      private const int _while = SeedPythonParser.WHILE;

      public TestData() {
        AddExpressionStatement();
        AddWhileBlock();
        AddWhileTrue();
        AddWhileWithIf();
        AddSingleLineWhile();
      }

      private void AddExpressionStatement() {
        string source = "1 + 2\n";
        var expectedTokens = new string[] {
          $"[@-1,0:0='1',<{_number}>,1:0]",
          $"[@-1,2:2='+',<{_add}>,1:2]",
          $"[@-1,4:4='2',<{_number}>,1:4]",
          $"[@-1,5:5='\\n',<{_newline}>,1:5]",
        };
        Add(source, expectedTokens);
      }


      private void AddWhileBlock() {
        string source = "while True:\n" +
                        "  x = 1\n" +
                        "  y = 2";
        var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{_while}>,1:0]",
          $"[@-1,6:9='True',<{_true}>,1:6]",
          $"[@-1,10:10=':',<{_colon}>,1:10]",
          $"[@-1,11:11='\\n',<{_newline}>,1:11]",
          $"[@-1,12:13='  ',<{_indent}>,2:0]",
          $"[@-1,14:14='x',<{_name}>,2:2]",
          $"[@-1,16:16='=',<{_equal}>,2:4]",
          $"[@-1,18:18='1',<{_number}>,2:6]",
          $"[@-1,19:21='\\n  ',<{_newline}>,2:7]",
          $"[@-1,22:22='y',<{_name}>,3:2]",
          $"[@-1,24:24='=',<{_equal}>,3:4]",
          $"[@-1,26:26='2',<{_number}>,3:6]",
          $"[@-1,27:27='\\n',<{_newline}>,3:7]",
          $"[@-1,27:27='',<{_dedent}>,4:0]",
        };
        Add(source, expectedTokens);
      }

      private void AddWhileTrue() {
        string source = "while True";
        var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{_while}>,1:0]",
          $"[@-1,6:9='True',<{_true}>,1:6]",
          $"[@-1,10:10='\\n',<{_newline}>,1:10]",
        };
        Add(source, expectedTokens);
      }

      private void AddWhileWithIf() {
        string source = "while True:\n" +
                        "  if False:\n" +
                        "    x = 1\n" +
                        "  else:\n" +
                        "      y = 2";
        var expectedTokens = new string[] {
          $"[@-1,0:4='while',<{_while}>,1:0]",
          $"[@-1,6:9='True',<{_true}>,1:6]",
          $"[@-1,10:10=':',<{_colon}>,1:10]",
          $"[@-1,11:11='\\n',<{_newline}>,1:11]",
          $"[@-1,12:13='  ',<{_indent}>,2:0]",
          $"[@-1,14:15='if',<{_if}>,2:2]",
          $"[@-1,17:21='False',<{_false}>,2:5]",
          $"[@-1,22:22=':',<{_colon}>,2:10]",
          $"[@-1,23:23='\\n',<{_newline}>,2:11]",
          $"[@-1,24:27='    ',<{_indent}>,3:0]",
          $"[@-1,28:28='x',<{_name}>,3:4]",
          $"[@-1,30:30='=',<{_equal}>,3:6]",
          $"[@-1,32:32='1',<{_number}>,3:8]",
          $"[@-1,33:33='\\n',<{_newline}>,3:9]",
          $"[@-1,34:35='  ',<{_dedent}>,4:0]",
          $"[@-1,36:39='else',<{_else}>,4:2]",
          $"[@-1,40:40=':',<{_colon}>,4:6]",
          $"[@-1,41:41='\\n',<{_newline}>,4:7]",
          $"[@-1,42:47='      ',<{_indent}>,5:0]",
          $"[@-1,48:48='y',<{_name}>,5:6]",
          $"[@-1,50:50='=',<{_equal}>,5:8]",
          $"[@-1,52:52='2',<{_number}>,5:10]",
          $"[@-1,53:53='\\n',<{_newline}>,5:11]",
          $"[@-1,53:53='',<{_dedent}>,6:0]",
          $"[@-1,53:53='',<{_dedent}>,6:0]",
        };
        Add(source, expectedTokens);
      }

      private void AddSingleLineWhile() {
        string source = "  while True:\n \tx = 1";
        var expectedTokens = new string[] {
          $"[@-1,0:0='\\n',<{_newline}>,1:0]",
          $"[@-1,0:1='  ',<{_indent}>,1:0]",
          $"[@-1,2:6='while',<{_while}>,1:2]",
          $"[@-1,8:11='True',<{_true}>,1:8]",
          $"[@-1,12:12=':',<{_colon}>,1:12]",
          $"[@-1,13:13='\\n',<{_newline}>,1:13]",
          $"[@-1,14:15=' \\t',<{_indent}>,2:0]",
          $"[@-1,16:16='x',<{_name}>,2:2]",
          $"[@-1,18:18='=',<{_equal}>,2:4]",
          $"[@-1,20:20='1',<{_number}>,2:6]",
          $"[@-1,21:21='\\n',<{_newline}>,2:7]",
          $"[@-1,21:21='',<{_dedent}>,3:0]",
          $"[@-1,21:21='',<{_dedent}>,3:0]",
        };
        Add(source, expectedTokens);
      }

      [Theory]
      [ClassData(typeof(TestData))]
      public void TestScanTokens(string source, string[] expectedTokens) {
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
}
