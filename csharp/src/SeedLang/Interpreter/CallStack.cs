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
  internal class CallStack {
    private class Frame {
      public Function Func { get; }

      internal Frame(Function func) {
        Func = func;
      }
    }

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    internal void PushFunc(Function func) {
      _frames.Push(new Frame(func));
    }

    internal void PopFunc() {
      _frames.Pop();
    }

    internal Chunk CurrentChunk() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().Func.Chunk;
    }
  }
}
