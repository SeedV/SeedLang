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

namespace SeedLang.Interpreter {
  // All the opcodes of SeedLang virtual machine.
  internal enum Opcode {
    LOADK,        // R(A) := Kst(Bx)
    GETGLOB,      // R[A] := Gbl[Kst(Bx)]
    SETGLOB,      // Gbl[Kst(Bx)] := R[A]
    ADD,          // R(A) := RK(B)  RK(C)
    SUB,          // R(A) := RK(B) - RK(C)
    MUL,          // R(A) := RK(B) * RK(C)
    DIV,          // R(A) := RK(B) / RK(C)
    UNM,          // R(A) := -RK(B)
    EVAL,         // Eval R(A). Evaluates the expresion statement. TODO: do we need this?
    RETURN,       // Return R(A)
  }

  // The types of opcodes.
  internal enum OpcodeType {
    A,
    ABC,
    ABx,
  }

  internal static class OpcodeExtension {
    // Returns the type of this opcode.
    internal static OpcodeType Type(this Opcode op) {
      switch (op) {
        case Opcode.LOADK:
        case Opcode.GETGLOB:
        case Opcode.SETGLOB:
          return OpcodeType.ABx;
        case Opcode.ADD:
        case Opcode.SUB:
        case Opcode.MUL:
        case Opcode.DIV:
        case Opcode.UNM:
          return OpcodeType.ABC;
        case Opcode.EVAL:
        case Opcode.RETURN:
          return OpcodeType.A;
        default:
          throw new NotImplementedException("Unsupported opcode: {op}.");
      }
    }
  }
}
