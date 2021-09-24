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

using System.Collections.Generic;
using Xunit;
using SeedLang.Common;

namespace SeedLang.Block.Tests {
  public class ConverterTests {
    [Fact]
    public void TestValidateInlineTextExpression() {
      var collection = new DiagnosticCollection();
      Assert.False(Converter.IsValidInlineTextExpression(null, collection));
      Assert.Single(collection.Diagnostics);

      collection = new DiagnosticCollection();
      Assert.False(Converter.IsValidInlineTextExpression("", collection));
      Assert.Single(collection.Diagnostics);

      collection = new DiagnosticCollection();
      Assert.True(Converter.IsValidInlineTextExpression("3+4", collection));
      Assert.Empty(collection.Diagnostics);

      collection = new DiagnosticCollection();
      Assert.True(Converter.IsValidInlineTextExpression("3+4*-1-(2/3e-2+(1+2))", collection));
      Assert.Empty(collection.Diagnostics);

      collection = new DiagnosticCollection();
      Assert.False(Converter.IsValidInlineTextExpression("@@@", collection));
      Assert.Single(collection.Diagnostics);
    }

    [Fact]
    public void TestInlineTextToBlocks() {
      var collection = new DiagnosticCollection();
      Assert.Null(Converter.InlineTextToBlocks(null, null, collection));
      Assert.Single(collection.Diagnostics);

      collection = new DiagnosticCollection();
      Assert.Null(Converter.InlineTextToBlocks("", null, collection));
      Assert.Single(collection.Diagnostics);

      var expressionBlock = new ExpressionBlock();
      var module = new Module { Name = "Main" };
      module.AddStandaloneBlock(expressionBlock);
      collection = new DiagnosticCollection();
      var blocks = Converter.InlineTextToBlocks("(1 + 2) * 3 / -4 + 3.14", null, collection);
      int index = 0;
      foreach (var block in blocks) {
        module.AddStandaloneBlock(block);
        expressionBlock.Dock(block, Position.DockType.Input, index++);
      }
      Assert.Equal(11, index);
      Assert.Equal(new TextRange(1, 19, 1, 22), blocks[index - 1].InlineTextReference);
      Assert.Equal("(1+2)*3/-4+3.14", expressionBlock.GetEditableText());
      Assert.Empty(collection.Diagnostics);
    }

    [Fact]
    public void TestInlineTextWithInvalidTokens() {
      var collection = new DiagnosticCollection();
      var invalidTokens = new List<TextRange>();
      var blocks = Converter.InlineTextToBlocks("9 * -3.14@3@@", invalidTokens, collection);
      Assert.Equal(4, blocks.Count);
      Assert.Equal("9", (blocks[0] as NumberBlock).Value);
      Assert.Equal(new TextRange(1, 0, 1, 0), blocks[0].InlineTextReference);
      Assert.Equal("*", (blocks[1] as ArithmeticOperatorBlock).Name);
      Assert.Equal(new TextRange(1, 2, 1, 2), blocks[1].InlineTextReference);
      Assert.Equal("-3.14", (blocks[2] as NumberBlock).Value);
      Assert.Equal(new TextRange(1, 4, 1, 8), blocks[2].InlineTextReference);
      Assert.Equal("3", (blocks[3] as NumberBlock).Value);
      Assert.Equal(new TextRange(1, 10, 1, 10), blocks[3].InlineTextReference);
      Assert.Equal(3, invalidTokens.Count);
      Assert.Equal(new TextRange(1, 9, 1, 9), invalidTokens[0]);
      Assert.Equal(new TextRange(1, 11, 1, 11), invalidTokens[1]);
      Assert.Equal(new TextRange(1, 12, 1, 12), invalidTokens[2]);
    }
  }
}
