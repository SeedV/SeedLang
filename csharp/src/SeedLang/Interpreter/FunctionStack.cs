using System;
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
  internal class FunctionStack {
    private class Frame {
      public Function Func { get; }
      public ConstantCache ConstantCache { get; } = new ConstantCache();
      public RegisterAllocator RegisterAllocator { get; } = new RegisterAllocator();

      internal Frame(string name) {
        Func = new Function(name);
      }

      internal void UpdateFunc() {
        Func.Chunk.RegisterCount = RegisterAllocator.MaxRegisterCount;
        Func.Chunk.SetConstants(ConstantCache.Constants.ToArray());
      }
    }

    public int Count => _frames.Count;

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    internal void PushFunc(string name) {
      _frames.Push(new Frame(name));
    }

    internal Function PopFunc() {
      UpdateCurrentFunc();
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

    internal ConstantCache CurrentConstantCache() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().ConstantCache;
    }

    internal RegisterAllocator CurrentRegisterAllocator() {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().RegisterAllocator;
    }

    internal void UpdateCurrentFunc() {
      _frames.Peek().UpdateFunc();
    }
  }
}
