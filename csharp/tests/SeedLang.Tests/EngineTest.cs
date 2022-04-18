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
using Xunit;

namespace SeedLang.Tests {
  public class EngineTests {
    [Fact]
    public void TestCompileBreak() {
      string source = @"
for i in range(10):
  if i == 5:
    break
  j = i
  while j < 10:
    if j == 8:
      break
    j += 1
  i += 1
";
      string result = (
        $"Function <main>\n" +
        $"  1    GETGLOB   0 5                                  [Ln 2, Col 9 - Ln 2, Col 13]\n" +
        $"  2    LOADK     1 -1             ; 10                [Ln 2, Col 15 - Ln 2, Col 16]\n" +
        $"  3    CALL      0 1 0                                [Ln 2, Col 9 - Ln 2, Col 17]\n" +
        $"  4    LOADK     1 -2             ; 0                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  5    LEN       2 0 0                                [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  6    LOADK     3 -3             ; 1                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  7    FORPREP   1 19             ; to 27             [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  8    GETELEM   4 0 1                                [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  9    SETGLOB   4 6                                  [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  10   GETGLOB   4 6                                  [Ln 3, Col 5 - Ln 3, Col 5]\n" +
        $"  11   EQ        1 4 -4           ; 5                 [Ln 3, Col 5 - Ln 3, Col 10]\n" +
        $"  12   JMP       0 1              ; to 14             [Ln 3, Col 5 - Ln 3, Col 10]\n" +
        $"  13   JMP       0 14             ; to 28             [Ln 4, Col 4 - Ln 4, Col 8]\n" +
        $"  14   GETGLOB   5 6                                  [Ln 5, Col 6 - Ln 5, Col 6]\n" +
        $"  15   MOVE      4 5                                  [Ln 5, Col 2 - Ln 5, Col 6]\n" +
        $"  16   LT        1 4 -1           ; 10                [Ln 6, Col 8 - Ln 6, Col 13]\n" +
        $"  17   JMP       0 6              ; to 24             [Ln 6, Col 8 - Ln 6, Col 13]\n" +
        $"  18   EQ        1 4 -5           ; 8                 [Ln 7, Col 7 - Ln 7, Col 12]\n" +
        $"  19   JMP       0 1              ; to 21             [Ln 7, Col 7 - Ln 7, Col 12]\n" +
        $"  20   JMP       0 3              ; to 24             [Ln 8, Col 6 - Ln 8, Col 10]\n" +
        $"  21   ADD       5 4 -3           ; 1                 [Ln 9, Col 4 - Ln 9, Col 9]\n" +
        $"  22   MOVE      4 5                                  [Ln 9, Col 4 - Ln 9, Col 9]\n" +
        $"  23   JMP       0 -8             ; to 16             [Ln 6, Col 2 - Ln 9, Col 9]\n" +
        $"  24   GETGLOB   6 6                                  [Ln 10, Col 2 - Ln 10, Col 2]\n" +
        $"  25   ADD       5 6 -3           ; 1                 [Ln 10, Col 2 - Ln 10, Col 7]\n" +
        $"  26   SETGLOB   5 6                                  [Ln 10, Col 2 - Ln 10, Col 7]\n" +
        $"  27   FORLOOP   1 -20            ; to 8              [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  28   RETURN    0 0                                  [Ln 10, Col 2 - Ln 10, Col 7]\n"
      ).Replace("\n", Environment.NewLine);
      TestEngineCompile(source, result);
    }

    private static void TestEngineCompile(string source, string result) {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      engine.Compile(source, "");
      Assert.Equal(result, engine.Disassemble());
    }
  }
}
