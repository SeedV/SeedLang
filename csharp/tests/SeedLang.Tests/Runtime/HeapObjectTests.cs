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
using System.Collections.Immutable;
using FluentAssertions;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using Range = HeapObjects.Range;
  using Slice = HeapObjects.Slice;

  public class HeapObjectTests {
    [Fact]
    public void TestRange() {
      var range = new HeapObject(new Range(10));
      range.Length.Should().Be(10);
      for (int i = 0; i < 10; i++) {
        range[new VMValue(i)].AsNumber().Should().Be(i);
      }

      int start = 3;
      int stop = 10;
      int step = 2;
      int length = 4;
      range = new HeapObject(new Range(start, stop, step));
      range.Length.Should().Be(length);
      for (int i = 0; i < length; i++) {
        range[new VMValue(i)].AsNumber().Should().Be(start + i * step);
      }

      start = 10;
      stop = 2;
      step = -3;
      length = 3;
      range = new HeapObject(new Range(start, stop, step));
      range.Length.Should().Be(length);
      for (int i = 0; i < length; i++) {
        range[new VMValue(i)].AsNumber().Should().Be(start + i * step);
      }

      start = 1;
      stop = 10;
      step = -1;
      range = new HeapObject(new Range(start, stop, step));
      range.Length.Should().Be(0);
    }

    [Fact]
    public void TestSlice() {
      new VMValue(new Slice()).ToString().Should().Be("slice(None, None, None)");
      new VMValue(new Slice(3)).ToString().Should().Be("slice(None, 3, None)");
      new VMValue(new Slice(1, 3)).ToString().Should().Be("slice(1, 3, None)");
      new VMValue(new Slice(1, 3, 2)).ToString().Should().Be("slice(1, 3, 2)");
    }

    [Fact]
    public void TestTuple() {
      var tuple = new HeapObject(ImmutableArray.Create<VMValue>());
      tuple.Length.Should().Be(0);
      tuple.AsString().Should().Be("()");

      tuple = new HeapObject(ImmutableArray.Create(new VMValue(1), new VMValue(2)));
      tuple.Length.Should().Be(2);
      tuple[new VMValue(0)].AsNumber().Should().Be(1);
      tuple[new VMValue(1)].AsNumber().Should().Be(2);
      tuple.AsString().Should().Be("(1, 2)");

      var anotherTuple = new HeapObject(ImmutableArray.Create(new VMValue(1), new VMValue(2)));
      tuple.Should().Be(anotherTuple);
      tuple.GetHashCode().Should().Be(anotherTuple.GetHashCode());

      Action action = () => tuple[new VMValue(1)] = new VMValue();
      action.Should().Throw<DiagnosticException>().Where(
          e => e.Diagnostic.MessageId == Message.RuntimeErrorNotSupportAssignment
      );
    }
  }
}
