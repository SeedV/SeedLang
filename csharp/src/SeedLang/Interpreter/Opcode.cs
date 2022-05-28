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
    LOADNIL,      // R(A), R(A+1), ..., R(A+B-1) := nil, B is the count of target registers
    LOADBOOL,     // R(A) := (Bool)B; if C then PC++
    LOADK,        // R(A) := Kst(Bx)
    GETGLOB,      // R[A] := Gbl[Kst(Bx)]
    SETGLOB,      // Gbl[Kst(Bx)] := R[A]
    NEWTUPLE,     // R(A) := (R(B), R(B+1), ..., R(B+C-1)), C is the count of initial elements
    NEWLIST,      // R(A) := [R(B), R(B+1), ..., R(B+C-1)], C is the count of initial elements
    NEWDICT,      // R(A) := {R(B): R(B+1), ..., R(B+C-2): R(B+C-1)], C is the count of initial keys
                  // and values
    GETELEM,      // R(A) := R(B)[RK(C)]
    SETELEM,      // R(A)[RK(B)] := RK(C)
    ADD,          // R(A) := RK(B) + RK(C)
    SUB,          // R(A) := RK(B) - RK(C)
    MUL,          // R(A) := RK(B) * RK(C)
    DIV,          // R(A) := RK(B) / RK(C)
    FLOORDIV,     // R(A) := RK(B) // RK(C)
    POW,          // R(A) := RK(B) ** RK(C)
    MOD,          // R(A) := RK(B) % RK(C)
    UNM,          // R(A) := -RK(B)
    LEN,          // R(A) := length of R(B)
    JMP,          // PC += sBx
    EQ,           // if (RK(B) == RK(C)) != A then PC++
    LT,           // if (RK(B) < RK(C)) != A then PC++
    LE,           // if (RK(B) <= RK(C)) != A then PC++
    IN,           // if (RK(B) in RK(C)) != A then PC++
    TEST,         // if R(A) == C then PC++
    TESTSET,      // if R(B) != C then R(A) := R(B) else PC++
    FORPREP,      // R(A) -= R(A+2); pc += sBx
    FORLOOP,      // R(A) += R(A+2); if R(A) <?= R(A+1) then PC += sBx
    CALL,         // calls function R(A), parameters are R(A+1), ..., R(A+B), B is the count of
                  // parameters
    RETURN,       // returns R(A), R(A+1), ..., R(A+B-1), B is the count of return values
    VISNOTIFY,    // creates a notification event from NotifyInfo[Bx], and sends to visualizers. A
                  // is the customized data of notifications
    HALT,         // Halts the execution. A == 1 indicates termination of the program
  }

  // The types of opcodes.
  internal enum OpcodeType {
    ABC,
    ABx,
    SBx,
  }

  internal static class OpcodeExtension {
    // Returns the type of this opcode.
    internal static OpcodeType Type(this Opcode op) {
      return op switch {
        Opcode.MOVE => OpcodeType.ABC,
        Opcode.LOADNIL => OpcodeType.ABC,
        Opcode.LOADBOOL => OpcodeType.ABC,
        Opcode.NEWTUPLE => OpcodeType.ABC,
        Opcode.NEWLIST => OpcodeType.ABC,
        Opcode.NEWDICT => OpcodeType.ABC,
        Opcode.GETELEM => OpcodeType.ABC,
        Opcode.SETELEM => OpcodeType.ABC,
        Opcode.ADD => OpcodeType.ABC,
        Opcode.SUB => OpcodeType.ABC,
        Opcode.MUL => OpcodeType.ABC,
        Opcode.DIV => OpcodeType.ABC,
        Opcode.FLOORDIV => OpcodeType.ABC,
        Opcode.POW => OpcodeType.ABC,
        Opcode.MOD => OpcodeType.ABC,
        Opcode.UNM => OpcodeType.ABC,
        Opcode.LEN => OpcodeType.ABC,
        Opcode.EQ => OpcodeType.ABC,
        Opcode.LT => OpcodeType.ABC,
        Opcode.LE => OpcodeType.ABC,
        Opcode.IN => OpcodeType.ABC,
        Opcode.TEST => OpcodeType.ABC,
        Opcode.TESTSET => OpcodeType.ABC,
        Opcode.CALL => OpcodeType.ABC,
        Opcode.RETURN => OpcodeType.ABC,
        Opcode.HALT => OpcodeType.ABC,

        Opcode.LOADK => OpcodeType.ABx,
        Opcode.GETGLOB => OpcodeType.ABx,
        Opcode.SETGLOB => OpcodeType.ABx,
        Opcode.VISNOTIFY => OpcodeType.ABx,

        Opcode.JMP => OpcodeType.SBx,
        Opcode.FORPREP => OpcodeType.SBx,
        Opcode.FORLOOP => OpcodeType.SBx,

        _ => throw new NotImplementedException($"Unsupported opcode: {op}."),
      };
    }
  }
}
