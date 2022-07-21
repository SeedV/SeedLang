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
using System.Reflection;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Tests {
  public class CompilePythonTests {
    private static readonly int _firstGlob = 14;
    private static readonly int _rangeFunc = NativeFunctionIdOf(BuiltinsDefinition.Range);

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
        $"  1    GETGLOB   0 {_rangeFunc}" +
        $"                                  [Ln 2, Col 9 - Ln 2, Col 13]\n" +
        $"  2    LOADK     1 -1             ; 10                [Ln 2, Col 15 - Ln 2, Col 16]\n" +
        $"  3    CALL      0 1 0                                [Ln 2, Col 9 - Ln 2, Col 17]\n" +
        $"  4    LOADK     1 -2             ; 0                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  5    LEN       2 0 0                                [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  6    LOADK     3 -3             ; 1                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  7    FORPREP   1 22             ; to 30             [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  8    GETELEM   4 0 1                                [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  9    SETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  10   GETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 3, Col 5 - Ln 3, Col 5]\n" +
        $"  11   EQ        1 4 -4           ; 5                 [Ln 3, Col 5 - Ln 3, Col 10]\n" +
        $"  12   JMP       0 1              ; to 14             [Ln 3, Col 5 - Ln 3, Col 10]\n" +
        $"  13   JMP       0 17             ; to 31             [Ln 4, Col 4 - Ln 4, Col 8]\n" +
        $"  14   GETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 5, Col 6 - Ln 5, Col 6]\n" +
        $"  15   SETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 5, Col 2 - Ln 5, Col 6]\n" +
        $"  16   GETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 6, Col 8 - Ln 6, Col 8]\n" +
        $"  17   LT        1 4 -1           ; 10                [Ln 6, Col 8 - Ln 6, Col 13]\n" +
        $"  18   JMP       0 8              ; to 27             [Ln 6, Col 8 - Ln 6, Col 13]\n" +
        $"  19   GETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 7, Col 7 - Ln 7, Col 7]\n" +
        $"  20   EQ        1 4 -5           ; 8                 [Ln 7, Col 7 - Ln 7, Col 12]\n" +
        $"  21   JMP       0 1              ; to 23             [Ln 7, Col 7 - Ln 7, Col 12]\n" +
        $"  22   JMP       0 4              ; to 27             [Ln 8, Col 6 - Ln 8, Col 10]\n" +
        $"  23   GETGLOB   5 {_firstGlob + 1}" +
        $"                                 [Ln 9, Col 4 - Ln 9, Col 4]\n" +
        $"  24   ADD       4 5 -3           ; 1                 [Ln 9, Col 4 - Ln 9, Col 9]\n" +
        $"  25   SETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 9, Col 4 - Ln 9, Col 9]\n" +
        $"  26   JMP       0 -11            ; to 16             [Ln 6, Col 2 - Ln 9, Col 9]\n" +
        $"  27   GETGLOB   5 {_firstGlob}" +
        $"                                 [Ln 10, Col 2 - Ln 10, Col 2]\n" +
        $"  28   ADD       4 5 -3           ; 1                 [Ln 10, Col 2 - Ln 10, Col 7]\n" +
        $"  29   SETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 10, Col 2 - Ln 10, Col 7]\n" +
        $"  30   FORLOOP   1 -23            ; to 8              [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  31   HALT      1 0                                  [Ln 10, Col 2 - Ln 10, Col 7]\n"
      ).Replace("\n", Environment.NewLine);
      TestCompile(source, result);
    }

    [Fact]
    public void TestCompileContinue() {
      string source = @"
for i in range(10):
  if i == 5:
    continue
  j = i
  while j < 10:
    if j == 8:
      continue
    j += 1
  i += 1
";
      string result = (
        $"Function <main>\n" +
        $"  1    GETGLOB   0 {_rangeFunc}" +
        $"                                  [Ln 2, Col 9 - Ln 2, Col 13]\n" +
        $"  2    LOADK     1 -1             ; 10                [Ln 2, Col 15 - Ln 2, Col 16]\n" +
        $"  3    CALL      0 1 0                                [Ln 2, Col 9 - Ln 2, Col 17]\n" +
        $"  4    LOADK     1 -2             ; 0                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  5    LEN       2 0 0                                [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  6    LOADK     3 -3             ; 1                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  7    FORPREP   1 22             ; to 30             [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  8    GETELEM   4 0 1                                [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  9    SETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  10   GETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 3, Col 5 - Ln 3, Col 5]\n" +
        $"  11   EQ        1 4 -4           ; 5                 [Ln 3, Col 5 - Ln 3, Col 10]\n" +
        $"  12   JMP       0 1              ; to 14             [Ln 3, Col 5 - Ln 3, Col 10]\n" +
        $"  13   JMP       0 16             ; to 30             [Ln 4, Col 4 - Ln 4, Col 11]\n" +
        $"  14   GETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 5, Col 6 - Ln 5, Col 6]\n" +
        $"  15   SETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 5, Col 2 - Ln 5, Col 6]\n" +
        $"  16   GETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 6, Col 8 - Ln 6, Col 8]\n" +
        $"  17   LT        1 4 -1           ; 10                [Ln 6, Col 8 - Ln 6, Col 13]\n" +
        $"  18   JMP       0 8              ; to 27             [Ln 6, Col 8 - Ln 6, Col 13]\n" +
        $"  19   GETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 7, Col 7 - Ln 7, Col 7]\n" +
        $"  20   EQ        1 4 -5           ; 8                 [Ln 7, Col 7 - Ln 7, Col 12]\n" +
        $"  21   JMP       0 1              ; to 23             [Ln 7, Col 7 - Ln 7, Col 12]\n" +
        $"  22   JMP       0 -7             ; to 16             [Ln 8, Col 6 - Ln 8, Col 13]\n" +
        $"  23   GETGLOB   5 {_firstGlob + 1}" +
        $"                                 [Ln 9, Col 4 - Ln 9, Col 4]\n" +
        $"  24   ADD       4 5 -3           ; 1                 [Ln 9, Col 4 - Ln 9, Col 9]\n" +
        $"  25   SETGLOB   4 {_firstGlob + 1}" +
        $"                                 [Ln 9, Col 4 - Ln 9, Col 9]\n" +
        $"  26   JMP       0 -11            ; to 16             [Ln 6, Col 2 - Ln 9, Col 9]\n" +
        $"  27   GETGLOB   5 {_firstGlob}" +
        $"                                 [Ln 10, Col 2 - Ln 10, Col 2]\n" +
        $"  28   ADD       4 5 -3           ; 1                 [Ln 10, Col 2 - Ln 10, Col 7]\n" +
        $"  29   SETGLOB   4 {_firstGlob}" +
        $"                                 [Ln 10, Col 2 - Ln 10, Col 7]\n" +
        $"  30   FORLOOP   1 -23            ; to 8              [Ln 2, Col 0 - Ln 10, Col 7]\n" +
        $"  31   HALT      1 0                                  [Ln 10, Col 2 - Ln 10, Col 7]\n"
      ).Replace("\n", Environment.NewLine);
      TestCompile(source, result);
    }

    private static void TestCompile(string source, string expectedResult) {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      engine.Compile(source, "");
      engine.Disassemble(out string result);
      Assert.Equal(expectedResult, result);
    }

    private static int NativeFunctionIdOf(string name) {
      var fields = typeof(BuiltinsDefinition).GetFields(BindingFlags.Public | BindingFlags.Static);
      return Array.FindIndex(fields, field => {
        return field.GetValue(null) as string == name;
      });
    }
  }
}
