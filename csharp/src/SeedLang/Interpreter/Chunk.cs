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
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  using AbstractNotification = Notification.AbstractNotification;

  // A data structure to hold bytecode and constants generated from the AST tree by the compiler.
  internal class Chunk {
    // The maximum number of registers that can be allocated in the stack of a chunk.
    public const uint MaxRegisterCount = 250;

    // The bytecode of this chunk.
    public IReadOnlyList<Instruction> Bytecode => _bytecode;

    // Source code ranges of the instructions in bytecode.
    //
    // The length of Bytecode and Range lists shall be the same.
    public IReadOnlyList<Range> Ranges => _ranges;

    // The notification information list that is used by VISNOTIFY opcode to create the correspoding
    // notification events and sent to visualizers.
    public IReadOnlyList<AbstractNotification> Notifications => _notifications;

    private readonly List<Instruction> _bytecode = new List<Instruction>();

    private readonly List<Range> _ranges = new List<Range>();

    private readonly List<AbstractNotification> _notifications = new List<AbstractNotification>();

    // The constant list to hold all the constants used in this chunk.
    private Value[] _constants;

    internal static bool IsConstId(uint id) {
      return id >= MaxRegisterCount;
    }

    // Emits an instruction with the opcode of type ABC.
    internal void Emit(Opcode opcode, uint a, uint b, uint c, Range range) {
      _bytecode.Add(new Instruction(opcode, a, b, c));
      _ranges.Add(range);
    }

    // Emits an instruction with the opcode of type ABx.
    internal void Emit(Opcode opcode, uint a, uint bx, Range range) {
      _bytecode.Add(new Instruction(opcode, a, bx));
      _ranges.Add(range);
    }

    // Emits an instruction with the opcode of type SBx.
    internal void Emit(Opcode opcode, uint a, int sbx, Range range) {
      _bytecode.Add(new Instruction(opcode, a, sbx));
      _ranges.Add(range);
    }

    internal void PatchSBXAt(int pos, int sbx) {
      _bytecode[pos] = new Instruction(_bytecode[pos].Opcode, _bytecode[pos].A, sbx);
    }

    // Sets the constant list. It must be called by the compiler after compilation.
    internal void SetConstants(Value[] constants) {
      _constants = constants;
    }

    internal bool IsConstIdValid(uint constId) {
      return IsConstId(constId) && constId - MaxRegisterCount < _constants.Length;
    }

    // Gets the constant value of the given constId. Returns a readonly reference to avoid copying.
    internal ref readonly Value ValueOfConstId(uint constId) {
      return ref _constants[IndexOfConstId(constId)];
    }

    internal uint AddNotification(AbstractNotification notification) {
      _notifications.Add(notification);
      return (uint)_notifications.Count - 1;
    }

    // Converts the constant id to the index in the constant list.
    private int IndexOfConstId(uint constId) {
      Debug.Assert(IsConstIdValid(constId), "Invalid constant id.");
      return (int)(constId - MaxRegisterCount);
    }
  }
}
