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

namespace SeedLang.Common {
  // Represents a range in a plaintext source code.
  public sealed class TextRange : IEquatable<TextRange> {
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
      return new { Start, End }.GetHashCode();
    }

    public override bool Equals(object obj) {
      return Equals(obj as TextRange);
    }

    public bool Equals(TextRange range) {
      if (range is null) {
        return false;
      }
      if (ReferenceEquals(this, range)) {
        return true;
      }
      return Start == range.Start && End == range.End;
    }

    public static bool operator ==(TextRange range1, TextRange range2) {
      if (range1 is null) {
        return range2 is null;
      }
      return range1.Equals(range2);
    }

    public static bool operator !=(TextRange range1, TextRange range2) {
      return !(range1 == range2);
    }
  }
}
