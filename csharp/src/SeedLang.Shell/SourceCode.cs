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
using System.Text.RegularExpressions;
using SeedLang.Common;

namespace SeedLang.Shell {
  // A class to handle source code input and output.
  internal class SourceCode {
    private readonly List<string> _lines = new List<string>();

    public string Source => string.Join(null, _lines);

    internal void AddLine(string line) {
      _lines.Add(line + Environment.NewLine);
    }

    internal void Reset() {
      _lines.Clear();
    }

    internal void WriteSourceWithHighlight(TextRange range) {
      int lineId = range.Start.Line;
      while (lineId <= range.End.Line && lineId > 0 && lineId <= _lines.Count) {
        Console.Write($"{lineId,-5} ");
        string line = _lines[lineId - 1];
        if (Intersect(lineId, range) is (int start, int end)) {
          Console.Write(line[..start]);
          if (line[start..] != Environment.NewLine) {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.Black;
          }
          Console.Write(line[start..(end + 1)]);
          Console.ResetColor();
          Console.Write(line[(end + 1)..]);
        } else {
          Console.Write(line);
        }
        lineId++;
      }
    }

    internal void WriteSourceWithTokens(IReadOnlyList<TokenInfo> tokens) {
      Console.ResetColor();
      Console.WriteLine("---------- Source ----------");
      int tokenIndex = 0;
      for (int lineId = 1; lineId <= _lines.Count; lineId++) {
        WriteLineWithTokens(lineId, tokens, ref tokenIndex);
      }
    }

    private void WriteLineWithTokens(int lineId, IReadOnlyList<TokenInfo> tokens,
                                     ref int tokenIndex) {
      int column = 0;
      Console.Write($"{lineId,-5} ");
      string line = _lines[lineId - 1];
      while (column < line.Length && tokenIndex < tokens.Count &&
             tokens[tokenIndex].Range.Start.Line <= lineId) {
        TokenInfo token = tokens[tokenIndex];
        if (token.Range.Start.Column > column) {
          Console.Write(line[column..token.Range.Start.Column]);
        }
        if (Theme.SyntaxToThemeInfoMap.ContainsKey(token.Type)) {
          Console.ForegroundColor = Theme.SyntaxToThemeInfoMap[token.Type].ForegroundColor;
        }
        Console.Write(Substring(line, token.Range.Start.Column, token.Range.End.Column));
        Console.ResetColor();
        column = token.Range.End.Line <= lineId ? token.Range.End.Column + 1 : line.Length;
        if (token.Range.End.Line <= lineId) {
          tokenIndex++;
        }
      }
      Console.Write(line[column..]);
    }

    internal bool IsCompleteStatement() {
      Debug.Assert(_lines.Count > 0);
      return IsCompleteStatement(0, _lines.Count - 1);
    }

    private static bool IsSingleLineVTag(string line) {
      return new Regex(@"^#[ \t]*\[\[.*\]\][ \t]*\n").IsMatch(line);
    }

    private static bool IsMultipleLineVTagStart(string line) {
      return new Regex(@"^#[ \t]*\[\[[^\]]*\n").IsMatch(line);
    }

    private static bool IsMultipleLineVTagEnd(string line) {
      return new Regex(@"^#[ \t]*\]\][ \t]*\n").IsMatch(line);
    }

    private static bool IsFirstLineOfCompoundStatement(string line) {
      return new Regex(@".*:[ \t]*\n").IsMatch(line);
    }

    private static bool EndsWithBackSlash(string line) {
      return new Regex(@".*\\\n").IsMatch(line);
    }

    private static string Substring(string line, int columnStart, int columnEnd) {
      return line.Substring(Math.Max(columnStart, 0),
                            Math.Min(line.Length, columnEnd + 1) - columnStart);
    }

    private bool IsCompleteStatement(int startLine, int endLine) {
      if (startLine > endLine) {
        return false;
      }
      if (IsSingleLineVTag(_lines[startLine])) {
        return IsCompleteStatement(startLine + 1, endLine);
      } else if (IsMultipleLineVTagStart(_lines[startLine])) {
        return endLine > startLine && IsMultipleLineVTagEnd(_lines[endLine]);
      } else if (IsFirstLineOfCompoundStatement(_lines[startLine])) {
        return endLine > startLine && _lines[endLine] == "\n";
      }
      return HaveMatchedBraces(startLine, endLine) && !EndsWithBackSlash(_lines[endLine]);
    }

    private bool HaveMatchedBraces(int startLine, int endLine) {
      const char leftBrace = '{';
      const char rightBrace = '}';
      const char leftBrack = '[';
      const char rightBrack = ']';
      const char leftParen = '(';
      const char rightParen = ')';
      var stack = new Stack<char>();
      for (int i = startLine; i <= endLine; i++) {
        foreach (char ch in _lines[i]) {
          switch (ch) {
            case leftBrace:
              stack.Push(leftBrace);
              break;
            case rightBrace:
              if (stack.Pop() != leftBrace) {
                return false;
              }
              break;
            case leftBrack:
              stack.Push(leftBrack);
              break;
            case rightBrack:
              if (stack.Pop() != leftBrack) {
                return false;
              }
              break;
            case leftParen:
              stack.Push(leftParen);
              break;
            case rightParen:
              if (stack.Pop() != leftParen) {
                return false;
              }
              break;
          }
        }
      }
      return stack.Count == 0;
    }

    private (int, int)? Intersect(int lineId, TextRange range) {
      if (range.Start.Line > lineId || range.End.Line < lineId) {
        return null;
      }
      bool isStartLine = lineId == range.Start.Line;
      bool isEndLine = lineId == range.End.Line;
      int lineEnd = _lines[lineId - 1].Length - 1;
      int start = isStartLine ? Math.Min(Math.Max(0, range.Start.Column), lineEnd) : 0;
      int end = isEndLine ? Math.Min(Math.Max(0, range.End.Column), lineEnd) : lineEnd;
      return (start, end);
    }
  }
}
