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
using SeedLang.Runtime;
using SeedLang.Visualization;
using SeedLang.X;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMProxyTests {
    private class MockupStateVisualizer : IVisualizer<Event.SingleStep> {
      public bool StopOnNextLine { get; set; }
      public Event.SingleStep Event { get; private set; }

      public void On(Event.SingleStep e, IVM vm) {
        Event = e;
        if (StopOnNextLine) {
          vm.Stop();
          StopOnNextLine = false;
        } else {
          vm.Pause();
        }
      }
    }

    private class MockupVariableVisualizer : IVisualizer<Event.SingleStep> {
      public int Line { get; private set; }
      public IEnumerable<IVM.VariableInfo> Globals { get; private set; }
      public IEnumerable<IVM.VariableInfo> Locals { get; private set; }

      public void On(Event.SingleStep e, IVM vm) {
        Line = e.Range.Start.Line;
        Globals = vm.Globals;
        Locals = vm.Locals;
        vm.Pause();
      }
    }

    private class MockupLocalsVisualizer : IVisualizer<Event.SingleStep> {
      public void On(Event.SingleStep e, IVM vm) {
        switch (e.Range.Start.Line) {
          case 3: {
              var locals = Enumerable.ToDictionary(vm.Locals,
                                                    variable => variable.Name,
                                                    variable => variable.Value);
              var expected = new Dictionary<string, Value>() {
                ["add.a"] = new Value(1),
                ["add.b"] = new Value(2),
              };
              locals.Should().BeEquivalentTo(expected);
            }
            break;
          case 4: {
              var locals = Enumerable.ToDictionary(vm.Locals,
                                                    variable => variable.Name,
                                                    variable => variable.Value);
              var expected = new Dictionary<string, Value>() {
                ["add.a"] = new Value(1),
                ["add.b"] = new Value(2),
                ["add.c"] = new Value(3),
              };
              locals.Should().BeEquivalentTo(expected);
            }
            break;
        }
      }
    }

    [Fact]
    public void TestGlobals() {
      string source = @"
a = 1
b = 2
print(a + b)
";
      var visualizer = new MockupVariableVisualizer();
      Function func = Compile(source, visualizer, out VM vm, out StringWriter _);
      vm.Run(func);
      visualizer.Line.Should().Be(2);
      visualizer.Globals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());
      vm.Continue();
      visualizer.Line.Should().Be(3);
      visualizer.Globals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("a", new Value(1)),
      });
      vm.Continue();
      visualizer.Line.Should().Be(4);
      visualizer.Globals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("a", new Value(1)),
        new IVM.VariableInfo("b", new Value(2)),
      });
    }

    [Fact]
    public void TestLocals() {
      string source = @"
def add(a, b):
  c = a + b
  return c

add(1, 2)
";
      var visualizer = new MockupVariableVisualizer();
      Function func = Compile(source, visualizer, out VM vm, out StringWriter _);
      vm.Run(func);
      visualizer.Line.Should().Be(2);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());
      vm.Continue();
      visualizer.Line.Should().Be(6);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());
      vm.Continue();
      visualizer.Line.Should().Be(2);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());
      vm.Continue();
      visualizer.Line.Should().Be(3);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("add.a", new Value(1)),
        new IVM.VariableInfo("add.b", new Value(2)),
      });
      vm.Continue();
      visualizer.Line.Should().Be(4);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("add.a", new Value(1)),
        new IVM.VariableInfo("add.b", new Value(2)),
        new IVM.VariableInfo("add.c", new Value(3)),
      });
    }

    [Fact]
    public void TestState() {
      string source = @"
a = 1
b = 2
print(a + b)
";
      var visualizer = new MockupStateVisualizer();
      Function func = Compile(source, visualizer, out VM vm, out StringWriter stringWriter);
      vm.State.Should().Be(VMState.Ready);
      vm.Run(func);
      vm.State.Should().Be(VMState.Paused);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(2, 0, 2, 0)));
      vm.Continue();
      vm.State.Should().Be(VMState.Paused);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(3, 0, 3, 0)));
      vm.Continue();
      vm.State.Should().Be(VMState.Paused);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(4, 0, 4, 0)));
      vm.Continue();
      vm.State.Should().Be(VMState.Stopped);
      stringWriter.ToString().Should().Be("3" + Environment.NewLine);

      Action action = () => vm.Continue();
      action.Should().Throw<Exception>();
      action = () => vm.Pause();
      action.Should().Throw<Exception>();

      vm.Run(func);
      vm.State.Should().Be(VMState.Paused);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(2, 0, 2, 0)));
      vm.Stop();
      vm.State.Should().Be(VMState.Stopped);

      stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      vm.VisualizerCenter.Unregister(visualizer);
      vm.Run(func);
      vm.State.Should().Be(VMState.Stopped);
      stringWriter.ToString().Should().Be("3" + Environment.NewLine);

      visualizer.StopOnNextLine = true;
      vm.VisualizerCenter.Register(visualizer);
      vm.Run(func);
      vm.State.Should().Be(VMState.Stopped);
    }

    private static Function Compile<Event>(string source, IVisualizer<Event> visualizer,
                                           out VM vm, out StringWriter stringWriter) {
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _);
      vm = new VM();
      vm.VisualizerCenter.VariableTrackingEnabled = true;
      vm.VisualizerCenter.Register(visualizer);
      stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      return compiler.Compile(program, vm.Env, vm.VisualizerCenter, RunMode.Interactive);
    }
  }
}
