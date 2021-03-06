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
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // A data structure to hold bytecode and constants generated from the AST tree by the compiler.
  internal class Chunk {
    // The maximum number of registers that can be allocated in the stack of a chunk.
    public const uint MaxRegisterCount = 250;

    // The bytecode of this chunk.
    public IReadOnlyList<Instruction> Bytecode => _bytecode;
    public int LatestCodePos => _bytecode.Count - 1;
    // The notification information list of this chunk.
    public IReadOnlyList<Notification.AbstractNotification> Notifications => _notifications;

    // Source code ranges of the instructions in bytecode.
    //
    // The length of Bytecode and Range lists shall be the same.
    public IReadOnlyList<TextRange> Ranges => _ranges;

    private readonly List<Instruction> _bytecode = new List<Instruction>();

    // The position of the breakpoint. Only support one breakpoint now.
    private int? _breakpointPos;
    // The original instruction at the position of the breakpoint. It is replaced with a HALT
    // instruction when the breakpoint is set.
    private Instruction _instructionAtBreakPoint;

    private readonly List<TextRange> _ranges = new List<TextRange>();

    // The constant list to hold all the constants used in this chunk.
    private VMValue[] _constants;
    // The notification information list that is used by VISNOTIFY opcode to create the
    // corresponding notification events and send to visualizers.
    private Notification.AbstractNotification[] _notifications;


    internal static bool IsConstId(uint id) {
      return id >= MaxRegisterCount;
    }

    // Emits an instruction with the opcode of type ABC.
    internal void Emit(Opcode opcode, uint a, uint b, uint c, TextRange range) {
      if (range is null) {
        throw new ArgumentNullException(nameof(range));
      }
      _bytecode.Add(new Instruction(opcode, a, b, c));
      _ranges.Add(range);
    }

    // Emits an instruction with the opcode of type ABx.
    internal void Emit(Opcode opcode, uint a, uint bx, TextRange range) {
      if (range is null) {
        throw new ArgumentNullException(nameof(range));
      }
      _bytecode.Add(new Instruction(opcode, a, bx));
      _ranges.Add(range);
    }

    // Emits an instruction with the opcode of type SBx.
    internal void Emit(Opcode opcode, uint a, int sbx, TextRange range) {
      if (range is null) {
        throw new ArgumentNullException(nameof(range));
      }
      _bytecode.Add(new Instruction(opcode, a, sbx));
      _ranges.Add(range);
    }

    // Patches the SBx field of an instruction.
    internal void PatchSBXAt(int pos, int sbx) {
      _bytecode[pos] = new Instruction(_bytecode[pos].Opcode, _bytecode[pos].A, sbx);
    }

    // Sets the breakpoint at the given position. The original instruction will be stored, and
    // replaced with a HALT instruction.
    internal void SetBreakpointAt(int pos) {
      Debug.Assert(pos >= 0 && pos < _bytecode.Count);
      _breakpointPos = pos;
      _instructionAtBreakPoint = _bytecode[pos];
      _bytecode[pos] = new Instruction(Opcode.HALT, (uint)HaltReason.Breakpoint, 0, 0);
    }

    // Restores the breakpoint with the stored instruction.
    internal void RestoreBreakpoint() {
      if (_breakpointPos is int breakpointPos) {
        _bytecode[breakpointPos] = _instructionAtBreakPoint;
        _breakpointPos = default;
      }
    }

    // Sets the constant list. It must be called by the compiler after compilation.
    internal void SetConstants(VMValue[] constants) {
      _constants = constants;
    }

    // Sets the notification list. It must be called by the compiler after compilation.
    internal void SetNotifications(Notification.AbstractNotification[] notifications) {
      _notifications = notifications;
    }

    internal bool IsConstIdValid(uint constId) {
      return IsConstId(constId) && constId - MaxRegisterCount < _constants.Length;
    }

    // Gets the constant value of the given constId. Returns a readonly reference to avoid copying.
    internal ref readonly VMValue ValueOfConstId(uint constId) {
      return ref _constants[IndexOfConstId(constId)];
    }

    // Converts the constant id to the index in the constant list.
    private int IndexOfConstId(uint constId) {
      Debug.Assert(IsConstIdValid(constId), "Invalid constant id.");
      return (int)(constId - MaxRegisterCount);
    }
  }
}
