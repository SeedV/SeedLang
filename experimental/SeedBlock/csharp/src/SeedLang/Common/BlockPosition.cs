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
  // An immutable class to represent a position in a SeedBlock source code.
  //
  // Typically a position in a block-style language is the ID of a specified block.
  public struct BlockPosition : IEquatable<BlockPosition> {
    public string BlockId { get; }

    public BlockPosition(string blockId) {
      BlockId = blockId;
    }

    public override string ToString() {
      return BlockId is null ? "" : $"Block: {BlockId}";
    }

    public override int GetHashCode() {
      return BlockId.GetHashCode();
    }

    public bool Equals(BlockPosition pos) {
      return BlockId == pos.BlockId;
    }

    public override bool Equals(object obj) {
      return (obj is BlockPosition objBlockPosition) && Equals(objBlockPosition);
    }

    public static bool operator ==(BlockPosition pos1, BlockPosition pos2) {
      return pos1.Equals(pos2);
    }

    public static bool operator !=(BlockPosition pos1, BlockPosition pos2) {
      return !(pos1 == pos2);
    }
  }
}
