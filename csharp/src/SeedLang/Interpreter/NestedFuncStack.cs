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
  // A stack used in the compiler to store compiled nested functions.
  internal class NestedFuncStack {
    private class Frame {
      public Function Func { get; }
      public ChunkCache Cache { get; } = new ChunkCache();

      // The source code line number (1-based) of the previous bytecode.
      public int SourceLineOfPrevBytecode { get; set; }

      internal Frame(string name) {
        Func = new Function(name);
      }

      internal void UpdateConstantArray() {
        Func.Chunk.SetConstants(Cache.ConstantArray());
        Func.Chunk.SetNotifications(Cache.NotificationArray());
      }
    }

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    internal void PushFunc(string name) {
      _frames.Push(new Frame(name));
    }

    internal Function PopFunc() {
      _frames.Peek().UpdateConstantArray();
      Function func = _frames.Peek().Func;
      _frames.Pop();
      return func;
    }

    internal Function CurrentFunc() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().Func;
    }

    internal Chunk CurrentChunk() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().Func.Chunk;
    }

    internal ChunkCache CurrentChunkCache() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().Cache;
    }

    internal int CurrentSourceLineOfPrevBytecode() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().SourceLineOfPrevBytecode;
    }

    internal void SetCurrentSourceLineOfPrevBytecode(int sourceLineOfPrevBytecode) {
      Debug.Assert(_frames.Count > 0);
      _frames.Peek().SourceLineOfPrevBytecode = sourceLineOfPrevBytecode;
    }
  }
}
