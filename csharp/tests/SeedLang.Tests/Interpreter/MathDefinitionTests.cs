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
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class MathDefinitionTests {
    [Fact]
    public void TestFAbs() {
      var fAbsFunc = FindFunc(MathDefinition.FAbs);
      var args = new ValueSpan(new VMValue[] {
        new VMValue(-1),
      }, 0, 1);
      fAbsFunc.Call(args, null).Should().Be(new VMValue(1));
      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), }, 0, 2);
      Action action = () => fAbsFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);

    }

    [Fact]
    public void TestSin() {
      var sinFunc = FindFunc(MathDefinition.Sin);
      var args = new ValueSpan(new VMValue[] {
        new VMValue(1),
      }, 0, 1);
      sinFunc.Call(args, null).Should().Be(new VMValue(Math.Sin(1)));
      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), }, 0, 2);
      Action action = () => sinFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);

    }

    private static NativeFunction FindFunc(string name) {
      var value = MathDefinition.Variables[name];
      value.IsFunction.Should().BeTrue();
      return value.AsFunction() as NativeFunction;
    }
  }
}
