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
using SeedLang.Common;

namespace SeedLang.Block {
  // The base class of all the in-memory block classes.
  public abstract class BaseBlock : IEquatable<BaseBlock> {
    public string Id { get; set; } = "";
    public string Doc { get; set; } = "";
    public Position Pos { get; set; } = new Position(0, 0);

    // Once a block is parsed from an inline text (got from the user input in the inline editing
    // mode), it records a reference to the position of the inline text. This info is only used by
    // the editor for display purposes. It won't be serialized to persistent storages.
    public TextRange InlineTextReference { get; set; } = null;

    public override int GetHashCode() {
      return Id.GetHashCode();
    }

    // In the SeedBlock layer, two blocks are equal if and only if they share the same type and the
    // same ID. Other states of the blocks do not affect their equality.
    public bool Equals(BaseBlock block) {
      if (block is null) {
        return false;
      }
      if (ReferenceEquals(this, block)) {
        return true;
      }
      if (GetType() != block.GetType()) {
        return false;
      }
      return Id == block.Id;
    }

    public override bool Equals(object obj) {
      return Equals(obj as BaseBlock);
    }

    public static bool operator ==(BaseBlock block1, BaseBlock block2) {
      if (block1 is null) {
        return block2 is null;
      }
      return block1.Equals(block2);
    }

    public static bool operator !=(BaseBlock block1, BaseBlock block2) {
      return !(block1 == block2);
    }

    // Accepts the block visitor and traverses in the block hierarchy.
    public abstract void Accept(IBlockVisitor visitor);
  }
}
