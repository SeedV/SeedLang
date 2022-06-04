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
using System.IO;
using FluentAssertions;
using SeedLang.Common;
using SeedLang.Visualization;
using Xunit;

namespace SeedLang.Tests {
  public class PythonVisualizationTests {
    private class MockupVisualizer : IVisualizer<Event.SingleStep> {
      public Event.SingleStep Event { get; set; }

      public void On(Event.SingleStep e, IVM vm) {
        Event = e;
        vm.Pause();
      }
    }

    [Fact]
    public void TestRunningStates() {
      var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Interactive);
      var stringWriter = new StringWriter();
      engine.RedirectStdout(stringWriter);
      var visualizer = new MockupVisualizer();
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
    }
  }
}
