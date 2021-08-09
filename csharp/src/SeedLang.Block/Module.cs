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

using System.Collections.Generic;
using System.Diagnostics;
using SeedLang.Common;

namespace SeedLang.Block {
  // The in-memory representation of a SeedBlock module.
  //
  // A module defines a namespace in the SeedBlock program. Syntactically it contains a group of
  // blocks and maintains their positions and relationships.
  //
  // In a module, there are two categories of blocks:
  //
  // * Root block: The block that is not docked to any other block. A root block is either a
  //   standalone block, or the root node of a docked block group. The position of a root block is
  //   defined by a canvas coordinates (x, y).
  // * Docked block: The block that is docked to a target block. The position of a docked block is
  //   defined by its relationship with the target block - the dock type, the target block ID and
  //   the dock slot index.
  public class Module {
    // The visitor class to collect the IDs of a docked block group.
    protected class DockedBlockCollector : AbstractBlockVisitor {
      private readonly List<string> _blockIds = new List<string>();

      public IReadOnlyList<string> BlockIds => _blockIds;

      public override void VisitEnter(BaseBlock block) {
        _blockIds.Add(block.Id);
      }

      public override void VisitExit(BaseBlock block) {
      }
    }

    // Each module maintains its own block ID generator.
    private readonly AutoIncrementId _idGenerator = new AutoIncrementId();

    // The collection of all the blocks in the module, indexed by the block IDs.
    private readonly Dictionary<string, BaseBlock> _blocks = new Dictionary<string, BaseBlock>();

    // The ID set of all the root blocks.
    private readonly HashSet<string> _rootBlockIdSet = new HashSet<string>();

    // The module name.
    public string Name { get; set; } = "";

    // The module doc.
    public string Doc { get; set; } = "";

    // The readonly view of all the blocks in the module.
    public IReadOnlyCollection<BaseBlock> Blocks => _blocks.Values;

    // The number of the root blocks.
    public int RootBlockCount => _rootBlockIdSet.Count;

    // The readonly iterator of all the root blocks in the module.
    public IEnumerable<BaseBlock> RootBlockIterator {
      get {
        foreach (string id in _rootBlockIdSet) {
          yield return _blocks[id];
        }
      }
    }

    public Module(string name) {
      Name = name;
    }

    // Clears the module.
    public void Clear() {
      _idGenerator.Reset();
      _blocks.Clear();
      _rootBlockIdSet.Clear();
    }

    // Returns the block instance per its ID.
    public BaseBlock GetBlock(string blockId) {
      Debug.Assert(_blocks.ContainsKey(blockId));
      return _blocks[blockId];
    }

    // Returns if the block is a root block.
    public bool IsRootBlock(string blockId) {
      return _rootBlockIdSet.Contains(blockId);
    }

    // Adds a new standalone block, with a new generated ID. Once added, the new block will be a
    // root block in the module, until it's docked to another block.
    public void AddStandaloneBlock(BaseBlock block) {
      string blockId = _idGenerator.NextString(BxfConstants.IdLength);
      block.Id = blockId;
      block.Pos.Type = Position.DockType.UnDocked;
      _blocks.Add(blockId, block);
      _rootBlockIdSet.Add(blockId);
    }

    // Moves a root block to a new canvas position. The blocks that dock to the root block will be
    // still docked as before.
    public void MoveRootBlock(string blockId, Vector2 newCanvasPosition) {
      Debug.Assert(IsRootBlock(blockId));
      Debug.Assert(_blocks.ContainsKey(blockId));
      BaseBlock block = _blocks[blockId];
      block.Pos.CanvasPosition = newCanvasPosition;
    }

    // Docks a block to a target block.
    public void DockBlock(string blockId, string targetBlockId,
                          Position.DockType type, int dockSlotIndex) {
      Debug.Assert(_blocks.ContainsKey(blockId));
      BaseBlock block = _blocks[blockId];
      Debug.Assert(_blocks.ContainsKey(targetBlockId));
      BaseBlock targetBlock = _blocks[targetBlockId];
      _rootBlockIdSet.Remove(blockId);
      Debug.Assert(targetBlock is IDockable);
      (targetBlock as IDockable).Dock(block, type, dockSlotIndex);
    }

    // Un-docks a block from its target block.
    public void UnDockBlock(string blockId, Vector2 newCanvasPosition) {
      Debug.Assert(_blocks.ContainsKey(blockId));
      BaseBlock block = _blocks[blockId];
      string targetBlockId = block.Pos.TargetBlockId;
      Debug.Assert(_blocks.ContainsKey(targetBlockId));
      BaseBlock targetBlock = _blocks[targetBlockId];
      Debug.Assert(targetBlock is IDockable);
      (targetBlock as IDockable).UnDock(block, newCanvasPosition);
      _rootBlockIdSet.Add(blockId);
    }

    // Removes a group of docked blocks. The block group is specified by the ID of the root block.
    // This method can also be used to remove a standalone block.
    public void RemoveBlocks(string rootBlockId) {
      Debug.Assert(_rootBlockIdSet.Contains(rootBlockId));
      Debug.Assert(_blocks.ContainsKey(rootBlockId));
      BaseBlock block = _blocks[rootBlockId];
      _rootBlockIdSet.Remove(rootBlockId);
      var collector = new DockedBlockCollector();
      block.Accept(collector);
      foreach (string id in collector.BlockIds) {
        _blocks.Remove(id);
      }
    }

    // Loads a group of blocks into the module as a batch operation.
    //
    // The blocks already have their IDs before they are loaded to the module, and the _idGenerator
    // will be reset with the maximum existing ID as its last ID after the batch loading.
    //
    // The module itself will be cleared before the batch loading.
    public void LoadBlocks(IReadOnlyCollection<BaseBlock> _) {
      // TODO: Consider supporting async since this operation may cost some time.
      Clear();
      // TODO: implement the batch loading.
    }
  }
}
