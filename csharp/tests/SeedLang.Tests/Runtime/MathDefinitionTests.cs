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
using SeedLang.Runtime.HeapObjects;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class MathDefinitionTests {
    [Fact]
    public void TestAcos() {
      var acosFunc = FindFunc(MathDefinition.Acos);
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      acosFunc.Call(args, null).Should().Be(new VMValue(Math.Acos(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => acosFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAcosh() {
      var acoshFunc = FindFunc(MathDefinition.Acosh);
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      acoshFunc.Call(args, null).Should().Be(new VMValue(Math.Acosh(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => acoshFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAsin() {
      var asinFunc = FindFunc(MathDefinition.Asin);
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      asinFunc.Call(args, null).Should().Be(new VMValue(Math.Asin(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => asinFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAsinh() {
      var asinhFunc = FindFunc(MathDefinition.Asinh);
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      asinhFunc.Call(args, null).Should().Be(new VMValue(Math.Asinh(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => asinhFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAtan() {
      var atanFunc = FindFunc(MathDefinition.Atan);
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      atanFunc.Call(args, null).Should().Be(new VMValue(Math.Atan(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => atanFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAtan2() {
      var atan2Func = FindFunc(MathDefinition.Atan2);
      var args = new ValueSpan(new VMValue[] { new VMValue(2), new VMValue(0.5) }, 0, 2);
      atan2Func.Call(args, null).Should().Be(new VMValue(Math.Atan2(2, 0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) }, 0, 3);
      Action action = () => atan2Func.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAtanh() {
      var atanhFunc = FindFunc(MathDefinition.Atanh);
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      atanhFunc.Call(args, null).Should().Be(new VMValue(Math.Atanh(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => atanhFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestCeil() {
      var ceilFunc = FindFunc(MathDefinition.Ceil);
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      ceilFunc.Call(args, null).Should().Be(new VMValue(2));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => ceilFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestCos() {
      var cosFunc = FindFunc(MathDefinition.Cos);
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      cosFunc.Call(args, null).Should().Be(new VMValue(Math.Cos(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => cosFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestCosh() {
      var coshFunc = FindFunc(MathDefinition.Cosh);
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      coshFunc.Call(args, null).Should().Be(new VMValue(Math.Cosh(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => coshFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestDegrees() {
      var degreesFunc = FindFunc(MathDefinition.Degrees);
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      degreesFunc.Call(args, null).Should().Be(new VMValue(180 / Math.PI * 1.5));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => degreesFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestExp() {
      var expFunc = FindFunc(MathDefinition.Exp);
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      expFunc.Call(args, null).Should().Be(new VMValue(Math.Exp(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => expFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestFloor() {
      var floorFunc = FindFunc(MathDefinition.Floor);
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      floorFunc.Call(args, null).Should().Be(new VMValue(1));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => floorFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLog() {
      var logFunc = FindFunc(MathDefinition.Log);
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      logFunc.Call(args, null).Should().Be(new VMValue(Math.Log(2)));
      args = new ValueSpan(new VMValue[] { new VMValue(2), new VMValue(2) }, 0, 2);
      logFunc.Call(args, null).Should().Be(new VMValue(Math.Log(2, 2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) }, 0, 3);
      Action action = () => logFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLog2() {
      var log2Func = FindFunc(MathDefinition.Log2);
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      log2Func.Call(args, null).Should().Be(new VMValue(Math.Log2(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => log2Func.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLog10() {
      var log10Func = FindFunc(MathDefinition.Log10);
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      log10Func.Call(args, null).Should().Be(new VMValue(Math.Log10(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => log10Func.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestPow() {
      var powFunc = FindFunc(MathDefinition.Pow);
      var args = new ValueSpan(new VMValue[] { new VMValue(2), new VMValue(3) }, 0, 2);
      powFunc.Call(args, null).Should().Be(new VMValue(Math.Pow(2, 3)));

      args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      Action action = () => powFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestRadians() {
      var radiansFunc = FindFunc(MathDefinition.Radians);
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      radiansFunc.Call(args, null).Should().Be(new VMValue(Math.PI / 180 * 1.5));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => radiansFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSin() {
      var sinFunc = FindFunc(MathDefinition.Sin);
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      sinFunc.Call(args, null).Should().Be(new VMValue(Math.Sin(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sinFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSinh() {
      var sinhFunc = FindFunc(MathDefinition.Sinh);
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      sinhFunc.Call(args, null).Should().Be(new VMValue(Math.Sinh(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sinhFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSqrt() {
      var sqrtFunc = FindFunc(MathDefinition.Sqrt);
      var args = new ValueSpan(new VMValue[] { new VMValue(10) }, 0, 1);
      sqrtFunc.Call(args, null).Should().Be(new VMValue(Math.Sqrt(10)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sqrtFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestTan() {
      var tanFunc = FindFunc(MathDefinition.Tan);
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      tanFunc.Call(args, null).Should().Be(new VMValue(Math.Tan(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => tanFunc.Call(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestTanh() {
      var tanhFunc = FindFunc(MathDefinition.Tanh);
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      tanhFunc.Call(args, null).Should().Be(new VMValue(Math.Tanh(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => tanhFunc.Call(args, null);
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
