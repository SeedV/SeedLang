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

using System;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using BinaryVisualizerCenter = Tuple<VisualizerCenter, MockupBinaryVisualizer>;
  using MultipleVisualizerCenter = Tuple<VisualizerCenter,
                                         MockupBinaryVisualizer,
                                         MockupMultipleVisualizer>;

  internal class MockupValue : IValue {
    public bool IsNone => false;
    public bool IsBoolean => false;
    public bool IsNumber => true;
    public bool IsString => false;

    public bool Boolean => false;
    public double Number => 0;
    public string String => "false";
  }

  internal class MockupBinaryVisualizer : IVisualizer<BinaryEvent> {
    public BinaryEvent BinaryEvent { get; private set; }

    public void On(BinaryEvent be) {
      BinaryEvent = be;
    }
  }

  internal class MockupMultipleVisualizer : IVisualizer<BinaryEvent>, IVisualizer<EvalEvent> {
    public BinaryEvent BinaryEvent { get; private set; }
    public EvalEvent EvalEvent { get; private set; }

    public void On(BinaryEvent be) {
      BinaryEvent = be;
    }

    public void On(EvalEvent ee) {
      EvalEvent = ee;
    }
  }


  public class VisualizerCenterTests {
    [Fact]
    public void TestRegisterVisualizer() {
      (var visualizerCenter, var binaryVisualizer) = NewBinaryVisualizerCenter();
      visualizerCenter.EvalPublisher.Notify(NewEvalEvent());
      Assert.Null(binaryVisualizer.BinaryEvent);
      visualizerCenter.BinaryPublisher.Notify(NewBinaryEvent());
      Assert.NotNull(binaryVisualizer.BinaryEvent);
    }

    [Fact]
    public void TestRegisterMultipleVisualizers() {
      (var visualizerCenter, var binaryVisualizer, var multipleVisualizer) =
          NewMultipleVisualizerCenter();
      Assert.Null(binaryVisualizer.BinaryEvent);
      Assert.Null(multipleVisualizer.BinaryEvent);
      Assert.Null(multipleVisualizer.EvalEvent);
      visualizerCenter.BinaryPublisher.Notify(NewBinaryEvent());
      visualizerCenter.EvalPublisher.Notify(NewEvalEvent());
      Assert.NotNull(binaryVisualizer.BinaryEvent);
      Assert.NotNull(multipleVisualizer.BinaryEvent);
      Assert.NotNull(multipleVisualizer.EvalEvent);
    }

    [Fact]
    public void TestUnregisterVisualizer() {
      (var visualizerCenter, var binaryVisualizer) = NewBinaryVisualizerCenter();
      visualizerCenter.Unregister(binaryVisualizer);
      visualizerCenter.BinaryPublisher.Notify(NewBinaryEvent());
      Assert.Null(binaryVisualizer.BinaryEvent);
    }

    private static BinaryVisualizerCenter NewBinaryVisualizerCenter() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      return new BinaryVisualizerCenter(visualizerCenter, binaryVisualizer);
    }

    private static MultipleVisualizerCenter NewMultipleVisualizerCenter() {
      var binaryVisualizer = new MockupBinaryVisualizer();
      var multipleVisualizer = new MockupMultipleVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(binaryVisualizer);
      visualizerCenter.Register(multipleVisualizer);
      return new MultipleVisualizerCenter(visualizerCenter, binaryVisualizer, multipleVisualizer);
    }

    private static BinaryEvent NewBinaryEvent() {
      return new BinaryEvent(new MockupValue(), BinaryOperator.Add, new MockupValue(),
                             new MockupValue(), new TextRange(0, 1, 2, 3));
    }

    private static EvalEvent NewEvalEvent() {
      return new EvalEvent(new MockupValue(), new TextRange(0, 1, 2, 3));
    }
  }
}
