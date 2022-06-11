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
using System.Linq;
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
        "[Ln 2, Col 0 - Ln 4, Col 9] VariableDefined: add: Global",
        "[Ln 6, Col 0 - Ln 6, Col 0] VariableDefined: x: Global",
        "[Ln 2, Col 8 - Ln 2, Col 8] VariableDefined: add.a: Local",
        "[Ln 2, Col 11 - Ln 2, Col 11] VariableDefined: add.b: Local",
        "[Ln 3, Col 2 - Ln 3, Col 2] VariableDefined: add.c: Local",
        "[Ln 6, Col 4 - Ln 6, Col 12] VariableDeleted: add.c: Local",
        "[Ln 6, Col 4 - Ln 6, Col 12] VariableDeleted: add.b: Local",
        "[Ln 6, Col 4 - Ln 6, Col 12] VariableDeleted: add.a: Local",
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
        typeof(Event.SubscriptAssignment),
        typeof(Event.VariableDefined),
        typeof(Event.VariableDeleted),
      });
      var expected = new string[] {
        "[Ln 3, Col 0 - Ln 3, Col 11] (a: Global)[0][1] = 10",
        "[Ln 2, Col 0 - Ln 2, Col 0] VariableDefined: a: Global",
      };
      events.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestQuickSort() {
      string source = @"
# [[ Data ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

# [[ Index(start, end) ]]
def partition(start, end, a):
  # [[ Index ]]
  pivot_index = start
  # [[ Save ]]
  pivot = a[pivot_index]
  while start < end:
    while start < len(a) and a[start] <= pivot:
      start += 1
    while a[end] > pivot:
      end -= 1
    if (start < end):
      # [[ Swap(start, end) ]]
      a[start], a[end] = a[end], a[start]
    # [[ Swap(end, pivot_index) ]]
    a[end], a[pivot_index] = a[pivot_index], a[end]
  return end


# [[ Bounds(start, end) ]]
def quick_sort(start, end, a):
  if start < end:
    mid = partition(start, end, a)
    quick_sort(start, mid - 1, a)
    quick_sort(mid + 1, end, a)


quick_sort(0, len(a) - 1, a)
print(a)
";
      (string output, IEnumerable<string> _) = Run(source, new Type[] {
        typeof(Event.VTagEntered),
        typeof(Event.VTagExited),
      });
      output.Should().Be("[0, 1, 1, 2, 3, 4, 5, 6, 7, 8]" + Environment.NewLine);
    }

    private static (string, IEnumerable<string>) Run(string source,
                                                     IReadOnlyList<Type> eventTypes) {
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _).Should().Be(true);
      var visualizerCenter = new VisualizerCenter(() => new VMProxy(null));
      var vm = new VM(visualizerCenter);
      var vh = new VisualizerHelper(eventTypes);
      vh.RegisterToVisualizerCenter(visualizerCenter);
      var stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, visualizerCenter, RunMode.Interactive);
      vm.Run(func);
      var events = vh.EventsToString().Split(Environment.NewLine).Where(str => str != string.Empty);
      return (stringWriter.ToString(), events);
    }
  }
}
