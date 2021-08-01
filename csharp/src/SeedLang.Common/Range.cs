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

namespace SeedLang.Common {
  // Represents a range in the source code. Here, the source code can be either a plain text source
  // code, or a SeedBlock module.
  public class Range {
    // The line index of the starting character.
    // TODO: consider how to express SeedBlock ranges.
    public int Line { get; }

    // The column index of the staring character.
    public int CharPosition { get; }

    // The length of this range.
    public int Length { get; }

    public Range(int line, int charPosition, int length) {
      Line = line;
      CharPosition = charPosition;
      Length = length;
    }

    public override string ToString() {
      return $"Line: {Line}, Column: {CharPosition}, Length: {Length}";
    }
  }
}
