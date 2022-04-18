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

namespace SeedLang.Common {
  // Represents a range in a SeedBlock source code.
  public class BlockRange : Range {
    public BlockPosition Position;

    // A block range is defined as a specified block. The block can be either a standalone primitive
    // block, or a compound block that contains other blocks as its inputs or child statements.
    public BlockRange(BlockPosition position) {
      Position = position;
    }

    public BlockRange(string blockId) : this(new BlockPosition(blockId)) {
    }

    public override string ToString() {
      return $"[{Position}]";
    }

    public override int GetHashCode() {
      return Position.GetHashCode();
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
      return Position == (range as BlockRange).Position;
    }
  }
}
