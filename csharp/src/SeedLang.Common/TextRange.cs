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

namespace SeedLang.Common {
  // Represents a range in a plaintext source code.
  public class TextRange : Range {
    public static readonly TextRange Empty = new TextRange(null, null);

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

    // Returns if the range is empty. A range can be empty when a diagnostic cannot be associated to
    // a particular code position.
    public override bool IsEmpty() {
      return Start == null;
    }

    public override int GetHashCode() {
      int n1 = Start == null ? -1 : Start.GetHashCode();
      int n2 = End == null ? -1 : End.GetHashCode();
      return Tuple.Create(n1, n2).GetHashCode();
    }

    public override string ToString() {
      return IsEmpty() ? "[]" : $"[{Start} - {End}]";
    }

    public override bool Equals(Range range) {
      return (range is TextRange textRange) && Start == textRange.Start && End == textRange.End;
    }
  }
}
