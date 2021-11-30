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
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace SeedLang.X {
  // A lexer can scan and inject indent and dedent tokens for SeedPython language.
  //
  // The indent and dedent tokens cannot be expressed as ANTLR4 grammar. The newline and spaces need
  // be scanned to generate indent and dedent tokens.
  internal class SeedPythonDentLexer : SeedPythonLexer {
    // A linked list to store multiple tokens, because the original ANTLR4 lexer can only generate
    // one token at the same time.
    private readonly LinkedList<IToken> _tokens = new LinkedList<IToken>();
    // The stack that keeps track of the indentation level.
    private readonly Stack<int> _indents = new Stack<int>();
    // The amount of opened braces, brackets and parenthesis.
    private int _opened = 0;
    // A flag to indicate that current token is the first token to be generated.
    private bool _firstToken = true;

    public SeedPythonDentLexer(ICharStream input) : base(input) {
    }

    public override IToken NextToken() {
      if (TryPopFirstToken(out IToken first)) {
        return first;
      }
      IToken currentToken = base.NextToken();
      if (_firstToken) {
        _firstToken = false;
        HandleFirstToken(currentToken);
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
        default:
          EmitToken(currentToken);
          break;
      }
      return PopFirstToken();
    }

    private static IToken CreateDedent(int line) {
      return new CommonToken(SeedPythonParser.DEDENT, "") { Line = line, Column = 0 };
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

    private void HandleFirstToken(IToken firstToken) {
      if (firstToken.StartIndex > 0) {
        var interval = new Interval(0, firstToken.StartIndex - 1);
        string spaces = (InputStream as ICharStream).GetText(interval);
        int indent = GetIndentationCount(spaces);
        _indents.Push(indent);
        EmitToken(new CommonToken(SeedPythonParser.NEWLINE, "\n") { Line = 1, Column = 0 });
        EmitToken(CommonToken(SeedPythonParser.INDENT, 0, firstToken.StartIndex - 1, 0, 0));
      }
    }

    private void HandleNewline(IToken newlineToken) {
      int spacesStart = 0;
      while (spacesStart < Text.Length && IsNewline(Text[spacesStart])) {
        spacesStart++;
      }
      // Strips newlines inside open clauses except if we are near EOF. Keeps NEWLINEs near EOF to
      // satisfy the final newline needed by the interactive rule used by the REPL.
      int next = InputStream.LA(1);
      int nextNext = InputStream.LA(2);
      if (_opened > 0 || (nextNext != -1 && (IsNewline((char)next) || next == '#'))) {
        // Ignores all indents, dedents and line breaks when inside a list or on a blank line.
        Skip();
      } else {
        int indent = GetIndentationCount(Text.Substring(spacesStart));
        int previous = _indents.Count == 0 ? 0 : _indents.Peek();
        if (indent == previous) {
          EmitToken(newlineToken);
        } else if (indent > previous) {
          _indents.Push(indent);
          EmitToken(CommonToken(SeedPythonParser.NEWLINE,
                                newlineToken.StartIndex, newlineToken.StartIndex + spacesStart - 1,
                                newlineToken.Line, newlineToken.Column));
          EmitToken(CommonToken(SeedPythonParser.INDENT,
                                newlineToken.StartIndex + spacesStart, newlineToken.StopIndex,
                                newlineToken.Line + 1, 0));
        } else {
          EmitToken(CommonToken(SeedPythonParser.NEWLINE,
                                newlineToken.StartIndex, newlineToken.StartIndex + spacesStart - 1,
                                newlineToken.Line, newlineToken.Column));
          while (_indents.Count > 0 && _indents.Peek() > indent) {
            EmitToken(CreateDedent(newlineToken.Line + 1));
            _indents.Pop();
          }
        }
      }
    }

    private void HandleEof(IToken eofToken) {
      // Emits an extra line break that serves as the end of the statement.
      EmitToken(new CommonToken(SeedPythonParser.NEWLINE, "\n") {
        Line = eofToken.Line,
        Column = eofToken.Column,
      });
      // Emits as many dedent tokens as needed.
      while (_indents.Count != 0) {
        EmitToken(CreateDedent(eofToken.Line + 1));
        _indents.Pop();
      }
      EmitToken(eofToken);
    }

    private bool TryPopFirstToken(out IToken first) {
      if (_tokens.Count == 0) {
        first = null;
        return false;
      }
      first = PopFirstToken();
      return true;
    }

    private IToken PopFirstToken() {
      IToken first = _tokens.First.Value;
      _tokens.RemoveFirst();
      return first;
    }

    private static bool IsNewline(char ch) {
      return ch == '\r' || ch == '\n' || ch == '\f';
    }

    private void EmitToken(IToken token) {
      _tokens.AddLast(token);
    }

    private CommonToken CommonToken(int type, int start, int stop, int line, int column) {
      var source = Tuple.Create((ITokenSource)this, (ICharStream)InputStream);
      return new CommonToken(source, type, DefaultTokenChannel, start, stop) {
        Line = line,
        Column = column,
      };
    }
  }
}
