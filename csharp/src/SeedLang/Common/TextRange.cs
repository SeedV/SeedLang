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

using Antlr4.Runtime;
using System;

namespace SeedLang.Common {
  // Represents a range in a plaintext source code.
  public class TextRange : Range {
    public TextPosition Start { get; }
    public TextPosition End { get; }

    // A text range is defined as [start, end], where both ends of the range are inclusive.
    public TextRange(TextPosition start, TextPosition end) {
      Start = start;
      End = end;
    }

    public TextRange(int startLine, int startColumn, int endLine, int endColumn) :
        this(new TextPosition(startLine, startColumn), new TextPosition(endLine, endColumn)) {
    }

    public override string ToString() {
      return $"[{Start} - {End}]";
    }

    public override int GetHashCode() {
      return Tuple.Create(Start.GetHashCode(), End.GetHashCode()).GetHashCode();
    }

    public override bool Equals(Range range) {
      if (range is null) {
        return false;
      }
      if (ReferenceEquals(this, range)) {
        return true;
      }
      if (GetType() != range.GetType()) {
        return false;
      }
      return Start == (range as TextRange).Start && End == (range as TextRange).End;
    }

    public static TextRange RangeOfToken(IToken token) {
      return RangeOfTokens(token, token);
    }

    public static TextRange RangeOfTokens(IToken start, IToken end) {
      // TODO: need scan the source string to calculate the end column if the end token is in
      // multiple lines.
      return new TextRange(start.Line, start.Column,
                           end.Line, end.Column + end.StopIndex - end.StartIndex);
    }
  }
}
