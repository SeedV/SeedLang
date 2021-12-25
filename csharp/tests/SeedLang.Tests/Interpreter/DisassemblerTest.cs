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
using SeedLang.Common;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class DisassemblerTests {
    private static TextRange _testTextRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestDisassember() {
      var chunk = new Chunk();
      var cache = new ConstantCache();
      chunk.Emit(Opcode.LOADK, 0, cache.IdOfConstant(1), _testTextRange);
      chunk.Emit(Opcode.GETGLOB, 1, cache.IdOfConstant("global_variable"), _testTextRange);
      chunk.Emit(Opcode.SETGLOB, 1, cache.IdOfConstant("name"), _testTextRange);
      chunk.Emit(Opcode.ADD, 0, 1, 2, _testTextRange);
      chunk.Emit(Opcode.SUB, 0, cache.IdOfConstant(2), 2, _testTextRange);
      chunk.Emit(Opcode.MUL, 0, 1, cache.IdOfConstant(3), _testTextRange);
      chunk.Emit(Opcode.DIV, 0, cache.IdOfConstant(4), cache.IdOfConstant(5), _testTextRange);
      chunk.Emit(Opcode.UNM, 0, cache.IdOfConstant(6), 0, _testTextRange);
      chunk.Emit(Opcode.RETURN, 0);
      chunk.SetConstants(cache.Constants.ToArray());
      string expected = (
          "1    LOADK     0 -1           ; 1                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "2    GETGLOB   1 -2           ; global_variable   [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "3    SETGLOB   1 -3           ; name              [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "4    ADD       0 1 2                              [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "5    SUB       0 -4 2         ; 2                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "6    MUL       0 1 -5         ; 3                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "7    DIV       0 -6 -7        ; 4 5               [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "8    UNM       0 -8           ; 6                 [Ln 0, Col 1 - Ln 2, Col 3]\n" +
          "9    RETURN    0                                  \n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, new Disassembler(chunk).ToString());
    }
  }
}
