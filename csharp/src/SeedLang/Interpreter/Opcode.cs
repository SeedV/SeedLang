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

namespace SeedLang.Interpreter {
  // All the opcodes of SeedLang virtual machine.
  internal enum Opcode {
    MOVE,         // R(A) := RK(B)
    LOADBOOL,     // R(A) := (Bool)B; if C then PC++
    LOADK,        // R(A) := Kst(Bx)
    GETGLOB,      // R[A] := Gbl[Kst(Bx)]
    SETGLOB,      // Gbl[Kst(Bx)] := R[A]
    NEWTUPLE,     // R(A) := (R(B), R(B+1), ..., R(B+C-1))
    NEWLIST,      // R(A) := [R(B), R(B+1), ..., R(B+C-1)]
    GETELEM,      // R(A) := R(B)[RK(C)]
    SETELEM,      // R(A)[RK(B)] := RK(C)
    ADD,          // R(A) := RK(B) + RK(C)
    SUB,          // R(A) := RK(B) - RK(C)
    MUL,          // R(A) := RK(B) * RK(C)
    DIV,          // R(A) := RK(B) / RK(C)
    UNM,          // R(A) := -RK(B)
    LEN,          // R(A) := length of R(B)
    JMP,          // PC += sBx
    EQ,           // if (RK(B) == RK(C)) != A then PC++
    LT,           // if (RK(B) < RK(C)) != A then PC++
    LE,           // if (RK(B) <= RK(C)) != A then PC++
    TEST,         // if R(A) == C then PC++
    TESTSET,      // if R(B) != C then R(A) := R(B) else PC++
    FORPREP,      // R(A) -= R(A+2); pc += sBx
    FORLOOP,      // R(A) += R(A+2); if R(A) <?= R(A+1) then PC += sBx
    EVAL,         // Eval R(A). Evaluates the expresion statement. TODO: do we need this?
    CALL,         // call function R(A), parameters are R(A+1), ..., R(A+B)
    RETURN,       // if B == 0 return else return R(A)
  }

  // The types of opcodes.
  internal enum OpcodeType {
    A,
    ABC,
    ABx,
    SBx,
  }

  internal static class OpcodeExtension {
    // Returns the type of this opcode.
    internal static OpcodeType Type(this Opcode op) {
      switch (op) {
        case Opcode.EVAL:
          return OpcodeType.A;
        case Opcode.MOVE:
        case Opcode.LOADBOOL:
        case Opcode.NEWTUPLE:
        case Opcode.NEWLIST:
        case Opcode.GETELEM:
        case Opcode.SETELEM:
        case Opcode.ADD:
        case Opcode.SUB:
        case Opcode.MUL:
        case Opcode.DIV:
        case Opcode.UNM:
        case Opcode.LEN:
        case Opcode.EQ:
        case Opcode.LT:
        case Opcode.LE:
        case Opcode.TEST:
        case Opcode.TESTSET:
        case Opcode.CALL:
        case Opcode.RETURN:
          return OpcodeType.ABC;
        case Opcode.LOADK:
        case Opcode.GETGLOB:
        case Opcode.SETGLOB:
          return OpcodeType.ABx;
        case Opcode.JMP:
        case Opcode.FORPREP:
        case Opcode.FORLOOP:
          return OpcodeType.SBx;
        default:
          throw new NotImplementedException($"Unsupported opcode: {op}.");
      }
    }
  }
}
