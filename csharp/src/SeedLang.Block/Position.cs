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

using SeedLang.Common;

namespace SeedLang.Block {
  // The position where a block docks to another block, or stays as a standalone block on a
  // SeedBlock module canvas.
  //
  // This class is different with SeedLang.Common.BlockPosition, which represents a syntaxical code
  // position in a SeedBlock module.
  public class Position {
    // How the block is docked to another block.
    public enum DockType {
      // The block is standalone, not docked to any other block. The block's position is defined by
      // CanvasPosition.
      UnDocked = 0,
      // The block docks as one of the target block's input blocks. The index of the input position
      // is specified by DockPosition.
      Input,
      // The block docks as the next statement of the target block. In this case, DockPosition is
      // omitted.
      NextStatement,
      // The block docks as the first child statement at one of the target block's dock slots. The
      // index of the dock slot is specified by DockPosition.
      ChildStatement,
    }

    public DockType Type { get; set; }
    public Vector2 CanvasPosition = new Vector2(0, 0);
    public string TargetBlockId { get; set; }
    public int DockPosition { get; set; }
    public bool IsDocked => Type != DockType.UnDocked;

    // Initializes an un-docked position. It's canvas position is (x, y).
    public Position(int x, int y) {
      Type = DockType.UnDocked;
      CanvasPosition.X = x;
      CanvasPosition.Y = y;
    }

    // Initializes a docked position.
    public Position(DockType type, string targetBlockId, int dockPosition) {
      Type = type;
      TargetBlockId = targetBlockId;
      DockPosition = dockPosition;
    }
  }
}
