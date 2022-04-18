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
using SeedLang.Common;

namespace SeedLang.Block {
  // The position where a block docks to another block, or stays as a standalone block on a
  // SeedBlock module canvas.
  //
  // This class is different with SeedLang.Common.BlockPosition, which represents a syntaxical code
  // position in a SeedBlock module.
  public class Position : IEquatable<Position> {
    // How the block is docked to another block.
    public enum DockType {
      // The block is standalone, not docked to any other block. The block's position is defined by
      // CanvasPosition.
      UnDocked = 0,
      // The block docks as one of the target block's input blocks. The index of the input dock slot
      // is specified by DockSlotIndex.
      Input,
      // The block docks as the next statement of the target block. In this case, DockSlotIndex is
      // omitted.
      NextStatement,
      // The block docks as the first child statement at one of the target block's statement dock
      // slots. The index of the statement dock slot is specified by DockSlotIndex.
      ChildStatement,
    }

    public DockType Type { get; set; }
    public bool IsDocked => Type != DockType.UnDocked;

    // If a block is un-docked, its position is defined by a 2D coordinate (x, y).
    public Vector2 CanvasPosition = new Vector2(0, 0);

    // The target block ID if a block is docked.
    public string TargetBlockId { get; set; }

    // The target block may have multiple dock slots. E.g., a set...to... block has two input dock
    // slots, one for the variable, and the other for the expression. And, a if...else... block has
    // two statement dock slots, one for the compound statements in if..., the other for the
    // compound statements in else...
    public int DockSlotIndex { get; set; }

    // Initializes an un-docked position. It's canvas position is (x, y).
    public Position(int x, int y) {
      Type = DockType.UnDocked;
      CanvasPosition.X = x;
      CanvasPosition.Y = y;
    }

    // Initializes an un-docked position. It's canvas position is point.
    public Position(Vector2 point) {
      Type = DockType.UnDocked;
      CanvasPosition = point;
    }

    // Initializes a docked position.
    public Position(DockType type, string targetBlockId, int dockSlotIndex) {
      Type = type;
      TargetBlockId = targetBlockId;
      DockSlotIndex = dockSlotIndex;
    }

    public override string ToString() {
      if (IsDocked) {
        return $"DockPosition: ({Enum.GetName(typeof(DockType), Type)}, " +
               $"{TargetBlockId}, {DockSlotIndex})";
      } else {
        return $"CanvasPosition: ({CanvasPosition.X}, {CanvasPosition.Y})";
      }
    }

    public override int GetHashCode() {
      if (IsDocked) {
        return Tuple.Create(Type, TargetBlockId, DockSlotIndex).GetHashCode();
      } else {
        return Tuple.Create(CanvasPosition.X, CanvasPosition.Y).GetHashCode();
      }
    }

    public bool Equals(Position pos) {
      if (pos is null) {
        return false;
      }
      if (ReferenceEquals(this, pos)) {
        return true;
      }
      if (GetType() != pos.GetType()) {
        return false;
      }
      if (IsDocked) {
        return Type == pos.Type &&
               TargetBlockId == pos.TargetBlockId &&
               DockSlotIndex == pos.DockSlotIndex;
      } else {
        return CanvasPosition.X == pos.CanvasPosition.X && CanvasPosition.Y == pos.CanvasPosition.Y;
      }
    }

    public override bool Equals(object obj) {
      return Equals(obj as Position);
    }

    public static bool operator ==(Position pos1, Position pos2) {
      if (pos1 is null) {
        return pos2 is null;
      }
      return pos1.Equals(pos2);
    }

    public static bool operator !=(Position pos1, Position pos2) {
      return !(pos1 == pos2);
    }
  }
}
