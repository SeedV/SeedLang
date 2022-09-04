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
using FluentAssertions;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class RandomDefinitionTests {
    [Fact]
    public void TestRandInt() {
      InitRandomWithSeed();

      var random = new Random(5);
      int expectedRandInt = random.Next(0, 10);

      var randIntFunc = RandomDefinition.RandIntFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(0), new VMValue(10) }, 0, 2);
      randIntFunc(args, null).Should().Be(new VMValue(expectedRandInt));

      args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      Action action = () => randIntFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestRandom() {
      InitRandomWithSeed();

      var random = new Random(5);
      double expectedRandom = random.NextDouble();

      var randomFunc = RandomDefinition.RandomFunc;
      var args = new ValueSpan(Array.Empty<VMValue>(), 0, 0);
      randomFunc(args, null).Should().Be(new VMValue(expectedRandom));

      args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      Action action = () => randomFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestRandRange() {
      InitRandomWithSeed();

      var random = new Random(5);
      double expectedRandom = random.Next(0, 10);

      var randRangeFunc = RandomDefinition.RandRangeFunc;
      var array = new VMValue[] { new VMValue(10) };
      var args = new ValueSpan(array, 0, array.Length);
      randRangeFunc(args, null).Should().Be(new VMValue(expectedRandom));

      expectedRandom = random.Next(2, 5);
      array = new VMValue[] { new VMValue(2), new VMValue(5) };
      args = new ValueSpan(array, 0, array.Length);
      randRangeFunc(args, null).Should().Be(new VMValue(expectedRandom));

      expectedRandom = random.Next(0, 5) * 2 + 1;
      array = new VMValue[] { new VMValue(1), new VMValue(10), new VMValue(2) };
      args = new ValueSpan(array, 0, array.Length);
      randRangeFunc(args, null).Should().Be(new VMValue(expectedRandom));

      args = new ValueSpan(Array.Empty<VMValue>(), 0, 0);
      Action action = () => randRangeFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSeed() {
      InitRandomWithSeed();

      var seedFunc = RandomDefinition.SeedFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => seedFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    private static void InitRandomWithSeed() {
      var seedFunc = RandomDefinition.SeedFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(5) }, 0, 1);
      seedFunc(args, null).Should().Be(new VMValue());
    }
  }
}
