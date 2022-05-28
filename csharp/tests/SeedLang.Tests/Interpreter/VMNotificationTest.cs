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
    public void TestSingleStepNotification() {
      string source = @"
# [[ Assign(a) ]]
a = 1
b = 2
";
      (string _, VisualizerHelper vh) = Run(source, new Type[] {
        typeof(Event.SingleStep),
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      });
      var expected = (
        "[Ln 3, Col 0 - Ln 3, Col 0] SingleStep\n" +
        "[Ln 4, Col 0 - Ln 4, Col 0] SingleStep\n" +
        "[Ln 2, Col 0 - Ln 3, Col 4] VTagEntered: Assign(a: None)\n" +
        "[Ln 2, Col 0 - Ln 3, Col 4] VTagExited: Assign(a: 1)\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, vh.EventsToString());
    }

    [Fact]
    public void TestVariableNotification() {
      string source = @"
def add(a, b):
  c = a + b
  return c

x = add(1, 2)
";
      (string _, VisualizerHelper vh) = Run(source, new Type[] {
        typeof(Event.VariableDefined),
        typeof(Event.VariableDeleted),
      });
      var expected = (
        "[Ln 2, Col 0 - Ln 4, Col 9] VariableDefined: global.add: Global\n" +
        "[Ln 6, Col 0 - Ln 6, Col 0] VariableDefined: global.x: Global\n" +
        "[Ln 2, Col 8 - Ln 2, Col 8] VariableDefined: global.add.a: Local\n" +
        "[Ln 2, Col 11 - Ln 2, Col 11] VariableDefined: global.add.b: Local\n" +
        "[Ln 3, Col 2 - Ln 3, Col 2] VariableDefined: global.add.c: Local\n" +
        "[Ln 4, Col 2 - Ln 4, Col 9] VariableDeleted: global.add.c: Local\n" +
        "[Ln 4, Col 2 - Ln 4, Col 9] VariableDeleted: global.add.b: Local\n" +
        "[Ln 4, Col 2 - Ln 4, Col 9] VariableDeleted: global.add.a: Local\n"
      ).Replace("\n", Environment.NewLine);
      Assert.Equal(expected, vh.EventsToString());
    }

    private static (string, VisualizerHelper) Run(string source, IReadOnlyList<Type> eventTypes) {
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _).Should().Be(true);
      var vm = new VM();
      var vh = new VisualizerHelper(eventTypes);
      vh.RegisterToVisualizerCenter(vm.VisualizerCenter);
      var stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vm.VisualizerCenter, RunMode.Interactive);
      vm.Run(func);
      return (stringWriter.ToString(), vh);
    }
  }
}
