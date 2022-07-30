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
using System.Collections.Immutable;
using System.IO;
using FluentAssertions;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using Range = HeapObjects.Range;

  internal class MockupNativeContext : INativeContext {
    public TextWriter Stdout { get; } = new StringWriter();

    VMValue INativeContext.ModuleDir() {
      return new VMValue(new List<VMValue> { new VMValue("__builtins__") });
    }

    VMValue INativeContext.ModuleDir(VMValue value) {
      return new VMValue(new List<VMValue> { new VMValue("__builtins__") });
    }
  }

  public class BuiltinsDefinitionTests {
    [Fact]
    public void TestPrintValFunc() {
      var printValFunc = BuiltinsDefinition.PrintValFunc;
      var context = new MockupNativeContext();

      var args = new ValueSpan(new VMValue[] { new VMValue(), }, 0, 1);
      printValFunc(args, context).Should().Be(new VMValue());
      context.Stdout.ToString().Should().Be("");

      args = new ValueSpan(new VMValue[] { new VMValue(1), }, 0, 1);
      printValFunc(args, context).Should().Be(new VMValue());
      context.Stdout.ToString().Should().Be("1" + Environment.NewLine);

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), }, 0, 2);
      Action action = () => printValFunc(args, context);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAbsFunc() {
      var absFunc = BuiltinsDefinition.AbsFunc;

      var args = new ValueSpan(new VMValue[] { new VMValue(3) }, 0, 1);
      absFunc(args, null).Should().Be(new VMValue(3));
      args = new ValueSpan(new VMValue[] { new VMValue(-3) }, 0, 1);
      absFunc(args, null).Should().Be(new VMValue(3));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => absFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAllFunc() {
      var allFunc = BuiltinsDefinition.AllFunc;

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(true), new VMValue(false), new VMValue(true) })
      }, 0, 1);
      allFunc(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(true), new VMValue(true), new VMValue(true) })
      }, 0, 1);
      allFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(new List<VMValue>()) }, 0, 1);
      allFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => allFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAnyFunc() {
      var anyFunc = BuiltinsDefinition.AnyFunc;

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> {
          new VMValue(false),
          new VMValue(false),
          new VMValue(true)
        }),
      }, 0, 1);
      anyFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> {
          new VMValue(false),
          new VMValue(false),
          new VMValue(false)
        }),
      }, 0, 1);
      anyFunc(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] { new VMValue(new List<VMValue>()) }, 0, 1);
      anyFunc(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => anyFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAppendFunc() {
      var appendFunc = BuiltinsDefinition.AppendFunc;
      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
        new VMValue(3),
      }, 0, 2);
      appendFunc(args, null).Should().Be(new VMValue());
      args[0].Length.Should().Be(3);
      var expectedList = new List<VMValue> { new VMValue(1), new VMValue(2), new VMValue(3) };
      args[0].AsList().Should().BeEquivalentTo(expectedList);
      args = new ValueSpan(new VMValue[] { new VMValue(), }, 0, 1);
      Action action = () => appendFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestBoolFunc() {
      var boolFunc = BuiltinsDefinition.BoolFunc;

      var args = new ValueSpan(new VMValue[] { new VMValue() }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] { new VMValue(false) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] { new VMValue(true) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(0) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue("") }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] { new VMValue("test") }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(new List<VMValue> { }) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
      }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(ImmutableArray.Create<VMValue>()) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] {
        new VMValue(ImmutableArray.Create(new VMValue(1), new VMValue(2))),
      }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(new Dictionary<VMValue, VMValue>()) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] {
        new VMValue(new Dictionary<VMValue, VMValue>{[new VMValue(1)] = new VMValue("test")}),
      }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(new Range(0)) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(false));
      args = new ValueSpan(new VMValue[] { new VMValue(new Range(10)) }, 0, 1);
      boolFunc(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => boolFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestDirFunc() {
      var dirFunc = BuiltinsDefinition.DirFunc;

      var args = new ValueSpan(Array.Empty<VMValue>(), 0, 0);
      dirFunc(args, new MockupNativeContext()).Should().Be(new VMValue(
        new List<VMValue> { new VMValue("__builtins__") }
      ));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => dirFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLenFunc() {
      var lenFunc = BuiltinsDefinition.LenFunc;
      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
      }, 0, 1);
      lenFunc(args, null).Should().Be(new VMValue(2));
      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => lenFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestListFunc() {
      var listFunc = BuiltinsDefinition.ListFunc;

      VMValue list = listFunc(new ValueSpan(Array.Empty<VMValue>(), 0, 0), null);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(0);

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue>() { new VMValue(1), new VMValue(2) }),
      }, 0, 1);
      list = listFunc(args, null);
      list.IsList.Should().Be(true);
      var expectedList = new List<VMValue> { new VMValue(1), new VMValue(2) };
      list.AsList().Should().BeEquivalentTo(expectedList);

      int length = 10;
      args = new ValueSpan(new VMValue[] { new VMValue(new Range(length)) }, 0, 1);
      list = listFunc(args, null);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(length);
      for (int i = 0; i < length; i++) {
        list[new VMValue(i)].AsNumber().Should().Be(i);
      }

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => listFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestPrintFunc() {
      var context = new MockupNativeContext();
      var printFunc = BuiltinsDefinition.PrintFunc;
      var args = new ValueSpan(new VMValue[] {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3)
      }, 0, 3);
      printFunc(args, context);
      context.Stdout.ToString().Should().Be("1 2 3" + Environment.NewLine);
    }

    [Fact]
    public void TestRangFunc() {
      var rangeFunc = BuiltinsDefinition.RangeFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(10) }, 0, 1);
      rangeFunc(args, null).ToString().Should().Be("range(0, 10, 1)");
      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(10) }, 0, 2);
      rangeFunc(args, null).ToString().Should().Be("range(1, 10, 1)");
      args = new ValueSpan(new VMValue[] {
        new VMValue(1),
        new VMValue(10),
        new VMValue(2)
      }, 0, 3);
      rangeFunc(args, null).ToString().Should().Be("range(1, 10, 2)");

      Action action = () => rangeFunc(new ValueSpan(Array.Empty<VMValue>(), 0, 0), null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestRoundFunc() {
      var roundFunc = BuiltinsDefinition.RoundFunc;

      var args = new ValueSpan(new VMValue[] { new VMValue(1.2) }, 0, 1);
      roundFunc(args, null).Should().Be(new VMValue(1));
      args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      roundFunc(args, null).Should().Be(new VMValue(2));
      args = new ValueSpan(new VMValue[] { new VMValue(1.567), new VMValue(2) }, 0, 2);
      roundFunc(args, null).Should().Be(new VMValue(1.57));

      args = new ValueSpan(new VMValue[] { new VMValue(1.5), new VMValue(2.1) }, 0, 2);
      Action action = () => roundFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorInvalidInteger);

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) }, 0, 3);
      action = () => roundFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSlice() {
      var sliceFunc = BuiltinsDefinition.SliceFunc;
      var args = new ValueSpan(new VMValue[] {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3)
      }, 0, 3);
      sliceFunc(args, null).ToString().Should().Be("slice(1, 2, 3)");
      args = new ValueSpan(new VMValue[] {
        new VMValue(),
        new VMValue(2),
        new VMValue(),
      }, 0, 3);
      sliceFunc(args, null).ToString().Should().Be("slice(None, 2, None)");
      args = new ValueSpan(new VMValue[] {
        new VMValue(),
        new VMValue(),
        new VMValue(),
      }, 0, 3);
      sliceFunc(args, null).ToString().Should().Be("slice(None, None, None)");

      Action action = () => sliceFunc(new ValueSpan(Array.Empty<VMValue>(), 0, 0), null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSumFunc() {
      var sumFunc = BuiltinsDefinition.SumFunc;

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2), new VMValue(3) })
      }, 0, 1);
      sumFunc(args, null).Should().Be(new VMValue(6));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sumFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }
  }
}
