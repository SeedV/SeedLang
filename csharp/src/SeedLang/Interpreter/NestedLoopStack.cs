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

using System.Collections.Generic;
using System.Diagnostics;

namespace SeedLang.Interpreter {
  // A helper class to collect positions of break and continue jump bytecode for nested "for in" and
  // "while" statements during compilation.
  internal class NestedLoopStack {
    private class Frame {
      // A list to collect positions of all break jump bytecode.
      public readonly List<int> BreakJumps = new List<int>();
    }

    public List<int> BreaksJumps => _frames.Peek().BreakJumps;

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    // Pushes a new loop frame when a "for in" or "while" statement is visited by the compiler.
    internal void PushLoopFrame() {
      _frames.Push(new Frame());
    }

    // Pops a frame when a "for in" or "while" statement is finished visiting.
    internal void PopLoopFrame() {
      Debug.Assert(_frames.Count > 0, "Frames shall be pushed into the stack before.");
      Debug.Assert(_frames.Peek().BreakJumps.Count == 0, "Break jumps shall be patched.");
      _frames.Pop();
    }

    internal void AddBreakJump(int jumpPos) {
      _frames.Peek().BreakJumps.Add(jumpPos);
    }
  }
}
