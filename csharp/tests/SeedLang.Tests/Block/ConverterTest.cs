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
  public class ConverterTests {
    [Fact]
    public void TestInlineTextToBlocks() {
      var module = new Module { Name = "Main" };
      var expressionBlock = new ExpressionBlock();
      module.AddStandaloneBlock(expressionBlock);
      var blocks = Converter.InlineTextToBlocks("(1 + 2) * 3 / -4 + 3.14");
      int index = 0;
      foreach (var block in blocks) {
        module.AddStandaloneBlock(block);
        expressionBlock.Dock(block, Position.DockType.Input, index++);
      }
      Assert.Equal(11, index);
      Assert.Equal("(1+2)*3/-4+3.14", expressionBlock.GetEditableText());
    }
  }
}
