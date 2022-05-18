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

namespace SeedLang.Runtime.Tests {
  using NativeFunction = HeapObject.NativeFunction;
  using Range = HeapObject.Range;

  public class NativeFunctionsTests {
    [Fact]
    public void TestIsInternalFunction() {
      NativeFunctions.IsInternalFunction(NativeFunctions.PrintVal).Should().Be(true);
      NativeFunctions.IsInternalFunction(NativeFunctions.Append).Should().Be(false);
    }

    [Fact]
    public void TestPrintValFunc() {
      var printValFunc = FindFunc(NativeFunctions.PrintVal);
      var sys = new Sys() { Stdout = new StringWriter() };
      printValFunc.Call(new VMValue[] { new VMValue() }, 0, 1, sys).Should().Be(new VMValue());
      sys.Stdout.ToString().Should().Be("");
      printValFunc.Call(new VMValue[] { new VMValue(1) }, 0, 1, sys).Should().Be(new VMValue());
      sys.Stdout.ToString().Should().Be("1" + Environment.NewLine);
      Action action = () => printValFunc.Call(new VMValue[] { new VMValue(1), new VMValue(2), },
                                              0, 2, sys);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAppendFunc() {
      var appendFunc = FindFunc(NativeFunctions.Append);
      var args = new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
        new VMValue(3),
      };
      appendFunc.Call(args, 0, args.Length, null).Should().Be(new VMValue());
      args[0].Length.Should().Be(3);
      var expectedList = new List<VMValue> { new VMValue(1), new VMValue(2), new VMValue(3) };
      args[0].AsList().Should().BeEquivalentTo(expectedList);
      Action action = () => appendFunc.Call(Array.Empty<VMValue>(), 0, 1, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLenFunc() {
      var lenFunc = FindFunc(NativeFunctions.Len);
      var args = new VMValue[] {
        new VMValue(new List<VMValue> { new VMValue(1), new VMValue(2) }),
      };
      lenFunc.Call(args, 0, args.Length, null).Should().Be(new VMValue(2));
      Action action = () => lenFunc.Call(Array.Empty<VMValue>(), 0, 2, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestListFunc() {
      var listFunc = FindFunc(NativeFunctions.List);

      VMValue list = listFunc.Call(Array.Empty<VMValue>(), 0, 0, null);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(0);

      var args = new VMValue[] {
        new VMValue(new List<VMValue>() { new VMValue(1), new VMValue(2) }),
      };
      list = listFunc.Call(args, 0, args.Length, null);
      list.IsList.Should().Be(true);
      var expectedList = new List<VMValue> { new VMValue(1), new VMValue(2) };
      list.AsList().Should().BeEquivalentTo(expectedList);

      int length = 10;
      args = new VMValue[] { new VMValue(new Range(length)) };
      list = listFunc.Call(args, 0, args.Length, null);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(length);
      for (int i = 0; i < length; i++) {
        list[new VMValue(i)].AsNumber().Should().Be(i);
      }

      Action action = () => listFunc.Call(Array.Empty<VMValue>(), 0, 2, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestPrintFunc() {
      var sys = new Sys() { Stdout = new StringWriter() };
      var printFunc = FindFunc(NativeFunctions.Print);
      var args = new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) };
      printFunc.Call(args, 0, args.Length, sys);
      sys.Stdout.ToString().Should().Be("1 2 3" + Environment.NewLine);
    }

    [Fact]
    public void TestSlice() {
      var sliceFunc = FindFunc(NativeFunctions.Slice);
      var args = new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) };
      sliceFunc.Call(args, 0, args.Length, null).ToString().Should().Be("slice(1, 2, 3)");
      args = new VMValue[] { new VMValue(), new VMValue(2), new VMValue() };
      sliceFunc.Call(args, 0, args.Length, null).ToString().Should().Be("slice(None, 2, None)");
      args = new VMValue[] { new VMValue(), new VMValue(), new VMValue() };
      sliceFunc.Call(args, 0, args.Length, null).ToString().Should().Be("slice(None, None, None)");
    }

    private static NativeFunction FindFunc(string name) {
      var func = Array.Find(NativeFunctions.Funcs, func => func.Name == name);
      func.Should().NotBeNull();
      return func;
    }
  }
}
