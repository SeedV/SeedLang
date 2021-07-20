// Copyright 2021 The Aha001 Team.
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

using Xunit;

namespace SeedLang.Block.Tests {
  public class SeedBlockTests {
    [Fact]
    public void TestBlockProgram() {
      var program = BlockProgram.Instance;
      Assert.Empty(program.Modules);
      program.LoadModuleFromString("");
      Assert.Single(program.Modules);
      program.LoadModuleFromString("");
      Assert.Equal(2, program.Modules.Count);
      program.Clear();
      Assert.Empty(program.Modules);
    }

    [Fact]
    public void TestModule() {
      var module = new Module();
      module.Add(new NumberBlock("3.14"));
      Assert.Single(module.Blocks);
      Assert.Equal("3.14", ((NumberBlock)module.Blocks[0]).GetEditableCode());
      module.Clear();
      Assert.Empty(module.Blocks);
    }

    [Fact]
    public void TestPrimitiveBlocks() {
      var block = new NumberBlock("3.14");
      Assert.Equal("3.14", block.GetEditableCode());
      block.UpdateCode("10");
      Assert.Equal("10", block.GetEditableCode());
    }
  }
}
