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
  public class ProgramTests {
    [Fact]
    public void TestBlockProgram() {
      var program = Program.Instance;
      Assert.Empty(program.Modules);
      const string minimalBxf = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""MinimalBxfExample"",
    ""blocks"": []
  }
}
";
      program.LoadModuleFromString(minimalBxf);
      Assert.Single(program.Modules);
      program.Add(new Module());
      Assert.Equal(2, program.Modules.Count);
      program.Clear();
      Assert.Empty(program.Modules);
    }
  }
}
