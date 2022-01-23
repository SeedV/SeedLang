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
  // A helper class to collect positions of jump bytecodes for nested "if" and "while" statements
  // during compilation.
  internal class NestedJumpStack {
    private class Frame {
      // A list to collect positions of all true jump bytecode.
      public readonly List<int> TrueJumps = new List<int>();
      // A list to collect bytecode positions of all false jump bytecode.
      public readonly List<int> FalseJumps = new List<int>();
    }

    public List<int> TrueJumps => _frames.Peek().TrueJumps;
    public List<int> FalseJumps => _frames.Peek().FalseJumps;

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    // Pushes a new frame when a "if" or "while" statement is visited by the compiler.
    internal void PushFrame() {
      _frames.Push(new Frame());
    }

    // Pops a frame when a "if" or "while" statement is finished visiting.
    internal void PopFrame() {
      Debug.Assert(_frames.Count > 0 && _frames.Peek().TrueJumps.Count == 0 &&
                   _frames.Peek().FalseJumps.Count == 0);
      _frames.Pop();
    }
  }
}
