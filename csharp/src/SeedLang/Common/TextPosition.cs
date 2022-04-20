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
  // An immutable class to represent a position in a plaintext source code.
  public sealed class TextPosition : IComparable<TextPosition>, IEquatable<TextPosition> {
    public int Line { get; }
    public int Column { get; }

    public TextPosition(int line, int column) {
      Line = line;
      Column = column;
    }

    public override string ToString() {
      return $"Ln {Line}, Col {Column}";
    }

    public override int GetHashCode() {
      return new { Line, Column }.GetHashCode();
    }

    public int CompareTo(TextPosition pos) {
      if (Line < pos.Line) {
        return -1;
      } else if (Line > pos.Line) {
        return 1;
      } else {
        if (Column < pos.Column) {
          return -1;
        } else if (Column > pos.Column) {
          return 1;
        } else {
          return 0;
        }
      }
    }

    public bool Equals(TextPosition pos) {
      if (pos is null) {
        return false;
      }
      if (ReferenceEquals(this, pos)) {
        return true;
      }
      return CompareTo(pos) == 0;
    }

    public override bool Equals(object obj) {
      return Equals(obj as TextPosition);
    }

    public static bool operator ==(TextPosition pos1, TextPosition pos2) {
      return pos1.Equals(pos2);
    }

    public static bool operator !=(TextPosition pos1, TextPosition pos2) {
      return !(pos1 == pos2);
    }

    public static bool operator <(TextPosition pos1, TextPosition pos2) {
      return pos1.CompareTo(pos2) < 0;
    }

    public static bool operator <=(TextPosition pos1, TextPosition pos2) {
      return pos1.CompareTo(pos2) <= 0;
    }

    public static bool operator >(TextPosition pos1, TextPosition pos2) {
      return pos1.CompareTo(pos2) > 0;
    }

    public static bool operator >=(TextPosition pos1, TextPosition pos2) {
      return pos1.CompareTo(pos2) >= 0;
    }
  }
}
