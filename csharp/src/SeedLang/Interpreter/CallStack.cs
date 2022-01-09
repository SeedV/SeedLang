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
  // A stack used in SeedVM to store running functions, base register indices and program counters.
  internal class CallStack {
    private class Frame {
      public Function Func { get; }
      public uint BaseRegister { get; }
      public int PC { get; set; }

      internal Frame(Function func, uint baseRegister) {
        Func = func;
        BaseRegister = baseRegister;
      }
    }

    public bool IsEmpty => _frames.Count == 0;

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    internal void PushFunc(Function func, uint baseRegister, int pc) {
      if (_frames.Count > 0) {
        _frames.Peek().PC = pc;
      }
      _frames.Push(new Frame(func, baseRegister));
    }

    internal void PopFunc() {
      _frames.Pop();
    }

    internal Chunk CurrentChunk() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().Func.Chunk;
    }

    internal uint CurrentBase() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().BaseRegister;
    }

    internal int CurrentPC() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().PC;
    }
  }
}
