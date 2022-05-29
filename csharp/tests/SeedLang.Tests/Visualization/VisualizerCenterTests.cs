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
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Visualization.Tests {
  internal class MockupBinaryVisualizer : IVisualizer<Event.Binary> {
    public Event.Binary BinaryEvent { get; private set; }

    public void On(Event.Binary be, IVM vm) {
      BinaryEvent = be;
    }
  }

  internal class AnotherMockupBinaryVisualizer : IVisualizer<Event.Binary> {
    public Event.Binary BinaryEvent { get; private set; }
    public void On(Event.Binary be, IVM vm) {
      BinaryEvent = be;
    }
  }

  internal class MockupVariableDefinedVisualizer : IVisualizer<Event.VariableDefined> {
    public Event.VariableDefined VariableDefined { get; private set; }

    public void On(Event.VariableDefined vde, IVM vm) {
      VariableDefined = vde;
    }
  }

  internal class MockupVariableDeletedVisualizer : IVisualizer<Event.VariableDeleted> {
    public Event.VariableDeleted VariableDeleted { get; private set; }

    public void On(Event.VariableDeleted vde, IVM vm) {
      VariableDeleted = vde;
    }
  }

  internal class MockupVM : IVM {
    public IEnumerable<IVM.VariableInfo> Globals => throw new NotImplementedException();

    public IEnumerable<IVM.VariableInfo> Locals => throw new NotImplementedException();

    public void Pause() {
      throw new NotImplementedException();
    }

    public void Stop() {
      throw new NotImplementedException();
    }
  }

  public class VisualizerCenterTests {
    [Fact]
    public void TestEnableVariableTracking() {
      var vc = new VisualizerCenter();
      vc.VariableTrackingEnabled.Should().Be(false);
      vc.VariableTrackingEnabled = true;
      vc.VariableTrackingEnabled.Should().Be(true);

      vc.VariableTrackingEnabled = false;
      vc.Register(new MockupVariableDefinedVisualizer());
      vc.VariableTrackingEnabled.Should().Be(true);
      vc.VariableTrackingEnabled = false;
      vc.Register(new MockupVariableDeletedVisualizer());
      vc.VariableTrackingEnabled.Should().Be(true);
    }

    [Fact]
    public void TestRegisterVisualizer() {
      (var visualizerCenter, var binaryVisualizer) = NewBinaryVisualizerCenter();
      Assert.Null(binaryVisualizer.BinaryEvent);
      visualizerCenter.Notify(NewBinaryEvent(), new MockupVM());
      Assert.NotNull(binaryVisualizer.BinaryEvent);
    }

    [Fact]
    public void TestRegisterMultipleVisualizers() {
      (var visualizerCenter, var binaryVisualizer, var multipleVisualizer) =
          NewMultipleVisualizerCenter();
      Assert.Null(binaryVisualizer.BinaryEvent);
      Assert.Null(multipleVisualizer.BinaryEvent);
      visualizerCenter.Notify(NewBinaryEvent(), new MockupVM());
      Assert.NotNull(binaryVisualizer.BinaryEvent);
      Assert.NotNull(multipleVisualizer.BinaryEvent);
    }

    [Fact]
    public void TestUnregisterVisualizer() {
      (var visualizerCenter, var binaryVisualizer) = NewBinaryVisualizerCenter();
      visualizerCenter.Unregister(binaryVisualizer);
      visualizerCenter.Notify(NewBinaryEvent(), new MockupVM());
      Assert.Null(binaryVisualizer.BinaryEvent);
    }

    private static (VisualizerCenter, MockupBinaryVisualizer) NewBinaryVisualizerCenter() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      return (visualizerCenter, binaryVisualizer);
    }

    private static (VisualizerCenter, MockupBinaryVisualizer, AnotherMockupBinaryVisualizer)
        NewMultipleVisualizerCenter() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var anotherBinaryVisualizer = new AnotherMockupBinaryVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      visualizerCenter.Register(anotherBinaryVisualizer);
      return (visualizerCenter, binaryVisualizer, anotherBinaryVisualizer);
    }

    private static Event.Binary NewBinaryEvent() {
      return new Event.Binary(new Value(1), BinaryOperator.Add, new Value(2), new Value(3),
                              new TextRange(0, 1, 2, 3));
    }
  }
}
