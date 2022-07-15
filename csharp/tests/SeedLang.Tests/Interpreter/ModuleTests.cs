using System;
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

using FluentAssertions;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class ModuleTests {
    [Fact]
    public void TestModule() {
      var registers = new GlobalRegisters();
      var module = new Module("root", registers);
      module.Name.Should().Be("root");
    }

    [Fact]
    public void TestMathModule() {
      var registers = new GlobalRegisters();
      var module = Module.CreateFrom("math", MathDefinition.Variables, registers);
      registers[(uint)module.FindVariable("pi")].Should().Be(new VMValue(Math.PI));
      registers[(uint)module.FindVariable("e")].Should().Be(new VMValue(Math.E));
      registers[(uint)module.FindVariable("fabs")].ToString().Should().Be("NativeFunction <fabs>");
      registers[(uint)module.FindVariable("sin")].ToString().Should().Be("NativeFunction <sin>");
    }
  }
}
