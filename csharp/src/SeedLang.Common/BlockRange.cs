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
  // Represents a range in a SeedBlock source code.
  public class BlockRange : Range {
    public static readonly BlockRange Empty = new BlockRange(null as BlockPosition);

    public BlockPosition Position;

    // A code range is defined as a specified block. The block can be either a standalone primitive
    // block, or a compound block that contains other blocks as its inputs or child statements.
    public BlockRange(BlockPosition position) {
      Position = position;
    }

    public BlockRange(string blockId) : this(new BlockPosition(blockId)) {
    }

    // Returns if the range is empty. A range can be empty when a diagnostic cannot be associated to
    // a particular code position.
    public override bool IsEmpty() {
      return Position == null;
    }

    public override int GetHashCode() {
      return Position.GetHashCode();
    }

    public override string ToString() {
      return IsEmpty() ? "[]" : $"[{Position}]";
    }

    public override bool Equals(Range range) {
      return (range is BlockRange blockRange) && Position == blockRange.Position;
    }
  }
}
