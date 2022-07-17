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
using System.IO;
using FluentAssertions;
using SeedLang.Common;
using Xunit;
using SeedLang.Runtime.HeapObjects;

namespace SeedLang.Runtime.Tests {
  using Range = HeapObjects.Range;

  internal class MockupNativeContext : INativeContext {
    public TextWriter Stdout { get; } = new StringWriter();

    VMValue INativeContext.Dir() {
      return new VMValue(new List<VMValue> { new VMValue("__builtins__") });
    }

    VMValue INativeContext.Dir(VMValue value) {
      return new VMValue(new List<VMValue> { new VMValue("__builtins__") });
    }
  }

  public class BuiltinsDefinitionTests {
    [Fact]
    public void TestPrintValFunc() {
      var printValFunc = FindFunc(BuiltinsDefinition.PrintVal);
      var context = new MockupNativeContext();

      var args = new ValueSpan(new VMValue[] { new VMValue(), }, 0, 1);
      printValFunc.Call(args, context).Should().Be(new VMValue());
      context.Stdout.ToString().Should().Be("");

      args = new ValueSpan(new VMValue[] { new VMValue(1), }, 0, 1);
      printValFunc.Call(args, context).Should().Be(new VMValue());
      context.Stdout.ToString().Should().Be("1" + Environment.NewLine);

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), }, 0, 2);
      Action action = () => printValFunc.Call(args, context);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAbsFunc() {
      var absFunc = FindFunc(BuiltinsDefinition.Abs);

      var args = new ValueSpan(new VMValue[] { new VMValue(3) }, 0, 1);
      absFunc.Call(args, null).Should().Be(new VMValue(3));
      args = new ValueSpan(new VMValue[] { new VMValue(-3) }, 0, 1);
      absFunc.Call(args, null).Should().Be(new VMValue(3));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => absFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAllFunc() {
      var allFunc = FindFunc(BuiltinsDefinition.All);

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(true), new VMValue(false), new VMValue(true) })
      }, 0, 1);
      allFunc.Call(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(true), new VMValue(true), new VMValue(true) })
      }, 0, 1);
      allFunc.Call(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(new List<VMValue>()) }, 0, 1);
      allFunc.Call(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => allFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAnyFunc() {
      var anyFunc = FindFunc(BuiltinsDefinition.Any);

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> {
          new VMValue(false),
          new VMValue(false),
          new VMValue(true)
        }),
      }, 0, 1);
      anyFunc.Call(args, null).Should().Be(new VMValue(true));

      args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> {
          new VMValue(false),
          new VMValue(false),
          new VMValue(false)
        }),
      }, 0, 1);
      anyFunc.Call(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] { new VMValue(new List<VMValue>()) }, 0, 1);
      anyFunc.Call(args, null).Should().Be(new VMValue(false));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => anyFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAppendFunc() {
      var appendFunc = FindFunc(BuiltinsDefinition.Append);
      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
        new VMValue(3),
      }, 0, 2);
      appendFunc.Call(args, null).Should().Be(new VMValue());
      args[0].Length.Should().Be(3);
      var expectedList = new List<VMValue> { new VMValue(1), new VMValue(2), new VMValue(3) };
      args[0].AsList().Should().BeEquivalentTo(expectedList);
      args = new ValueSpan(new VMValue[] { new VMValue(), }, 0, 1);
      Action action = () => appendFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestDirFunc() {
      var dirFunc = FindFunc(BuiltinsDefinition.Dir);

      var args = new ValueSpan(Array.Empty<VMValue>(), 0, 0);
      dirFunc.Call(args, new MockupNativeContext()).Should().Be(new VMValue(
        new List<VMValue> { new VMValue("__builtins__") }
      ));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => dirFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLenFunc() {
      var lenFunc = FindFunc(BuiltinsDefinition.Len);
      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
      }, 0, 1);
      lenFunc.Call(args, null).Should().Be(new VMValue(2));
      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => lenFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestListFunc() {
      var listFunc = FindFunc(BuiltinsDefinition.List);

      VMValue list = listFunc.Call(new ValueSpan(Array.Empty<VMValue>(), 0, 0), null);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(0);

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue>() { new VMValue(1), new VMValue(2) }),
      }, 0, 1);
      list = listFunc.Call(args, null);
      list.IsList.Should().Be(true);
      var expectedList = new List<VMValue> { new VMValue(1), new VMValue(2) };
      list.AsList().Should().BeEquivalentTo(expectedList);

      int length = 10;
      args = new ValueSpan(new VMValue[] { new VMValue(new Range(length)) }, 0, 1);
      list = listFunc.Call(args, null);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(length);
      for (int i = 0; i < length; i++) {
        list[new VMValue(i)].AsNumber().Should().Be(i);
      }

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => listFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestPrintFunc() {
      var context = new MockupNativeContext();
      var printFunc = FindFunc(BuiltinsDefinition.Print);
      var args = new ValueSpan(new VMValue[] {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3)
      }, 0, 3);
      printFunc.Call(args, context);
      context.Stdout.ToString().Should().Be("1 2 3" + Environment.NewLine);
    }

    [Fact]
    public void TestRangFunc() {
      var rangeFunc = FindFunc(BuiltinsDefinition.Range);
      var args = new ValueSpan(new VMValue[] { new VMValue(10) }, 0, 1);
      rangeFunc.Call(args, null).ToString().Should().Be("range(0, 10, 1)");
      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(10) }, 0, 2);
      rangeFunc.Call(args, null).ToString().Should().Be("range(1, 10, 1)");
      args = new ValueSpan(new VMValue[] {
        new VMValue(1),
        new VMValue(10),
        new VMValue(2)
      }, 0, 3);
      rangeFunc.Call(args, null).ToString().Should().Be("range(1, 10, 2)");

      Action action = () => rangeFunc.Call(new ValueSpan(Array.Empty<VMValue>(), 0, 0), null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestRoundFunc() {
      var roundFunc = FindFunc(BuiltinsDefinition.Round);

      var args = new ValueSpan(new VMValue[] { new VMValue(1.2) }, 0, 1);
      roundFunc.Call(args, null).Should().Be(new VMValue(1));
      args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      roundFunc.Call(args, null).Should().Be(new VMValue(2));
      args = new ValueSpan(new VMValue[] { new VMValue(1.567), new VMValue(2) }, 0, 2);
      roundFunc.Call(args, null).Should().Be(new VMValue(1.57));

      args = new ValueSpan(new VMValue[] { new VMValue(1.5), new VMValue(2.1) }, 0, 2);
      Action action = () => roundFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorInvalidInteger);

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) }, 0, 3);
      action = () => roundFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSlice() {
      var sliceFunc = FindFunc(BuiltinsDefinition.Slice);
      var args = new ValueSpan(new VMValue[] {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3)
      }, 0, 3);
      sliceFunc.Call(args, null).ToString().Should().Be("slice(1, 2, 3)");
      args = new ValueSpan(new VMValue[] {
        new VMValue(),
        new VMValue(2),
        new VMValue(),
      }, 0, 3);
      sliceFunc.Call(args, null).ToString().Should().Be("slice(None, 2, None)");
      args = new ValueSpan(new VMValue[] {
        new VMValue(),
        new VMValue(),
        new VMValue(),
      }, 0, 3);
      sliceFunc.Call(args, null).ToString().Should().Be("slice(None, None, None)");

      Action action = () => sliceFunc.Call(new ValueSpan(Array.Empty<VMValue>(), 0, 0), null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSumFunc() {
      var sumFunc = FindFunc(BuiltinsDefinition.Sum);

      var args = new ValueSpan(new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2), new VMValue(3) })
      }, 0, 1);
      sumFunc.Call(args, null).Should().Be(new VMValue(6));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sumFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    private static NativeFunction FindFunc(string name) {
      var value = BuiltinsDefinition.Variables[name];
      value.IsFunction.Should().BeTrue();
      return value.AsFunction() as NativeFunction;
    }
  }
}
