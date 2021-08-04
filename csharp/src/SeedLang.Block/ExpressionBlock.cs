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
using System.Text;
using SeedLang.Common;

namespace SeedLang.Block {
  // The block that represents an expression.
  public class ExpressionBlock : BaseBlock, IEditable, IDockable {
    private readonly List<BaseBlock> _inputs = new List<BaseBlock>();

    public IReadOnlyList<BaseBlock> Inputs => _inputs;

    public string GetEditableText() {
      var sb = new StringBuilder();
      foreach (var input in _inputs) {
        sb.Append((input as IEditable).GetEditableText());
      }
      return sb.ToString();
    }

    public bool UpdateText(string text) {
      // TODO: Validate and parse the input text with the underlying SeedLang engine.

      // TODO: Fill _inputs with the parse result.

      return true;
    }

    public bool CanDock(BaseBlock block, Position.DockType type, int dockPosition) {
      // TODO: Support docking as the next statement.
      return type == Position.DockType.Input &&
          block is IEditable &&
          dockPosition >= 0 && dockPosition <= _inputs.Count;
    }

    public void Dock(BaseBlock block, Position.DockType type, int dockPosition) {
      // TODO: Support docking as the next statement.
      Debug.Assert(CanDock(block, type, dockPosition));
      InsertInputBlock(dockPosition, block);
    }

    public void UnDock(BaseBlock block, Vector2 newCanvasPosition) {
      RemoveInputBlock(block);
      block.Pos.Type = Position.DockType.UnDocked;
      block.Pos.CanvasPosition = newCanvasPosition;
    }

    public override void Accept(IBlockVisitor visitor) {
      visitor.VisitEnter(this);
      foreach (var input in _inputs) {
        input.Accept(visitor);
      }
      visitor.VisitExit(this);
    }

    protected void InsertInputBlock(int index, BaseBlock block) {
      _inputs.Insert(index, block);
      Debug.Assert(!string.IsNullOrEmpty(block.Id));
      UpdateInputPositions(index);
    }

    protected void RemoveInputBlock(BaseBlock block) {
      int index = _inputs.IndexOf(block);
      Debug.Assert(index >= 0);
      _inputs.RemoveAt(index);
      UpdateInputPositions(index);
    }

    protected void UpdateInputPositions(int startIndex) {
      for (int i = startIndex; i < _inputs.Count; i++) {
        var inputBlock = _inputs[i];
        inputBlock.Pos.Type = Position.DockType.Input;
        inputBlock.Pos.TargetBlockId = Id;
        inputBlock.Pos.DockPosition = i;
      }
    }
  }
}
