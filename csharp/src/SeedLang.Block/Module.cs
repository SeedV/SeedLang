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

namespace SeedLang.Block {
  // The in-memory representation of a SeedBlock module.
  //
  // A module defines a namespace in the SeedBlock program. Syntactically it contains a set of
  // blocks and maintains their positions and relationships.
  public class Module {
    private readonly List<BaseBlock> _blocks;

    // A readonly interface for the client to access the contents of the module.
    public IReadOnlyList<BaseBlock> Blocks => _blocks;

    public Module() {
      _blocks = new List<BaseBlock>();
    }

    // Clears the module.
    public void Clear() {
      _blocks.Clear();
    }

    // Adds a new block.
    public void Add(BaseBlock block) {
      _blocks.Add(block);
    }
  }
}
