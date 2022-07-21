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
  public class MathDefinitionTests {
    [Fact]
    public void TestAcos() {
      var acosFunc = MathDefinition.AcosFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      acosFunc(args, null).Should().Be(new VMValue(Math.Acos(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => acosFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAcosh() {
      var acoshFunc = MathDefinition.AcoshFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      acoshFunc(args, null).Should().Be(new VMValue(Math.Acosh(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => acoshFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAsin() {
      var asinFunc = MathDefinition.AsinFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      asinFunc(args, null).Should().Be(new VMValue(Math.Asin(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => asinFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAsinh() {
      var asinhFunc = MathDefinition.AsinhFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      asinhFunc(args, null).Should().Be(new VMValue(Math.Asinh(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => asinhFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAtan() {
      var atanFunc = MathDefinition.AtanFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      atanFunc(args, null).Should().Be(new VMValue(Math.Atan(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => atanFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAtan2() {
      var atan2Func = MathDefinition.Atan2Func;
      var args = new ValueSpan(new VMValue[] { new VMValue(2), new VMValue(0.5) }, 0, 2);
      atan2Func(args, null).Should().Be(new VMValue(Math.Atan2(2, 0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) }, 0, 3);
      Action action = () => atan2Func(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestAtanh() {
      var atanhFunc = MathDefinition.AtanhFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(0.5) }, 0, 1);
      atanhFunc(args, null).Should().Be(new VMValue(Math.Atanh(0.5)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => atanhFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestCeil() {
      var ceilFunc = MathDefinition.CeilFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      ceilFunc(args, null).Should().Be(new VMValue(2));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => ceilFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestCos() {
      var cosFunc = MathDefinition.CosFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      cosFunc(args, null).Should().Be(new VMValue(Math.Cos(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => cosFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestCosh() {
      var coshFunc = MathDefinition.CoshFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      coshFunc(args, null).Should().Be(new VMValue(Math.Cosh(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => coshFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestDegrees() {
      var degreesFunc = MathDefinition.DegreesFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      degreesFunc(args, null).Should().Be(new VMValue(180 / Math.PI * 1.5));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => degreesFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestExp() {
      var expFunc = MathDefinition.ExpFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      expFunc(args, null).Should().Be(new VMValue(Math.Exp(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => expFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestFloor() {
      var floorFunc = MathDefinition.FloorFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      floorFunc(args, null).Should().Be(new VMValue(1));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => floorFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLog() {
      var logFunc = MathDefinition.LogFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      logFunc(args, null).Should().Be(new VMValue(Math.Log(2)));
      args = new ValueSpan(new VMValue[] { new VMValue(2), new VMValue(2) }, 0, 2);
      logFunc(args, null).Should().Be(new VMValue(Math.Log(2, 2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) }, 0, 3);
      Action action = () => logFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestLog10() {
      var log10Func = MathDefinition.Log10Func;
      var args = new ValueSpan(new VMValue[] { new VMValue(2) }, 0, 1);
      log10Func(args, null).Should().Be(new VMValue(Math.Log10(2)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => log10Func(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestPow() {
      var powFunc = MathDefinition.PowFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(2), new VMValue(3) }, 0, 2);
      powFunc(args, null).Should().Be(new VMValue(Math.Pow(2, 3)));

      args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      Action action = () => powFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestRadians() {
      var radiansFunc = MathDefinition.RadiansFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1.5) }, 0, 1);
      radiansFunc(args, null).Should().Be(new VMValue(Math.PI / 180 * 1.5));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => radiansFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSin() {
      var sinFunc = MathDefinition.SinFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      sinFunc(args, null).Should().Be(new VMValue(Math.Sin(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sinFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSinh() {
      var sinhFunc = MathDefinition.SinhFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      sinhFunc(args, null).Should().Be(new VMValue(Math.Sinh(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sinhFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestSqrt() {
      var sqrtFunc = MathDefinition.SqrtFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(10) }, 0, 1);
      sqrtFunc(args, null).Should().Be(new VMValue(Math.Sqrt(10)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => sqrtFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestTan() {
      var tanFunc = MathDefinition.TanFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      tanFunc(args, null).Should().Be(new VMValue(Math.Tan(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => tanFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }

    [Fact]
    public void TestTanh() {
      var tanhFunc = MathDefinition.TanhFunc;
      var args = new ValueSpan(new VMValue[] { new VMValue(1) }, 0, 1);
      tanhFunc(args, null).Should().Be(new VMValue(Math.Tanh(1)));

      args = new ValueSpan(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2);
      Action action = () => tanhFunc(args, null);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorIncorrectArgsCount);
    }
  }
}
