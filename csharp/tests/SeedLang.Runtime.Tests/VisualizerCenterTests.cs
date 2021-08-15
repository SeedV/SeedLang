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
    public void TestSubscribeVisualizer() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Subscribe(binaryVisualizer);
      visualizerCenter.EvalEvent.Notify(new EvalEvent(new MockupValue()));
      Assert.False(binaryVisualizer.Notified);
      visualizerCenter.BinaryEvent.Notify(new BinaryEvent(new MockupValue(),
                                                          new MockupValue(),
                                                          new MockupValue()));
      Assert.True(binaryVisualizer.Notified);
    }

    [Fact]
    public void TestSubscribeMultipleVisualizers() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var multipleVisualizer = new MockupMultipleVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Subscribe(binaryVisualizer);
      visualizerCenter.Subscribe(multipleVisualizer);
      visualizerCenter.BinaryEvent.Notify(new BinaryEvent(new MockupValue(),
                                                          new MockupValue(),
                                                          new MockupValue()));
      visualizerCenter.EvalEvent.Notify(new EvalEvent(new MockupValue()));
      Assert.True(binaryVisualizer.Notified);
      Assert.True(multipleVisualizer.BinaryEventNotified);
      Assert.True(multipleVisualizer.EvalEventNotified);
    }
  }
}
