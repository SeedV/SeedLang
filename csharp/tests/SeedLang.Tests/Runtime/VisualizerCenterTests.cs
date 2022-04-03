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
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using BinaryVisualizerCenter = Tuple<VisualizerCenter, MockupBinaryVisualizer>;
  using MultipleVisualizerCenter = Tuple<VisualizerCenter,
                                         MockupBinaryVisualizer,
                                         MockupMultipleVisualizer>;

  internal class MockupValue : IValue {
    public bool IsNil => false;
    public bool IsBoolean => false;
    public bool IsNumber => true;
    public bool IsString => false;
    public bool IsList => false;
    public bool IsFunction => false;

    public bool Boolean => false;
    public double Number => 0;
    public string String => "false";
    public int Length => 0;
    public IValue this[int index] => new ValueWrapper(new Value());
  }

  internal class MockupBinaryVisualizer : IVisualizer<BinaryEvent> {
    public BinaryEvent BinaryEvent { get; private set; }

    public void On(BinaryEvent be) {
      BinaryEvent = be;
    }
  }

  internal class MockupMultipleVisualizer : IVisualizer<BinaryEvent> {
    public BinaryEvent BinaryEvent { get; private set; }

    public void On(BinaryEvent be) {
      BinaryEvent = be;
    }
  }


  public class VisualizerCenterTests {
    [Fact]
    public void TestRegisterVisualizer() {
      (var visualizerCenter, var binaryVisualizer) = NewBinaryVisualizerCenter();
      Assert.Null(binaryVisualizer.BinaryEvent);
      visualizerCenter.Notify(NewBinaryEvent());
      Assert.NotNull(binaryVisualizer.BinaryEvent);
    }

    [Fact]
    public void TestRegisterMultipleVisualizers() {
      (var visualizerCenter, var binaryVisualizer, var multipleVisualizer) =
          NewMultipleVisualizerCenter();
      Assert.Null(binaryVisualizer.BinaryEvent);
      Assert.Null(multipleVisualizer.BinaryEvent);
      visualizerCenter.Notify(NewBinaryEvent());
      Assert.NotNull(binaryVisualizer.BinaryEvent);
      Assert.NotNull(multipleVisualizer.BinaryEvent);
    }

    [Fact]
    public void TestUnregisterVisualizer() {
      (var visualizerCenter, var binaryVisualizer) = NewBinaryVisualizerCenter();
      visualizerCenter.Unregister(binaryVisualizer);
      visualizerCenter.Notify(NewBinaryEvent());
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
  }
}
