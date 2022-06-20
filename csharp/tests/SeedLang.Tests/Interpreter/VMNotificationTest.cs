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
using System.IO;
using FluentAssertions;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Tests.Helper;
using SeedLang.Visualization;
using SeedLang.X;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMNotificationTests {
    [Fact]
    public void TestAssignment() {
      string source = @"
a = 1
b = a
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] { typeof(Event.Assignment) });
      var expected = new string[] {
        "[Ln 2, Col 0 - Ln 2, Col 4] a:Global = 1",
        "[Ln 3, Col 0 - Ln 3, Col 4] b:Global = a:Global 1",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestAssignment1() {
      string source = @"
a, b = 1, 2
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] { typeof(Event.Assignment) });
      var expected = new string[] {
        "[Ln 2, Col 0 - Ln 2, Col 10] a:Global = 1",
        "[Ln 2, Col 0 - Ln 2, Col 10] b:Global = 2",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestAssignment2() {
      string source = @"
a = 1
a += 1
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] { typeof(Event.Assignment) });
      var expected = new string[] {
        "[Ln 2, Col 0 - Ln 2, Col 4] a:Global = 1",
        "[Ln 3, Col 0 - Ln 3, Col 5] a:Global = 2",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestAssignment3() {
      string source = @"
for i in range(3):
  pass
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] { typeof(Event.Assignment) });
      var expected = new string[] {
        "[Ln 2, Col 4 - Ln 2, Col 4] i:Global = 0",
        "[Ln 2, Col 4 - Ln 2, Col 4] i:Global = 1",
        "[Ln 2, Col 4 - Ln 2, Col 4] i:Global = 2",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestComparison() {
      string source = @"
str = 'string'
if str == 'string' and str != 'str' and 'i' in str:
  print(str)
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] { typeof(Event.Comparison) });
      var expected = new string[] {
        "[Ln 3, Col 3 - Ln 3, Col 17] str:Global 'string' EqEqual 'string' = True",
        "[Ln 3, Col 23 - Ln 3, Col 34] str:Global 'string' NotEqual 'str' = True",
        "[Ln 3, Col 40 - Ln 3, Col 49] 'i' In str:Global 'string' = True",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestMultipleListAssignment() {
      string source = @"
a = [[1, 2, 3], [1, 2]]
a[0][1] = 10
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] {
        typeof(Event.Assignment),
        typeof(Event.VariableDefined),
        typeof(Event.VariableDeleted),
      });
      var expected = new string[] {
        "[Ln 2, Col 0 - Ln 2, Col 22] a:Global = [[1, 2, 3], [1, 2]]",
        "[Ln 3, Col 0 - Ln 3, Col 11] a:Global[0][1] = 10",
        "[Ln 2, Col 0 - Ln 2, Col 0] VariableDefined: a (Global)",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestSingleStep() {
      string source = @"
# [[ Assign(a) ]]
a = 1
b = 2
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] {
            typeof(Event.SingleStep),
            typeof(Event.VTagEntered),
            typeof(Event.VTagExited),
          });
      var expected = new string[] {
        "[Ln 3, Col 0 - Ln 3, Col 0] SingleStep",
        "[Ln 4, Col 0 - Ln 4, Col 0] SingleStep",
        "[Ln 2, Col 0 - Ln 3, Col 4] VTagEntered: Assign(a: None)",
        "[Ln 2, Col 0 - Ln 3, Col 4] VTagExited: Assign(a: 1)",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestVariableNotification() {
      string source = @"
def add(a, b):
  c = a + b
  return c

x = add(1, 2)
";
      (string _, IEnumerable<string> events) = Run(source, new Type[] {
            typeof(Event.VariableDefined),
            typeof(Event.VariableDeleted),
          });
      var expected = new string[] {
        "[Ln 2, Col 0 - Ln 4, Col 9] VariableDefined: add (Global)",
        "[Ln 6, Col 0 - Ln 6, Col 0] VariableDefined: x (Global)",
        "[Ln 2, Col 8 - Ln 2, Col 8] VariableDefined: add.a (Local)",
        "[Ln 2, Col 11 - Ln 2, Col 11] VariableDefined: add.b (Local)",
        "[Ln 3, Col 2 - Ln 3, Col 2] VariableDefined: add.c (Local)",
        "[Ln 6, Col 4 - Ln 6, Col 12] VariableDeleted: add.c (Local)",
        "[Ln 6, Col 4 - Ln 6, Col 12] VariableDeleted: add.b (Local)",
        "[Ln 6, Col 4 - Ln 6, Col 12] VariableDeleted: add.a (Local)",
      };
      events.Should().BeEquivalentTo(expected);
    }

    private static (string, IEnumerable<string>) Run(string source,
                                                     IReadOnlyList<Type> eventTypes) {
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _).Should().Be(true);
      var vc = new VisualizerCenter(() => new VMProxy(SeedXLanguage.SeedPython, null));
      var vm = new VM(vc);
      var vh = new VisualizerHelper(eventTypes);
      vh.RegisterToVisualizerCenter(vc);
      var stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vc, RunMode.Interactive);
      vm.Run(func);
      return (stringWriter.ToString(), vh.EventStrings);
    }
  }
}
