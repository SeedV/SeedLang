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
