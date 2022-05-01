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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SeedLang.Common;

namespace SeedLang.Interpreter {
  // A helper class to collect positions of jump bytecode for nested expressions and statements.
  internal class NestedJumpStack {
    private class Frame {
      // A list array to collect positions of jump bytecode.
      public readonly List<int>[] Jumps;

      internal Frame(int jumpsCount) {
        Jumps = new List<int>[jumpsCount];
        for (int i = 0; i < jumpsCount; i++) {
          Jumps[i] = new List<int>();
        }
      }
    }

    protected bool IsEmpty() => _frames.Count == 0;

    private readonly Stack<Frame> _frames = new Stack<Frame>();
    // The count of collected jumps of each frame.
    private readonly int _jumpsCount;

    internal NestedJumpStack(int jumpsCount) {
      _jumpsCount = jumpsCount;
    }

    // Returns the jumps list of a given index.
    internal List<int> JumpsOf(int index) {
      Debug.Assert(_frames.Count > 0);
      return _frames.Peek().Jumps[index];
    }

    // Pushes a new frame when a nested expression or statement is visited by the compiler.
    internal void PushFrame() {
      _frames.Push(new Frame(_jumpsCount));
    }

    // Pops a frame when a nested expression or statement is finished visiting.
    internal void PopFrame() {
      Debug.Assert(_frames.Count > 0, "Frames shall be pushed before.");
      foreach (List<int> jumps in _frames.Peek().Jumps) {
        Debug.Assert(jumps.Count == 0, "Jumps shall be patched.");
      }
      _frames.Pop();
    }

    internal void AddJump(int index, int jumpPos) {
      JumpsOf(index).Add(jumpPos);
    }
  }

  // The class to collect positions of jump bytecode for nested loop statements.
  internal class NestedLoopStack : NestedJumpStack {
    private enum Index : int {
      Break,
      Continue,
    }

    public List<int> BreakJumps => JumpsOf((int)Index.Break);
    public List<int> ContinueJumps => JumpsOf((int)Index.Continue);

    internal NestedLoopStack() : base(Enum.GetNames(typeof(Index)).Length) { }

    internal void AddBreakJump(int jumpPos, TextRange range) {
      if (IsEmpty()) {
        throw new DiagnosticException(SystemReporters.SeedInterpreter, Severity.Fatal, "", range,
                                      Message.RuntimeErrorBreakOutsideLoop);
      }
      AddJump((int)Index.Break, jumpPos);
    }

    internal void AddContinueJump(int jumpPos, TextRange range) {
      if (IsEmpty()) {
        throw new DiagnosticException(SystemReporters.SeedInterpreter, Severity.Fatal, "", range,
                                      Message.RuntimeErrorContinueOutsideLoop);
      }
      AddJump((int)Index.Continue, jumpPos);
    }
  }

  // The class to collect positions of jump bytecode for nested comparison or boolean expressions.
  internal class ExprJumpStack : NestedJumpStack {
    private enum Index : int {
      False,
      True,
    }

    public List<int> FalseJumps => JumpsOf((int)Index.False);
    public List<int> TrueJumps => JumpsOf((int)Index.True);

    internal ExprJumpStack() : base(Enum.GetNames(typeof(Index)).Length) { }

    internal void AddFalseJump(int jumpPos) {
      AddJump((int)Index.False, jumpPos);
    }

    internal void AddTrueJump(int jumpPos) {
      AddJump((int)Index.True, jumpPos);
    }
  }
}
