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
  // The common interface of all the blocks that can be docked with other blocks.
  public interface IDockable {
    // Tests if the input block can be docked to the instance, as the specified dock type and dock
    // position.
    bool CanDock(BaseBlock block, Position.DockType type, int dockPosition);

    // Docks the input block to the instance.
    //
    // It's this method's duty to update the position states of: (1) the target block itself, (2)
    // the input block, and (3) other relevant blocks.
    void Dock(BaseBlock block, Position.DockType type, int dockPosition);

    // Un-docks the input block from the instance.
    //
    // It's this method's duty to update the position states of: (1) the target block itself, (2)
    // the input block, and (3) other relevant blocks.
    void UnDock(BaseBlock block, Vector2 newCanvasPosition);
  }
}
