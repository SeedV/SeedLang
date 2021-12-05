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
using System.Diagnostics;
using System.Linq;
using System.Text;
using SeedLang.Common;

namespace SeedLang.Shell {
  // A class to handle source code input and output.
  internal class SourceCode {
    public string Source { get; private set; }

    // The starting index of each line in source code.
    private readonly List<int> _lineStarts = new List<int>();

    // Reads the source code from console. Continues to read if there is a ':' character in the end
    // of this line.
    internal void Read() {
      _lineStarts.Clear();
      _lineStarts.Add(0);
      var sb = new StringBuilder();
      string line = null;
      while (string.IsNullOrEmpty(line)) {
        line = ReadLine.Read(">>> ").TrimEnd();
      }
      sb.Append(line);
      if (line.Last() == ':') {
        while (true) {
          line = ReadLine.Read("... ").TrimEnd();
          if (string.IsNullOrEmpty(line)) {
            break;
          }
          sb.Append(Environment.NewLine);
          _lineStarts.Add(sb.Length);
          sb.Append(line);
        }
      }
      _lineStarts.Add(sb.Length);
      Source = sb.ToString();
    }

    internal void WriteSourceWithHighlight(TextRange range) {
      var first = new TextPosition(1, 0);
      if (range.Start.CompareTo(first) > 0) {
        var r = new TextRange(first, new TextPosition(range.Start.Line, range.Start.Column - 1));
        Console.Write(SourceOfRange(r));
      }
      Console.BackgroundColor = ConsoleColor.DarkCyan;
      Console.ForegroundColor = ConsoleColor.Black;
      Console.Write(SourceOfRange(range));
      Console.ResetColor();
      int startIndex = _lineStarts[range.End.Line - 1] + range.End.Column + 1;
      if (startIndex < Source.Length) {
        Console.Write(Source.Substring(startIndex));
      }
    }

    internal void WriteSourceWithSyntaxTokens(IReadOnlyList<SyntaxToken> syntaxTokens) {
      Console.ResetColor();
      Console.WriteLine("---------- Source ----------");
      var currentPos = new TextPosition(1, 0);
      foreach (SyntaxToken token in syntaxTokens) {
        if (token.Range.Start.CompareTo(currentPos) > 0) {
          var endPos = new TextPosition(token.Range.Start.Line, token.Range.Start.Column - 1);
          Console.Write(SourceOfRange(new TextRange(currentPos, endPos)));
        }
        if (Theme.SyntaxToThemeInfoMap.ContainsKey(token.Type)) {
          Console.ForegroundColor = Theme.SyntaxToThemeInfoMap[token.Type].ForegroundColor;
        }
        Console.Write(SourceOfRange(token.Range));
        Console.ResetColor();
        currentPos = new TextPosition(token.Range.End.Line, token.Range.End.Column + 1);
      }
      Console.Write(Source.Substring(_lineStarts[currentPos.Line - 1] + currentPos.Column));
      Console.WriteLine();
      Console.WriteLine();
    }

    private string SourceOfRange(TextRange range) {
      Debug.Assert(range.Start.Line <= range.End.Line);
      int startIndex = _lineStarts[range.Start.Line - 1] + range.Start.Column;
      int endIndex = _lineStarts[range.End.Line - 1] + range.End.Column;
      if (startIndex >= Source.Length) {
        return "";
      }
      int length = Math.Min(endIndex, Source.Length - 1) - startIndex + 1;
      return Source.Substring(startIndex, length);
    }
  }
}
