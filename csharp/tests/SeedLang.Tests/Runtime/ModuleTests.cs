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
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ModuleTests {
    [Fact]
    public void TestCreateRootModule() {
      var module = Module.Create("root");
      module.Name.Should().Be("root");
      module.Registers[(uint)module.FindVariable(BuiltinsDefinition.PrintVal)].ToString().
          Should().Be("NativeFunction <__printval__>");
      module.Registers[(uint)module.FindVariable(BuiltinsDefinition.Append)].ToString().
          Should().Be("NativeFunction <append>");
      module.Registers[(uint)module.FindVariable(BuiltinsDefinition.Len)].ToString().
          Should().Be("NativeFunction <len>");
    }

    [Fact]
    public void TestIsInternalFunction() {
      Module.IsInternalFunction(BuiltinsDefinition.PrintVal).Should().Be(true);
      Module.IsInternalFunction(BuiltinsDefinition.Append).Should().Be(false);
    }

    [Fact]
    public void TestImportModule() {
      var module = Module.Create("root");
      var submoduleName = "math";
      module.ImportBuiltinModule(submoduleName);

      module.Registers[(uint)module.FindVariable(new string[] {
        submoduleName,
        MathDefinition.PI,
      })].Should().Be(new VMValue(Math.PI));

      module.Registers[(uint)module.FindVariable(new string[] {
        submoduleName,
        MathDefinition.E,
      })].Should().Be(new VMValue(Math.E));

      module.Registers[(uint)module.FindVariable(new string[] {
        submoduleName,
        MathDefinition.FAbs,
      })].ToString().Should().Be("NativeFunction <fabs>");

      module.Registers[(uint)module.FindVariable(new string[] {
        submoduleName,
        MathDefinition.Sin,
      })].ToString().Should().Be("NativeFunction <sin>");
    }
  }
}
