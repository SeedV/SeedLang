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
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace SeedLang.X {
  internal class SeedPythonDentLexer : SeedPythonLexer {
    // A linked list where extra tokens are added in.
    private readonly LinkedList<IToken> _tokens = new LinkedList<IToken>();
    // The stack that keeps track of the indentation level.
    private readonly Stack<int> _indents = new Stack<int>();
    // The amount of opened braces, brackets and parenthesis.
    private int _opened = 0;
    // The most recently produced token.
    private IToken _lastToken = null;

    public SeedPythonDentLexer(ICharStream input) : base(input) {
    }

    public override void Emit(IToken token) {
      Token = token;
      _tokens.AddLast(token);
    }

    public override IToken NextToken() {
      // Check if the end-of-file is ahead and there are still some DEDENTs expected.
      if (InputStream.LA(1) == Eof && _indents.Count != 0) {
        // Remove any trailing EOF tokens from our buffer.
        LinkedListNode<IToken> node = _tokens.First;
        while (node != null) {
          LinkedListNode<IToken> next = node.Next;
          if (node.Value.Type == Eof) {
            _tokens.Remove(node);
          }
          node = next;
        }
        // First emit an extra line break that serves as the end of the statement.
        Emit(CommonToken(SeedPythonParser.NEWLINE, "\n"));
        // Now emit as many DEDENT tokens as needed.
        while (_indents.Count != 0) {
          Emit(CreateDedent());
          _indents.Pop();
        }
        // Put the EOF back on the token stream.
        Emit(CommonToken(SeedPythonParser.Eof, "<EOF>"));
      }

      IToken nextToken = base.NextToken();
      switch (nextToken.Type) {
        case SeedPythonParser.NEWLINE:
          OnNewLine();
          break;
        case SeedPythonParser.OPEN_PAREN:
        case SeedPythonParser.OPEN_BRACE:
        case SeedPythonParser.OPEN_BRACK:
          _opened++;
          break;
        case SeedPythonParser.CLOSE_PAREN:
        case SeedPythonParser.CLOSE_BRACE:
        case SeedPythonParser.CLOSE_BRACK:
          _opened--;
          break;
      }
      if (nextToken.Channel == DefaultTokenChannel) {
        // Keep track of the last token on the default channel.
        _lastToken = nextToken;
      }

      if (_tokens.Count == 0) {
        return nextToken;
      } else {
        IToken first = _tokens.First.Value;
        _tokens.RemoveFirst();
        return first;
      }
    }

    private CommonToken CommonToken(int type, string text) {
      int stop = CharIndex - 1;
      int start = text.Length == 0 ? stop : CharIndex - text.Length;
      var source = Tuple.Create((ITokenSource)this, (ICharStream)InputStream);
      return new CommonToken(source, type, DefaultTokenChannel, start, stop);
    }

    private IToken CreateDedent() {
      var dedent = CommonToken(SeedPythonParser.DEDENT, "");
      dedent.Line = _lastToken.Line;
      return dedent;
    }

    // Calculates the indentation of the provided spaces, taking the
    // following rules into account:
    //
    // "Tabs are replaced (from left to right) by one to eight spaces
    //  such that the total number of characters up to and including
    //  the replacement is a multiple of eight [...]"
    //
    //  -- https://docs.python.org/3.1/reference/lexical_analysis.html#indentation
    private static int GetIndentationCount(string spaces) {
      int count = 0;
      foreach (char ch in spaces.ToCharArray()) {
        count += ch == '\t' ? 8 - (count % 8) : 1;
      }
      return count;
    }

    private bool AtStartOfInput() {
      return Column == 0 && Line == 1;
    }

    private void OnNewLine() {
      var newLine = new Regex("[^\r\n\f]+").Replace(Text, "");
      var spaces = new Regex("[\r\n\f]+").Replace(Text, "");
      // Strip newlines inside open clauses except if we are near EOF. We keep NEWLINEs near EOF to
      // satisfy the final newline needed by the single_put rule used by the REPL.
      int next = InputStream.LA(1);
      int nextNext = InputStream.LA(2);
      if (_opened > 0 ||
          (nextNext != -1 && (next == '\r' || next == '\n' || next == '\f' || next == '#'))) {
        // If we're inside a list or on a blank line, ignore all indents, dedents and line breaks.
        Skip();
      } else {
        _tokens.RemoveFirst();
        Emit(CommonToken(SeedPythonParser.NEWLINE, newLine));
        int indent = GetIndentationCount(spaces);
        int previous = _indents.Count == 0 ? 0 : _indents.Peek();
        if (indent == previous) {
          // skip indents of the same size as the present indent-size
          Skip();
        } else if (indent > previous) {
          _indents.Push(indent);
          Emit(CommonToken(SeedPythonParser.INDENT, spaces));
        } else {
          // Possibly emit more than 1 DEDENT token.
          while (_indents.Count != 0 && _indents.Peek() > indent) {
            Emit(CreateDedent());
            _indents.Pop();
          }
        }
      }
    }
  }
}
