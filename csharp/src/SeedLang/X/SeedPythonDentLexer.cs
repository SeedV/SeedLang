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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace SeedLang.X {
  // A lexer can scan and inject indent and dedent tokens for SeedPython language.
  //
  // The indent and dedent tokens cannot be expressed as ANTLR4 grammar. The newline and spaces need
  // be scanned to generate indent and dedent tokens.
  internal class SeedPythonDentLexer : SeedPythonLexer {
    // The token queue to store extra multiple tokens, because the original ANTLR4 lexer can only
    // generate one token at the same time.
    private readonly Queue<IToken> _tokens = new Queue<IToken>();
    // The stack that keeps track of the indentation level.
    private readonly Stack<int> _indents = new Stack<int>();
    // The amount of opened braces, brackets and parenthesis.
    private int _opened = 0;
    // A flag to indicate that current token is the first token to be generated.
    private bool _firstToken = true;
    // A flag to indicate if an extra trailing newline is needed before EOF.
    private bool _addTrailingNewline = true;

    public SeedPythonDentLexer(ICharStream input) : base(input) { }

    public override IToken NextToken() {
      if (_tokens.Count > 0) {
        return _tokens.Dequeue();
      }
      while (_tokens.Count == 0) {
        IToken currentToken = base.NextToken();
        if (_firstToken) {
          _firstToken = false;
          HandleIndentForFirstToken(currentToken);
        }
        switch (currentToken.Type) {
          case SeedPythonParser.NEWLINE:
            HandleNewline(currentToken);
            break;
          case SeedPythonParser.OPEN_PAREN:
          case SeedPythonParser.OPEN_BRACE:
          case SeedPythonParser.OPEN_BRACK:
            EmitToken(currentToken);
            _opened++;
            break;
          case SeedPythonParser.CLOSE_PAREN:
          case SeedPythonParser.CLOSE_BRACE:
          case SeedPythonParser.CLOSE_BRACK:
            EmitToken(currentToken);
            _opened--;
            break;
          case SeedPythonParser.Eof:
            HandleEof(currentToken);
            break;
          case SeedPythonParser.COMMENT:
            ScanVTag(currentToken);
            break;
          default:
            EmitToken(currentToken);
            break;
        }
      }
      return _tokens.Dequeue();
    }

    public override void Reset() {
      base.Reset();
      _tokens.Clear();
      _indents.Clear();
      _opened = 0;
      _firstToken = true;
      _addTrailingNewline = true;
    }

    private void HandleIndentForFirstToken(IToken firstToken) {
      if (firstToken.Type != SeedPythonParser.NEWLINE && firstToken.StartIndex > 0) {
        var interval = new Interval(0, firstToken.StartIndex - 1);
        string spaces = (InputStream as ICharStream).GetText(interval);
        int indent = GetIndentationCount(spaces);
        _indents.Push(indent);
        EmitToken(CommonToken(SeedPythonParser.NEWLINE, 0, 0, 1, 0, "\n"));
        EmitToken(CommonToken(SeedPythonParser.INDENT, 0, firstToken.StartIndex - 1, 1, 0, spaces));
      }
    }

    private void HandleNewline(IToken newlineToken) {
      int spacesStart = 0;
      while (spacesStart < Text.Length && IsNewline(Text[spacesStart])) {
        spacesStart++;
      }
      int next = InputStream.LA(1);
      if (next == SeedPythonParser.Eof) {
        _addTrailingNewline = false;
      }
      // Ignores all indents, dedents and line breaks when inside brackets or on a blank line.
      if (_opened == 0 && !IsNewline((char)next)) {
        int indent = GetIndentationCount(Text.Substring(spacesStart));
        int previous = _indents.Count == 0 ? 0 : _indents.Peek();
        if (indent == previous) {
          EmitToken(newlineToken);
        } else if (indent > previous) {
          _indents.Push(indent);
          EmitToken(CommonToken(SeedPythonParser.NEWLINE,
                                newlineToken.StartIndex, newlineToken.StartIndex + spacesStart - 1,
                                newlineToken.Line, newlineToken.Column));
          EmitToken(CommonToken(SeedPythonParser.INDENT, newlineToken.StartIndex + spacesStart,
                                newlineToken.StopIndex, newlineToken.Line + 1, 0));
        } else {
          EmitToken(CommonToken(SeedPythonParser.NEWLINE,
                                newlineToken.StartIndex, newlineToken.StartIndex + spacesStart - 1,
                                newlineToken.Line, newlineToken.Column));
          while (_indents.Count > 0 && _indents.Peek() > indent) {
            EmitToken(CommonToken(SeedPythonParser.DEDENT, newlineToken.StartIndex + spacesStart,
                                  newlineToken.StopIndex, newlineToken.Line + 1, 0));
            _indents.Pop();
          }
        }
      }
    }

    private void HandleEof(IToken eofToken) {
      if (_addTrailingNewline) {
        // Emits an extra line break that serves as the end of the statement.
        EmitToken(CommonToken(SeedPythonParser.NEWLINE, eofToken.StartIndex, eofToken.StartIndex,
                              Environment.NewLine));
        // Emits as many dedent tokens as needed.
        while (_indents.Count != 0) {
          EmitToken(CommonToken(SeedPythonParser.DEDENT, eofToken.StartIndex, eofToken.StartIndex,
                                eofToken.Line + 1, 0, ""));
          _indents.Pop();
        }
      }
      EmitToken(eofToken);
    }

    private void ScanVTag(IToken comment) {
      if (comment.Text.Length > 1) {
        Debug.Assert(comment.Text[0] == '#');
        var inputStream = new AntlrInputStream(comment.Text.Substring(1));
        var lexer = new SeedPythonLexer(inputStream);
        var tokens = lexer.GetAllTokens();
        if (tokens.Count > 0 && (tokens[0].Type == SeedPythonParser.VTAG_START ||
                                 tokens[0].Type == SeedPythonParser.VTAG_END)) {
          for (int i = 0; i < tokens.Count; i++) {
            // Adds back '#' for VTAG_START and VTAG_END tokens.
            int start = i == 0 ? comment.StartIndex : comment.StartIndex + tokens[i].StartIndex + 1;
            int column = i == 0 ? comment.Column : comment.Column + tokens[i].StartIndex + 1;
            // The length of first token is StopIndex + 1 + (length of '#').
            string text = i == 0 ? comment.Text.Substring(0, tokens[i].StopIndex + 2) :
                                   tokens[i].Text;
            _tokens.Enqueue(CommonToken(tokens[i].Type, start,
                                        comment.StartIndex + tokens[i].StopIndex + 1, comment.Line,
                                        column, text));
          }
        }
      }
    }

    private static bool IsNewline(char ch) {
      return ch == '\r' || ch == '\n' || ch == '\f';
    }

    private void EmitToken(IToken token) {
      _tokens.Enqueue(token);
    }

    private CommonToken CommonToken(int type, int start, int stop) {
      var source = Tuple.Create((ITokenSource)this, (ICharStream)InputStream);
      return new CommonToken(source, type, DefaultTokenChannel, start, stop);
    }

    private CommonToken CommonToken(int type, int start, int stop, string text) {
      CommonToken token = CommonToken(type, start, stop);
      token.Text = text;
      return token;
    }

    private CommonToken CommonToken(int type, int start, int stop, int line, int column) {
      CommonToken token = CommonToken(type, start, stop);
      token.Line = line;
      token.Column = column;
      return token;
    }

    private CommonToken CommonToken(int type, int start, int stop, int line, int column,
                                    string text) {
      CommonToken token = CommonToken(type, start, stop, text);
      token.Line = line;
      token.Column = column;
      return token;
    }

    // Calculates the indentation of the provided spaces, taking the following rules into account:
    //
    // Tabs are replaced (from left to right) by one to eight spaces such that the total number of
    // characters up to and including the replacement is a multiple of eight.
    // https://docs.python.org/3.10/reference/lexical_analysis.html#indentation
    private static int GetIndentationCount(string spaces) {
      int count = 0;
      foreach (char ch in spaces.ToCharArray()) {
        count += ch == '\t' ? 8 - (count % 8) : 1;
      }
      return count;
    }
  }
}
