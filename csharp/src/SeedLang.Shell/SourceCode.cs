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
using SeedLang.Common;

namespace SeedLang.Shell {
  // A class to handle source code input and output.
  internal class SourceCode {
    public string Source => string.Join(null, _lines);

    private readonly List<string> _lines = new List<string>();

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
          Console.Write(line.Substring(0, start));
          if (line.Substring(start) != Environment.NewLine) {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.Black;
          }
          Console.Write(line.Substring(start, end - start + 1));
          Console.ResetColor();
          Console.Write(line.Substring(end + 1));
        } else {
          Console.Write(line);
        }
        lineId++;
      }
    }

    internal void WriteSourceWithSyntaxTokens(IReadOnlyList<SyntaxToken> syntaxTokens) {
      Console.ResetColor();
      Console.WriteLine("---------- Source ----------");
      int tokenIndex = 0;
      for (int lineId = 1; lineId <= _lines.Count; lineId++) {
        WriteLineWithSyntaxTokens(lineId, syntaxTokens, ref tokenIndex);
      }
    }

    private void WriteLineWithSyntaxTokens(int lineId, IReadOnlyList<SyntaxToken> syntaxTokens,
                                           ref int tokenIndex) {
      int column = 0;
      Console.Write($"{lineId,-5} ");
      string line = _lines[lineId - 1];
      while (column < line.Length && tokenIndex < syntaxTokens.Count &&
             syntaxTokens[tokenIndex].Range.Start.Line <= lineId) {
        SyntaxToken token = syntaxTokens[tokenIndex];
        if (token.Range.Start.Column > column) {
          Console.Write(line.Substring(column, token.Range.Start.Column - column));
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
      Console.Write(line.Substring(column));
    }

    private static string Substring(string line, int columnStart, int columnEnd) {
      return line.Substring(Math.Max(columnStart, 0),
                            Math.Min(line.Length, columnEnd + 1) - columnStart);
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
