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
  // An immutable class to represent a position in a plaintext source code.
  public class TextCodePosition : CodePostion {
    public int Line { get; }
    public int Column { get; }

    public TextCodePosition(int line, int column) {
      Line = line;
      Column = column;
    }

    public override int GetHashCode() {
      return Line << 8 | Column;
    }

    public override string ToString() {
      return $"Ln {Line}, Col {Column}";
    }

    public override int CompareTo(CodePostion pos) {
      if (!(pos is TextCodePosition)) {
        throw new NotSupportedException();
      }
      var textCodePosition = pos as TextCodePosition;
      if (Line < textCodePosition.Line) {
        return -1;
      } else if (Line > textCodePosition.Line) {
        return 1;
      } else {
        if (Column < textCodePosition.Column) {
          return -1;
        } else if (Column > textCodePosition.Column) {
          return 1;
        } else {
          return 0;
        }
      }
    }
  }
}
