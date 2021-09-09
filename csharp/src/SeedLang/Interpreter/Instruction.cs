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

using System;
using System.Diagnostics;

namespace SeedLang.Interpreter {
  // The instruction data structure that includes the opcode and operands.
  //
  // See design/seed_vm.md for the layout of instructions.
  internal struct Instruction {
    private const int _lengthOpcode = 6;
    private const int _lengthA = 8;
    private const int _lengthB = 9;
    private const int _lengthC = sizeof(uint) - _lengthOpcode - _lengthA - _lengthB;
    private const int _lengthBx = sizeof(uint) - _lengthOpcode - _lengthA;

    private const int _posA = _lengthOpcode;
    private const int _posB = _lengthOpcode + _lengthA;
    private const int _posC = _lengthOpcode + _lengthA + _lengthB;
    private const int _posBx = _posB;

    private const uint _maskOpcode = (1 << _posA) - 1;
    private const uint _maskA = (1 << _lengthA) - 1;
    private const uint _maskB = (1 << _lengthB) - 1;
    private const uint _maskC = (1 << _lengthC) - 1;
    private const uint _maskBx = (1 << _lengthBx) - 1;

    public Opcode Opcode => (Opcode)(_code & _maskOpcode);
    public uint A => (_code >> _posA) & _maskA;
    public uint B => (_code >> _posB) & _maskB;
    public uint C => (_code >> _posC) & _maskC;
    public uint Bx => (_code >> _posBx) & _maskBx;

    private readonly uint _code;

    internal Instruction(Opcode opcode, uint a) {
      Debug.Assert(opcode.Type() == OpcodeType.A, $"{opcode} shall be type A opcode.");
      _code = (uint)opcode | a << _posA;
    }

    internal Instruction(Opcode opcode, uint a, uint b, uint c) {
      Debug.Assert(opcode.Type() == OpcodeType.ABC, $"{opcode} shall be type ABC opcode.");
      _code = (uint)opcode | a << _posA | b << _posB | c << _posC;
    }

    internal Instruction(Opcode opcode, uint a, uint bx) {
      Debug.Assert(opcode.Type() == OpcodeType.ABx, $"{opcode} shall be type ABx opcode.");
      _code = (uint)opcode | a << _posA | bx << _posBx;
    }

    public override string ToString() {
      switch (Opcode.Type()) {
        case OpcodeType.A:
          return $"{Opcode} {A}";
        case OpcodeType.ABC:
          return $"{Opcode} {A} {B} {C}";
        case OpcodeType.ABx:
          return $"{Opcode} {A} {Bx}";
        default:
          throw new NotImplementedException();
      }
    }
  }
}
