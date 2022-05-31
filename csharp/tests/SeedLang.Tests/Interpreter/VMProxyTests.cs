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
      public Event.SingleStep Event { get; set; }

      public void On(Event.SingleStep e, IVM vm) {
        Event = e;
        vm.Pause();
      }
    }

    private class MockupGlobalsVisualizer : IVisualizer<Event.SingleStep> {
      public void On(Event.SingleStep e, IVM vm) {
        switch (e.Range.Start.Line) {
          case 3: {
              var globals = Enumerable.ToDictionary(vm.Globals,
                                                    variable => variable.Name,
                                                    variable => variable.Value);
              var expected = new Dictionary<string, Value>() { ["a"] = new Value(1), };
              globals.Should().BeEquivalentTo(expected);
            }
            break;
          case 4: {
              var globals = Enumerable.ToDictionary(vm.Globals,
                                                    variable => variable.Name,
                                                    variable => variable.Value);
              var expected = new Dictionary<string, Value>() {
                ["a"] = new Value(1),
                ["b"] = new Value(2),
              };
              globals.Should().BeEquivalentTo(expected);
            }
            break;
        }
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
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                               out IReadOnlyList<TokenInfo> _);
      var vm = new VM();
      vm.VisualizerCenter.VariableTrackingEnabled = true;
      vm.VisualizerCenter.Register(new MockupGlobalsVisualizer());
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vm.VisualizerCenter, RunMode.Interactive);
      vm.Run(func);
    }

    [Fact]
    public void TestLocals() {
      string source = @"
def add(a, b):
  c = a + b
  return c

add(1, 2)
";
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                               out IReadOnlyList<TokenInfo> _);
      var vm = new VM();
      vm.VisualizerCenter.VariableTrackingEnabled = true;
      vm.VisualizerCenter.Register(new MockupLocalsVisualizer());
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vm.VisualizerCenter, RunMode.Interactive);
      vm.Run(func);
    }

    [Fact]
    public void TestState() {
      string source = @"
a = 1
b = 2
print(a + b)
";
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _);
      var vm = new VM();
      var visualizer = new MockupStateVisualizer();
      vm.VisualizerCenter.Register(visualizer);
      var stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vm.VisualizerCenter, RunMode.Interactive);

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

      stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
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
    }
  }
}
