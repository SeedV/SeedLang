// Copyright 2021 The Aha001 Team.
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

using Xunit;

namespace SeedLang.Runtime.Tests {
  public class VisualizerCenterTests {
    internal class MockupValue : IValue {
      public ValueType Type => ValueType.Number;

      public double ToNumber() {
        return 0;
      }
    }

    internal class MockupBinaryVisualizer : IVisualizer<BinaryEvent> {
      public bool Notified { get; private set; } = false;

      public void On(BinaryEvent e) {
        Notified = true;
      }
    }

    internal class MockupMultipleVisualizer : IVisualizer<BinaryEvent>, IVisualizer<EvalEvent> {
      public bool BinaryEventNotified { get; private set; } = false;
      public bool EvalEventNotified { get; private set; } = false;

      public void On(BinaryEvent e) {
        BinaryEventNotified = true;
      }

      public void On(EvalEvent e) {
        EvalEventNotified = true;
      }
    }

    [Fact]
    public void TestRegisterVisualizer() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      visualizerCenter.EvalPublisher.Notify(new EvalEvent(new MockupValue()));
      Assert.False(binaryVisualizer.Notified);
      var binaryEvent = new BinaryEvent(new MockupValue(), BinaryOperator.Add,
                                        new MockupValue(), new MockupValue());
      visualizerCenter.BinaryPublisher.Notify(binaryEvent);
      Assert.True(binaryVisualizer.Notified);
    }

    [Fact]
    public void TestRegisterMultipleVisualizers() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var multipleVisualizer = new MockupMultipleVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      visualizerCenter.Register(multipleVisualizer);
      Assert.False(binaryVisualizer.Notified);
      Assert.False(multipleVisualizer.BinaryEventNotified);
      Assert.False(multipleVisualizer.EvalEventNotified);
      var binaryEvent = new BinaryEvent(new MockupValue(), BinaryOperator.Add,
                                        new MockupValue(), new MockupValue());
      visualizerCenter.BinaryPublisher.Notify(binaryEvent);
      visualizerCenter.EvalPublisher.Notify(new EvalEvent(new MockupValue()));
      Assert.True(binaryVisualizer.Notified);
      Assert.True(multipleVisualizer.BinaryEventNotified);
      Assert.True(multipleVisualizer.EvalEventNotified);
    }

    [Fact]
    public void TestUnregisterVisualizer() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      visualizerCenter.Unregister(binaryVisualizer);
      var binaryEvent = new BinaryEvent(new MockupValue(), BinaryOperator.Add,
                                        new MockupValue(), new MockupValue());
      visualizerCenter.BinaryPublisher.Notify(binaryEvent);
      Assert.False(binaryVisualizer.Notified);
    }
  }
}
