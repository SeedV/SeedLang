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
using SeedLang.Common;
using SeedLang.Visualization;
using Xunit;

namespace SeedLang.Tests {
  public class EngineTests {
    private class MockupStateVisualizer : IVisualizer<Event.SingleStep> {
      public bool StopOnNextLine;
      public Event.SingleStep Event;

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
      public int Line;
      public IReadOnlyList<IVM.VariableInfo> Globals;
      public IReadOnlyList<IVM.VariableInfo> Locals;
      public Value Result;

      public void On(Event.SingleStep e, IVM vm) {
        Line = e.Range.Start.Line;
        vm.GetGlobals(out Globals).Should().Be(true);
        vm.GetLocals(out Locals).Should().Be(true);
        if (IsVariableDefined("a") && IsVariableDefined("b")) {
          vm.Eval("a + b", out Result).Should().Be(true);
        }
        vm.Pause();
      }

      private bool IsVariableDefined(string name) {
        try {
          _ = Locals.First((info) => info.Name == name);
          return true;
        } catch (Exception) {
          try {
            _ = Globals.First((info) => info.Name == name);
            return true;
          } catch (Exception) {
            return false;
          }
        }
      }
    }

    [Fact]
    public void TestGlobals() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script) {
        IsVariableTrackingEnabled = true
      };
      var visualizer = new MockupVariableVisualizer();
      engine.Register(visualizer);
      string source = @"
a = 1
b = 2
print(a + b)
";
      engine.Compile(source, "");
      engine.Run();
      visualizer.Line.Should().Be(2);
      visualizer.Globals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());

      engine.Continue();
      visualizer.Line.Should().Be(3);
      visualizer.Globals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("a", new Value(1)),
      });

      engine.Continue();
      visualizer.Line.Should().Be(4);
      visualizer.Globals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("a", new Value(1)),
        new IVM.VariableInfo("b", new Value(2)),
      });
      visualizer.Result.IsNumber.Should().Be(true);
      visualizer.Result.AsNumber().Should().Be(3);
    }

    [Fact]
    public void TestLocals() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script) {
        IsVariableTrackingEnabled = true
      };
      var visualizer = new MockupVariableVisualizer();
      engine.Register(visualizer);
      string source = @"
def add(a, b):
  c = a + b
  return c

add(1, 2)
";
      engine.Compile(source, "");
      engine.Run();
      visualizer.Line.Should().Be(2);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());

      engine.Continue();
      visualizer.Line.Should().Be(6);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());

      engine.Continue();
      visualizer.Line.Should().Be(2);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>());

      engine.Continue();
      visualizer.Line.Should().Be(3);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("a", new Value(1)),
        new IVM.VariableInfo("b", new Value(2)),
      });
      visualizer.Result.IsNumber.Should().Be(true);
      visualizer.Result.AsNumber().Should().Be(3);

      engine.Continue();
      visualizer.Line.Should().Be(4);
      visualizer.Locals.Should().BeEquivalentTo(new List<IVM.VariableInfo>() {
        new IVM.VariableInfo("a", new Value(1)),
        new IVM.VariableInfo("b", new Value(2)),
        new IVM.VariableInfo("c", new Value(3)),
      });
    }

    [Fact]
    public void TestRunningStates() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
      var stringWriter = new StringWriter();
      engine.RedirectStdout(stringWriter);
      var visualizer = new MockupStateVisualizer();
      engine.Register(visualizer);
      string source = @"
a = 1
b = 2
print(a + b)
";
      engine.Compile(source, "");
      engine.IsStopped.Should().Be(true);
      engine.Run().Should().Be(true);
      engine.IsPaused.Should().Be(true);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(2, 0, 2, 0)));
      engine.Continue().Should().Be(true);
      engine.IsPaused.Should().Be(true);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(3, 0, 3, 0)));
      engine.Continue().Should().Be(true);
      engine.IsPaused.Should().Be(true);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(4, 0, 4, 0)));
      engine.Continue().Should().Be(true);
      engine.IsStopped.Should().Be(true);
      stringWriter.ToString().Should().Be("3" + Environment.NewLine);

      engine.Continue().Should().Be(false);

      stringWriter = new StringWriter();
      engine.RedirectStdout(stringWriter);
      engine.Run().Should().Be(true);
      engine.IsPaused.Should().Be(true);
      visualizer.Event.Should().BeEquivalentTo(new Event.SingleStep(new TextRange(2, 0, 2, 0)));
      engine.Stop().Should().Be(true);
      engine.IsStopped.Should().Be(true);

      stringWriter = new StringWriter();
      engine.RedirectStdout(stringWriter);
      engine.Unregister(visualizer);
      engine.Run().Should().Be(true);
      engine.IsStopped.Should().Be(true);
      stringWriter.ToString().Should().Be("3" + Environment.NewLine);

      visualizer.StopOnNextLine = true;
      engine.Register(visualizer);
      engine.Run().Should().Be(true);
      engine.IsStopped.Should().Be(true);
    }
  }
}
