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
  // Represents a range in the source code. Here, the source code can be either a plain text source
  // code, or a SeedBlock module.
  public class CodeRange : IEquatable<CodeRange> {
    public static readonly CodeRange Empty = new CodeRange(null, null);

    public CodePostion Start { get; }
    public CodePostion End { get; }

    // For plaintext code, a code range is defined as [start, end], where both ends of the range are
    // inclusive.
    public CodeRange(TextCodePosition start, TextCodePosition end) {
      Start = start;
      End = end;
    }

    // For SeedBlock code, a code range is defined as a block. The block can be either a standalone
    // primitive block, or a compound block that contains other blocks as inputs or child
    // statements.
    public CodeRange(BlockCodePosition blockCodePosition) {
      Start = blockCodePosition;
      End = null;
    }

    // Returns if the range is empty.
    public bool IsEmpty() {
      return Start == null;
    }

    // Returns if the range is in a plaintext source code.
    public bool IsTextCodeRange() {
      return Start != null && Start is TextCodePosition;
    }

    public override int GetHashCode() {
      int n1 = Start == null ? -1 : Start.GetHashCode();
      int n2 = End == null ? -1 : End.GetHashCode();
      return Tuple.Create(n1, n2).GetHashCode();
    }

    public override string ToString() {
      if (IsEmpty()) {
        return "[]";
      } else if (IsTextCodeRange()) {
        return $"[{Start} - {End}]";
      } else {
        return $"[{Start}]";
      }
    }

    public bool Equals(CodeRange range) {
      return !(range is null) && Start == range.Start && End == range.End;
    }

    public override bool Equals(object obj) {
      return obj is CodeRange objCodeRange && Equals(objCodeRange);
    }

    public static bool operator ==(CodeRange range1, CodeRange range2) {
      if (range1 is null) {
        return range2 is null;
      }
      return range1.Equals(range2);
    }

    public static bool operator !=(CodeRange range1, CodeRange range2) {
      return !(range1 == range2);
    }
  }
}
