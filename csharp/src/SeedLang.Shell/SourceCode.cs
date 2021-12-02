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
using SeedLang.Common;

namespace SeedLang.Shell {
  // A class to handle source code input and output.
  internal class SourceCode {
    public string Source = "";

    internal void WriteSourceWithHighlight(TextRange inputRange) {
      TextRange range = RangeInSource(inputRange);
      Console.Write(Source.Substring(0, range.Start.Column));
      Console.BackgroundColor = ConsoleColor.DarkCyan;
      Console.ForegroundColor = ConsoleColor.Black;
      int length = range.End.Column - range.Start.Column + 1;
      Console.Write(Source.Substring(range.Start.Column, length));
      Console.ResetColor();
      Console.Write(Source.Substring(range.End.Column + 1));
      Console.Write(": ");
    }

    internal void WriteSourceWithSyntaxTokens(IReadOnlyList<SyntaxToken> syntaxTokens) {
      Console.ResetColor();
      Console.WriteLine("---------- Source ----------");
      int startColumn = 0;
      foreach (SyntaxToken token in syntaxTokens) {
        TextRange range = RangeInSource(token.Range);
        Console.Write(Source.Substring(startColumn, range.Start.Column - startColumn));
        if (Theme.SyntaxToThemeInfoMap.ContainsKey(token.Type)) {
          Console.ForegroundColor = Theme.SyntaxToThemeInfoMap[token.Type].ForegroundColor;
        }
        int length = range.End.Column - range.Start.Column + 1;
        Console.Write(Source.Substring(range.Start.Column, length));
        startColumn = range.End.Column + 1;
        Console.ResetColor();
      }
      Console.Write(Source.Substring(startColumn, Source.Length - startColumn));
      Console.WriteLine();
      Console.WriteLine();
    }

    private TextRange RangeInSource(TextRange range) {
      return new TextRange(PositionInSource(range.Start), PositionInSource(range.End));
    }

    private TextPosition PositionInSource(TextPosition position) {
      int column = Math.Min(Math.Max(0, position.Column), Source.Length - 1);
      return new TextPosition(position.Line, column);
    }
  }
}
