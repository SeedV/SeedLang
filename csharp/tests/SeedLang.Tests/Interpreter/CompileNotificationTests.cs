using System.Linq;
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
using FluentAssertions;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using SeedLang.Visualization;
using SeedLang.X;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompileNotificationTests {
    private static readonly int _printValFunc = NativeFunctionIdOf(NativeFunctions.PrintVal);
    private static readonly int _firstGlob = NativeFunctions.Funcs.Count;

    [Fact]
    public void TestAssignment() {
      string source = "name = 1";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 1, Col 0 - Ln 1, Col 3]\n" +
        $"  2    LOADK     0 -1             ; 1                 [Ln 1, Col 7 - Ln 1, Col 7]\n" +
        $"  3    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 1, Col 0 - Ln 1, Col 7]\n" +
        $"  4    VISNOTIFY 0 1                                  [Ln 1, Col 0 - Ln 1, Col 7]\n" +
        $"  5    HALT      1 0                                  [Ln 1, Col 0 - Ln 1, Col 7]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'name' Global 7\n" +
        $"  1    Notification.Assignment: 'name' Global 0\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Assignment) }, RunMode.Interactive);
    }

    [Fact]
    public void TestBinary() {
      string source = "1 + 2";
      string expected = (
        $"Function <main>\n" +
        $"  1    GETGLOB   0 {_printValFunc}" +
        $"                                  [Ln 1, Col 0 - Ln 1, Col 4]\n" +
        $"  2    ADD       1 -1 -2          ; 1 2               [Ln 1, Col 0 - Ln 1, Col 4]\n" +
        $"  3    VISNOTIFY 0 0                                  [Ln 1, Col 0 - Ln 1, Col 4]\n" +
        $"  4    CALL      0 1 0                                [Ln 1, Col 0 - Ln 1, Col 4]\n" +
        $"  5    HALT      1 0                                  [Ln 1, Col 0 - Ln 1, Col 4]\n" +
        $"Notifications\n" +
        $"  0    Notification.Binary: 250 Add 251 1\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Binary) }, RunMode.Interactive);
    }

    [Fact]
    public void TestFuncCall() {
      string source = @"
def add(a, b):
  return a + b
add(1, 2)
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 3, Col 13]\n" +
        $"  2    LOADK     0 -1             ; Func <add>        [Ln 2, Col 0 - Ln 3, Col 13]\n" +
        $"  3    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 3, Col 13]\n" +
        $"  4    GETGLOB   0 {_printValFunc}" +
        $"                                  [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"  5    GETGLOB   1 {_firstGlob}" +
        $"                                  [Ln 4, Col 0 - Ln 4, Col 2]\n" +
        $"  6    LOADK     2 -2             ; 1                 [Ln 4, Col 4 - Ln 4, Col 4]\n" +
        $"  7    LOADK     3 -3             ; 2                 [Ln 4, Col 7 - Ln 4, Col 7]\n" +
        $"  8    VISNOTIFY 0 1                                  [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"  9    CALL      1 2 0                                [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"  10   VISNOTIFY 1 1                                  [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"  11   VISNOTIFY 0 2                                  [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"  12   CALL      0 1 0                                [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"  13   HALT      1 0                                  [Ln 4, Col 0 - Ln 4, Col 8]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'add' Global 7\n" +
        $"  1    Notification.Function: add 1 2\n" +
        $"  2    Notification.VariableDeleted: 0\n" +
        $"\n" +
        $"Function <add>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 8 - Ln 2, Col 8]\n" +
        $"  2    VISNOTIFY 0 1                                  [Ln 2, Col 11 - Ln 2, Col 11]\n" +
        $"  3    ADD       2 0 1                                [Ln 3, Col 9 - Ln 3, Col 13]\n" +
        $"  4    VISNOTIFY 0 2                                  [Ln 3, Col 9 - Ln 3, Col 13]\n" +
        $"  5    RETURN    2 1                                  [Ln 3, Col 2 - Ln 3, Col 13]\n" +
        $"  6    RETURN    0 0                                  [Ln 3, Col 2 - Ln 3, Col 13]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'add.a' Local 0\n" +
        $"  1    Notification.VariableDefined: 'add.b' Local 1\n" +
        $"  2    Notification.Binary: 0 Add 1 2\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.Binary),
        typeof(Event.FuncCalled),
        typeof(Event.FuncReturned),
      }, RunMode.Interactive);
    }

    [Fact]
    public void TestSingleStep() {
      var source = @"
def inc(n):
  return n + 2

sum = 0
i = 0
while i < 10:
  for j in range(5):
    if i < 8 and j < 3:
      # [[ Inc(sum) ]]
      sum = inc(sum)
  i += 1

print(sum)
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 2, Col 0]\n" +
        $"  2    LOADK     0 -1             ; Func <inc>        [Ln 2, Col 0 - Ln 3, Col 13]\n" +
        $"  3    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 3, Col 13]\n" +
        $"  4    VISNOTIFY 0 0                                  [Ln 5, Col 0 - Ln 5, Col 0]\n" +
        $"  5    LOADK     0 -2             ; 0                 [Ln 5, Col 6 - Ln 5, Col 6]\n" +
        $"  6    SETGLOB   0 {_firstGlob + 1}" +
        $"                                  [Ln 5, Col 0 - Ln 5, Col 6]\n" +
        $"  7    VISNOTIFY 0 0                                  [Ln 6, Col 0 - Ln 6, Col 0]\n" +
        $"  8    LOADK     0 -2             ; 0                 [Ln 6, Col 4 - Ln 6, Col 4]\n" +
        $"  9    SETGLOB   0 {_firstGlob + 2}" +
        $"                                  [Ln 6, Col 0 - Ln 6, Col 4]\n" +
        $"  10   VISNOTIFY 0 0                                  [Ln 7, Col 0 - Ln 7, Col 0]\n" +
        $"  11   GETGLOB   0 {_firstGlob + 2}" +
        $"                                  [Ln 7, Col 6 - Ln 7, Col 6]\n" +
        $"  12   LT        1 0 -3           ; 10                [Ln 7, Col 6 - Ln 7, Col 11]\n" +
        $"  13   JMP       0 29             ; to 43             [Ln 7, Col 6 - Ln 7, Col 11]\n" +
        $"  14   VISNOTIFY 0 0                                  [Ln 8, Col 0 - Ln 8, Col 0]\n" +
        $"  15   GETGLOB   0 5                                  [Ln 8, Col 11 - Ln 8, Col 15]\n" +
        $"  16   LOADK     1 -4             ; 5                 [Ln 8, Col 17 - Ln 8, Col 17]\n" +
        $"  17   CALL      0 1 0                                [Ln 8, Col 11 - Ln 8, Col 18]\n" +
        $"  18   LOADK     1 -2             ; 0                 [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  19   LEN       2 0 0                                [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  20   LOADK     3 -5             ; 1                 [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  21   FORPREP   1 15             ; to 37             [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  22   GETELEM   4 0 1                                [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  23   SETGLOB   4 {_firstGlob + 3}" +
        $"                                 [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  24   VISNOTIFY 0 0                                  [Ln 9, Col 0 - Ln 9, Col 0]\n" +
        $"  25   GETGLOB   4 {_firstGlob + 2}" +
        $"                                  [Ln 9, Col 7 - Ln 9, Col 7]\n" +
        $"  26   LT        1 4 -6           ; 8                 [Ln 9, Col 7 - Ln 9, Col 11]\n" +
        $"  27   JMP       0 8              ; to 36             [Ln 9, Col 7 - Ln 9, Col 11]\n" +
        $"  28   GETGLOB   4 {_firstGlob + 3}" +
        $"                                 [Ln 9, Col 17 - Ln 9, Col 17]\n" +
        $"  29   LT        1 4 -7           ; 3                 [Ln 9, Col 17 - Ln 9, Col 21]\n" +
        $"  30   JMP       0 5              ; to 36             [Ln 9, Col 17 - Ln 9, Col 21]\n" +
        $"  31   VISNOTIFY 0 0                                  [Ln 11, Col 0 - Ln 11, Col 0]\n" +
        $"  32   GETGLOB   4 {_firstGlob}" +
        $"                                  [Ln 11, Col 12 - Ln 11, Col 14]\n" +
        $"  33   GETGLOB   5 {_firstGlob + 1}" +
        $"                                  [Ln 11, Col 16 - Ln 11, Col 18]\n" +
        $"  34   CALL      4 1 0                                [Ln 11, Col 12 - Ln 11, Col 19]\n" +
        $"  35   SETGLOB   4 {_firstGlob + 1}" +
        $"                                  [Ln 11, Col 6 - Ln 11, Col 19]\n" +
        $"  36   VISNOTIFY 0 0                                  [Ln 8, Col 0 - Ln 8, Col 0]\n" +
        $"  37   FORLOOP   1 -16            ; to 22             [Ln 8, Col 2 - Ln 11, Col 19]\n" +
        $"  38   VISNOTIFY 0 0                                  [Ln 12, Col 0 - Ln 12, Col 0]\n" +
        $"  39   GETGLOB   5 {_firstGlob + 2}" +
        $"                                  [Ln 12, Col 2 - Ln 12, Col 2]\n" +
        $"  40   ADD       4 5 -5           ; 1                 [Ln 12, Col 2 - Ln 12, Col 7]\n" +
        $"  41   SETGLOB   4 {_firstGlob + 2}" +
        $"                                  [Ln 12, Col 2 - Ln 12, Col 7]\n" +
        $"  42   JMP       0 -33            ; to 10             [Ln 7, Col 0 - Ln 12, Col 7]\n" +
        $"  43   VISNOTIFY 0 0                                  [Ln 14, Col 0 - Ln 14, Col 0]\n" +
        $"  44   GETGLOB   4 4                                  [Ln 14, Col 0 - Ln 14, Col 4]\n" +
        $"  45   GETGLOB   5 {_firstGlob + 1}" +
        $"                                  [Ln 14, Col 6 - Ln 14, Col 8]\n" +
        $"  46   CALL      4 1 0                                [Ln 14, Col 0 - Ln 14, Col 9]\n" +
        $"  47   HALT      1 0                                  [Ln 14, Col 0 - Ln 14, Col 9]\n" +
        $"Notifications\n" +
        $"  0    Notification.SingleStep\n" +
        $"\n" +
        $"Function <inc>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  2    ADD       1 0 -1           ; 2                 [Ln 3, Col 9 - Ln 3, Col 13]\n" +
        $"  3    RETURN    1 1                                  [Ln 3, Col 2 - Ln 3, Col 13]\n" +
        $"  4    RETURN    0 0                                  [Ln 3, Col 2 - Ln 3, Col 13]\n" +
        $"Notifications\n" +
        $"  0    Notification.SingleStep\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.SingleStep) }, RunMode.Script);
    }

    [Fact]
    public void TestSingleStepWithJoiningLine() {
      var source = @"
x = 0
flag = \
  x < \
  5
# [[ Reset ]]
flag = \
  True
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 2, Col 0]\n" +
        $"  2    LOADK     0 -1             ; 0                 [Ln 2, Col 4 - Ln 2, Col 4]\n" +
        $"  3    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 2, Col 4]\n" +
        $"  4    VISNOTIFY 0 0                                  [Ln 4, Col 0 - Ln 4, Col 0]\n" +
        $"  5    GETGLOB   1 {_firstGlob}" +
        $"                                  [Ln 4, Col 2 - Ln 4, Col 2]\n" +
        $"  6    LT        1 1 -2           ; 5                 [Ln 4, Col 2 - Ln 5, Col 2]\n" +
        $"  7    JMP       0 1              ; to 9              [Ln 4, Col 2 - Ln 5, Col 2]\n" +
        $"  8    LOADBOOL  0 1 1                                [Ln 4, Col 2 - Ln 5, Col 2]\n" +
        $"  9    LOADBOOL  0 0 0                                [Ln 4, Col 2 - Ln 5, Col 2]\n" +
        $"  10   VISNOTIFY 0 0                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  11   SETGLOB   0 {_firstGlob + 1}" +
        $"                                  [Ln 3, Col 0 - Ln 5, Col 2]\n" +
        $"  12   VISNOTIFY 0 1                                  [Ln 6, Col 0 - Ln 8, Col 5]\n" +
        $"  13   VISNOTIFY 0 0                                  [Ln 8, Col 0 - Ln 8, Col 0]\n" +
        $"  14   LOADBOOL  0 1 0                                [Ln 8, Col 2 - Ln 8, Col 5]\n" +
        $"  15   VISNOTIFY 0 0                                  [Ln 7, Col 0 - Ln 7, Col 0]\n" +
        $"  16   SETGLOB   0 {_firstGlob + 1}" +
        $"                                  [Ln 7, Col 0 - Ln 8, Col 5]\n" +
        $"  17   VISNOTIFY 1 1                                  [Ln 6, Col 0 - Ln 8, Col 5]\n" +
        $"  18   HALT      1 0                                  [Ln 7, Col 0 - Ln 8, Col 5]\n" +
        $"Notifications\n" +
        $"  0    Notification.SingleStep\n" +
        $"  1    Notification.VTag: Reset\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.SingleStep),
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      }, RunMode.Script);
    }

    [Fact]
    public void TestSingleStepVTagWithParameter() {
      var source = @"
# [[ Assign(x) ]]
x = 1
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 3, Col 4]\n" +
        $"  2    VISNOTIFY 0 1                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  3    LOADK     0 -1             ; 1                 [Ln 3, Col 4 - Ln 3, Col 4]\n" +
        $"  4    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  5    GETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 12 - Ln 2, Col 12]\n" +
        $"  6    VISNOTIFY 1 2                                  [Ln 2, Col 0 - Ln 3, Col 4]\n" +
        $"  7    HALT      1 0                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"Notifications\n" +
        $"  0    Notification.VTag: Assign(x: null)\n" +
        $"  1    Notification.SingleStep\n" +
        $"  2    Notification.VTag: Assign(x: 0)\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.SingleStep),
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      }, RunMode.Script);
    }

    [Fact]
    public void TestSubscriptAssignTempArray() {
      string source = "[1, 2][1] = 1";
      string expected = (
        $"Function <main>\n" +
        $"  1    LOADK     1 -1             ; 1                 [Ln 1, Col 1 - Ln 1, Col 1]\n" +
        $"  2    LOADK     2 -2             ; 2                 [Ln 1, Col 4 - Ln 1, Col 4]\n" +
        $"  3    NEWLIST   0 1 2                                [Ln 1, Col 0 - Ln 1, Col 5]\n" +
        $"  4    SETELEM   0 -1 -1          ; 1 1               [Ln 1, Col 0 - Ln 1, Col 12]\n" +
        $"  5    VISNOTIFY 0 0                                  [Ln 1, Col 0 - Ln 1, Col 12]\n" +
        $"  6    HALT      1 0                                  [Ln 1, Col 0 - Ln 1, Col 12]\n" +
        $"Notifications\n" +
        $"  0    Notification.SubscriptAssignment: 0 250 250\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Assignment) }, RunMode.Interactive);
    }

    [Fact]
    public void TestSubscriptAssignGlobalArray() {
      string source = @"
a = [1, 2]
a[1] = 1
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 2, Col 0]\n" +
        $"  2    LOADK     1 -1             ; 1                 [Ln 2, Col 5 - Ln 2, Col 5]\n" +
        $"  3    LOADK     2 -2             ; 2                 [Ln 2, Col 8 - Ln 2, Col 8]\n" +
        $"  4    NEWLIST   0 1 2                                [Ln 2, Col 4 - Ln 2, Col 9]\n" +
        $"  5    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 2, Col 9]\n" +
        $"  6    VISNOTIFY 0 1                                  [Ln 2, Col 0 - Ln 2, Col 9]\n" +
        $"  7    GETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  8    VISNOTIFY 0 2                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  9    SETELEM   0 -1 -1          ; 1 1               [Ln 3, Col 0 - Ln 3, Col 7]\n" +
        $"  10   VISNOTIFY 0 3                                  [Ln 3, Col 0 - Ln 3, Col 7]\n" +
        $"  11   HALT      1 0                                  [Ln 3, Col 0 - Ln 3, Col 7]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'a' Global 7\n" +
        $"  1    Notification.Assignment: 'a' Global 0\n" +
        $"  2    Notification.GlobalLoaded: 0 'a'\n" +
        $"  3    Notification.SubscriptAssignment: 0 250 250\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Assignment) }, RunMode.Interactive);
    }

    [Fact]
    public void TestSubscriptAssignLocalArray() {
      string source = @"
def func():
  a = [1, 2]
  a[1] = 1
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 4, Col 9]\n" +
        $"  2    LOADK     0 -1             ; Func <func>       [Ln 2, Col 0 - Ln 4, Col 9]\n" +
        $"  3    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 4, Col 9]\n" +
        $"  4    HALT      1 0                                  [Ln 4, Col 2 - Ln 4, Col 9]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'func' Global 7\n" +
        $"\n" +
        $"Function <func>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 3, Col 2 - Ln 3, Col 2]\n" +
        $"  2    LOADK     1 -1             ; 1                 [Ln 3, Col 7 - Ln 3, Col 7]\n" +
        $"  3    LOADK     2 -2             ; 2                 [Ln 3, Col 10 - Ln 3, Col 10]\n" +
        $"  4    NEWLIST   0 1 2                                [Ln 3, Col 6 - Ln 3, Col 11]\n" +
        $"  5    VISNOTIFY 0 1                                  [Ln 3, Col 2 - Ln 3, Col 11]\n" +
        $"  6    SETELEM   0 -1 -1          ; 1 1               [Ln 4, Col 2 - Ln 4, Col 9]\n" +
        $"  7    VISNOTIFY 0 2                                  [Ln 4, Col 2 - Ln 4, Col 9]\n" +
        $"  8    RETURN    0 0                                  [Ln 4, Col 2 - Ln 4, Col 9]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'func.a' Local 0\n" +
        $"  1    Notification.Assignment: 'func.a' Local 0\n" +
        $"  2    Notification.SubscriptAssignment: 0 250 250\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Assignment) }, RunMode.Interactive);
    }

    [Fact]
    public void TestSubscriptAssignMultipleKeys() {
      string source = @"
a = [[1, 2], 3]
a[0][1] = 1
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 2, Col 0]\n" +
        $"  2    LOADK     2 -1             ; 1                 [Ln 2, Col 6 - Ln 2, Col 6]\n" +
        $"  3    LOADK     3 -2             ; 2                 [Ln 2, Col 9 - Ln 2, Col 9]\n" +
        $"  4    NEWLIST   1 2 2                                [Ln 2, Col 5 - Ln 2, Col 10]\n" +
        $"  5    LOADK     2 -3             ; 3                 [Ln 2, Col 13 - Ln 2, Col 13]\n" +
        $"  6    NEWLIST   0 1 2                                [Ln 2, Col 4 - Ln 2, Col 14]\n" +
        $"  7    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 2, Col 14]\n" +
        $"  8    VISNOTIFY 0 1                                  [Ln 2, Col 0 - Ln 2, Col 14]\n" +
        $"  9    GETGLOB   1 {_firstGlob}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  10   VISNOTIFY 0 2                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  11   GETELEM   0 1 -4           ; 0                 [Ln 3, Col 0 - Ln 3, Col 3]\n" +
        $"  12   VISNOTIFY 0 3                                  [Ln 3, Col 0 - Ln 3, Col 3]\n" +
        $"  13   SETELEM   0 -1 -1          ; 1 1               [Ln 3, Col 0 - Ln 3, Col 10]\n" +
        $"  14   VISNOTIFY 0 4                                  [Ln 3, Col 0 - Ln 3, Col 10]\n" +
        $"  15   HALT      1 0                                  [Ln 3, Col 0 - Ln 3, Col 10]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'a' Global 7\n" +
        $"  1    Notification.Assignment: 'a' Global 0\n" +
        $"  2    Notification.GlobalLoaded: 1 'a'\n" +
        $"  3    Notification.ElementLoaded: 0 1 253\n" +
        $"  4    Notification.SubscriptAssignment: 0 250 250\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Assignment) }, RunMode.Interactive);
    }

    [Fact]
    public void TestUnary() {
      string source = "-1";
      string expected = (
        $"Function <main>\n" +
        $"  1    GETGLOB   0 {_printValFunc}" +
        $"                                  [Ln 1, Col 0 - Ln 1, Col 1]\n" +
        $"  2    UNM       1 -1             ; 1                 [Ln 1, Col 0 - Ln 1, Col 1]\n" +
        $"  3    VISNOTIFY 0 0                                  [Ln 1, Col 0 - Ln 1, Col 1]\n" +
        $"  4    CALL      0 1 0                                [Ln 1, Col 0 - Ln 1, Col 1]\n" +
        $"  5    HALT      1 0                                  [Ln 1, Col 0 - Ln 1, Col 1]\n" +
        $"Notifications\n" +
        $"  0    Notification.Unary: Negative 250 1\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] { typeof(Event.Unary) }, RunMode.Interactive);
    }

    [Fact]
    public void TestVariables() {
      string source = @"
def add(a, b):
  c = a + b
  return c

x = add(1, 2)
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 1                                  [Ln 2, Col 0 - Ln 2, Col 0]\n" +
        $"  2    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 4, Col 9]\n" +
        $"  3    LOADK     0 -1             ; Func <add>        [Ln 2, Col 0 - Ln 4, Col 9]\n" +
        $"  4    SETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 0 - Ln 4, Col 9]\n" +
        $"  5    VISNOTIFY 0 1                                  [Ln 6, Col 0 - Ln 6, Col 0]\n" +
        $"  6    VISNOTIFY 0 2                                  [Ln 6, Col 0 - Ln 6, Col 0]\n" +
        $"  7    GETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 6, Col 4 - Ln 6, Col 6]\n" +
        $"  8    LOADK     1 -2             ; 1                 [Ln 6, Col 8 - Ln 6, Col 8]\n" +
        $"  9    LOADK     2 -3             ; 2                 [Ln 6, Col 11 - Ln 6, Col 11]\n" +
        $"  10   CALL      0 2 0                                [Ln 6, Col 4 - Ln 6, Col 12]\n" +
        $"  11   VISNOTIFY 0 3                                  [Ln 6, Col 4 - Ln 6, Col 12]\n" +
        $"  12   SETGLOB   0 {_firstGlob + 1}" +
        $"                                  [Ln 6, Col 0 - Ln 6, Col 12]\n" +
        $"  13   HALT      1 0                                  [Ln 6, Col 0 - Ln 6, Col 12]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'add' Global {_firstGlob}\n" +
        $"  1    Notification.SingleStep\n" +
        $"  2    Notification.VariableDefined: 'x' Global {_firstGlob + 1}\n" +
        $"  3    Notification.VariableDeleted: 0\n" +
        $"\n" +
        $"Function <add>\n" +
        $"  1    VISNOTIFY 0 1                                  [Ln 2, Col 0 - Ln 2, Col 0]\n" +
        $"  2    VISNOTIFY 0 0                                  [Ln 2, Col 8 - Ln 2, Col 8]\n" +
        $"  3    VISNOTIFY 0 2                                  [Ln 2, Col 11 - Ln 2, Col 11]\n" +
        $"  4    VISNOTIFY 0 1                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  5    VISNOTIFY 0 3                                  [Ln 3, Col 2 - Ln 3, Col 2]\n" +
        $"  6    ADD       2 0 1                                [Ln 3, Col 6 - Ln 3, Col 10]\n" +
        $"  7    VISNOTIFY 0 1                                  [Ln 4, Col 0 - Ln 4, Col 0]\n" +
        $"  8    RETURN    2 1                                  [Ln 4, Col 2 - Ln 4, Col 9]\n" +
        $"  9    RETURN    0 0                                  [Ln 4, Col 2 - Ln 4, Col 9]\n" +
        $"Notifications\n" +
        $"  0    Notification.VariableDefined: 'add.a' Local 0\n" +
        $"  1    Notification.SingleStep\n" +
        $"  2    Notification.VariableDefined: 'add.b' Local 1\n" +
        $"  3    Notification.VariableDefined: 'add.c' Local 2\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.SingleStep),
        typeof(Event.VariableDefined),
        typeof(Event.VariableDeleted),
      }, RunMode.Interactive);
    }

    [Fact]
    public void TestVTag() {
      var source = @"
# [[ Add ]]
1 + 2
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 3, Col 4]\n" +
        $"  2    GETGLOB   0 {_printValFunc}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  3    ADD       1 -1 -2          ; 1 2               [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  4    VISNOTIFY 0 1                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  5    CALL      0 1 0                                [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  6    VISNOTIFY 1 0                                  [Ln 2, Col 0 - Ln 3, Col 4]\n" +
        $"  7    HALT      1 0                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"Notifications\n" +
        $"  0    Notification.VTag: Add\n" +
        $"  1    Notification.Binary: 250 Add 251 1\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.Binary),
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      }, RunMode.Interactive);
    }

    [Fact]
    public void TestVTagWithArguments() {
      var source = @"
# [[ Add(1, 2) ]]
1 + 2
";
      string expected = (
        $"Function <main>\n" +
        $"  1    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 3, Col 4]\n" +
        $"  2    GETGLOB   0 {_printValFunc}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  3    ADD       1 -1 -2          ; 1 2               [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  4    VISNOTIFY 0 1                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  5    CALL      0 1 0                                [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"  6    VISNOTIFY 1 0                                  [Ln 2, Col 0 - Ln 3, Col 4]\n" +
        $"  7    HALT      1 0                                  [Ln 3, Col 0 - Ln 3, Col 4]\n" +
        $"Notifications\n" +
        $"  0    Notification.VTag: Add(1: 250, 2: 251)\n" +
        $"  1    Notification.Binary: 250 Add 251 1\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.Binary),
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      }, RunMode.Interactive);
    }

    [Fact]
    public void TestVTagWithComplexArguments() {
      var source = @"
# [[ Assign(x, 1, y, 1 + 2)
x, y = 1, 1 + 2
# ]]
";
      string expected = (
        $"Function <main>\n" +
        $"  1    ADD       2 -1 -2          ; 1 2               [Ln 2, Col 21 - Ln 2, Col 25]\n" +
        $"  2    VISNOTIFY 0 0                                  [Ln 2, Col 0 - Ln 4, Col 3]\n" +
        $"  3    VISNOTIFY 0 1                                  [Ln 3, Col 0 - Ln 3, Col 0]\n" +
        $"  4    VISNOTIFY 0 2                                  [Ln 3, Col 3 - Ln 3, Col 3]\n" +
        $"  5    LOADK     1 -1             ; 1                 [Ln 3, Col 7 - Ln 3, Col 7]\n" +
        $"  6    ADD       2 -1 -2          ; 1 2               [Ln 3, Col 10 - Ln 3, Col 14]\n" +
        $"  7    VISNOTIFY 0 3                                  [Ln 3, Col 10 - Ln 3, Col 14]\n" +
        $"  8    NEWTUPLE  0 1 2                                [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  9    LOADK     2 -3             ; 0                 [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  10   GETELEM   1 0 2                                [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  11   SETGLOB   1 {_firstGlob}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  12   VISNOTIFY 0 4                                  [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  13   LOADK     2 -1             ; 1                 [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  14   GETELEM   1 0 2                                [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  15   SETGLOB   1 {_firstGlob + 1}" +
        $"                                  [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  16   VISNOTIFY 0 5                                  [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"  17   GETGLOB   0 {_firstGlob}" +
        $"                                  [Ln 2, Col 12 - Ln 2, Col 12]\n" +
        $"  18   GETGLOB   1 {_firstGlob + 1}" +
        $"                                  [Ln 2, Col 18 - Ln 2, Col 18]\n" +
        $"  19   ADD       2 -1 -2          ; 1 2               [Ln 2, Col 21 - Ln 2, Col 25]\n" +
        $"  20   VISNOTIFY 1 6                                  [Ln 2, Col 0 - Ln 4, Col 3]\n" +
        $"  21   HALT      1 0                                  [Ln 3, Col 0 - Ln 3, Col 14]\n" +
        $"Notifications\n" +
        $"  0    Notification.VTag: Assign(x: null, 1: 250, y: null, 1+2: 2)\n" +
        $"  1    Notification.VariableDefined: 'x' Global {_firstGlob}\n" +
        $"  2    Notification.VariableDefined: 'y' Global {_firstGlob + 1}\n" +
        $"  3    Notification.Binary: 250 Add 251 2\n" +
        $"  4    Notification.Assignment: 'x' Global 1\n" +
        $"  5    Notification.Assignment: 'y' Global 1\n" +
        $"  6    Notification.VTag: Assign(x: 0, 1: 250, y: 1, 1+2: 2)\n"
      ).Replace("\n", Environment.NewLine);
      TestCompiler(source, expected, new Type[] {
        typeof(Event.Assignment),
        typeof(Event.Binary),
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      }, RunMode.Interactive);
    }

    private static void TestCompiler(string source, string expected, IReadOnlyList<Type> eventTypes,
                                     RunMode mode) {
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _).Should().Be(true);
      var env = new GlobalEnvironment(NativeFunctions.Funcs.Values);
      var visualizerCenter = new VisualizerCenter(() => null);
      var visualizerHelper = new VisualizerHelper(eventTypes);
      visualizerHelper.RegisterToVisualizerCenter(visualizerCenter);
      var compiler = new Compiler();
      var func = compiler.Compile(program, env, visualizerCenter, mode);
      Assert.Equal(expected, new Disassembler(func).ToString());
    }

    private static int NativeFunctionIdOf(string name) {
      return NativeFunctions.Funcs.Values.ToList().FindIndex(func => { return func.Name == name; });
    }
  }
}
