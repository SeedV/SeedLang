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

using System.IO;
using Xunit;
using SeedLang.Common;

namespace SeedLang.Block.Tests {
  public class BxfTests {
    [Fact]
    public void TestReadAndWriteString() {
      var module = new Module { Name = "Main", Doc = "The main entry." };
      Assert.Empty(module.Blocks);
      Assert.Equal(0, module.RootBlockCount);

      var expressionBlock = new ExpressionBlock();
      var numberBlock1 = new NumberBlock { Value = "3.14" };
      var numberBlock2 = new NumberBlock { Value = "5" };
      var operatorBlock = new ArithmeticOperatorBlock { Name = "*" };
      var parenthsisBlock1 = new ParenthesisBlock(ParenthesisBlock.Type.Left);
      var parenthsisBlock2 = new ParenthesisBlock(ParenthesisBlock.Type.Right);

      module.AddStandaloneBlock(expressionBlock);
      module.AddStandaloneBlock(numberBlock1);
      module.AddStandaloneBlock(numberBlock2);
      module.AddStandaloneBlock(operatorBlock);
      module.AddStandaloneBlock(parenthsisBlock1);
      module.AddStandaloneBlock(parenthsisBlock2);

      module.DockBlock(parenthsisBlock2.Id, expressionBlock.Id, Position.DockType.Input, 0);
      module.DockBlock(numberBlock2.Id, expressionBlock.Id, Position.DockType.Input, 0);
      module.DockBlock(operatorBlock.Id, expressionBlock.Id, Position.DockType.Input, 0);
      module.DockBlock(numberBlock1.Id, expressionBlock.Id, Position.DockType.Input, 0);
      module.DockBlock(parenthsisBlock1.Id, expressionBlock.Id, Position.DockType.Input, 0);

      string json = BxfWriter.WriteToString(module);

      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.NotNull(moduleParsed);
      Assert.Empty(diagnosticCollection.Diagnostics);

      Assert.Equal(module.Name, moduleParsed.Name);
      Assert.Equal(module.Doc, moduleParsed.Doc);
      Assert.Equal(module.Blocks.Count, moduleParsed.Blocks.Count);
      Assert.Equal(module.RootBlockCount, moduleParsed.RootBlockCount);

      for (int i = 0; i < 6; i++) {
        string id = i.ToString($"D{BxfConstants.IdLength}");
        BaseBlock block = module.GetBlock(id);
        BaseBlock blockParsed = moduleParsed.GetBlock(id);
        Assert.NotNull(block);
        Assert.NotNull(blockParsed);
        Assert.Equal(block.Doc, blockParsed.Doc);
        Assert.Equal((block as IEditable).GetEditableText(),
                     (blockParsed as IEditable).GetEditableText());
        Assert.Equal(block.Pos, blockParsed.Pos);
      }

      // After a successful Bxf reading, the module can still hold new blocks, there IDs starting
      // from the maximum value of the loaded blocks.
      var newBlock = new NumberBlock { Value = "0" };
      module.AddStandaloneBlock(newBlock);
      Assert.Equal("00000006", newBlock.Id);
    }

    [Fact]
    public void TestReadAndWriteFile() {
      var module = new Module { Name = "Main", Doc = "The main entry." };
      string path = Path.GetTempFileName();
      BxfWriter.WriteToFile(path, module);
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromFile(path, diagnosticCollection);
      File.Delete(path);
      Assert.NotNull(moduleParsed);
      Assert.Empty(diagnosticCollection.Diagnostics);
      Assert.Equal(module.Name, moduleParsed.Name);
      Assert.Equal(module.Doc, moduleParsed.Doc);
      Assert.Equal(module.Blocks.Count, moduleParsed.Blocks.Count);
      Assert.Equal(module.RootBlockCount, moduleParsed.RootBlockCount);
    }

    [Fact]
    public void TestInvalidJson() {
      string invalidJson = "{ invalidJson }"; // @"{ schema: ""bxf"" }";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(invalidJson, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
    }

    [Fact]
    public void TestInvalidBxf() {
      string json = @"{ ""schema"": ""Unknown"" }";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("InvalidBxfSchema Unknown",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);

      json = @"{ ""schema"": ""bxf"",  ""version"": ""Unknown"" }";
      diagnosticCollection = new DiagnosticCollection();
      moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("InvalidBxfVersion Unknown",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
    }

    [Fact]
    public void TestInvalidId() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": """",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""InvalidId"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 0
        }
      },
      {
        ""id"": ""InvalidId"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""InvalidId"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 0
        }
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Equal(2, diagnosticCollection.Diagnostics.Count);
      Assert.Equal(Severity.Error, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("EmptyBlockId",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
      Assert.Equal(Severity.Error, diagnosticCollection.Diagnostics[1].Severity);
      Assert.Equal("InvalidBlockId InvalidId",
                   diagnosticCollection.Diagnostics[1].LocalizedMessage);
      // The BxfReader should report errors and still returns a correct module.
      Assert.NotNull(moduleParsed);
      Assert.Equal(0, moduleParsed.Blocks.Count);
      Assert.Equal(0, moduleParsed.RootBlockCount);
    }

    [Fact]
    public void TestDupId() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": ""00000001"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 0
        }
      },
      {
        ""id"": ""00000002"",
        ""type"": ""arithmeticOperator"",
        ""doc"": """",
        ""content"": ""*"",
        ""dockPosition"": {
          ""targetBlockId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 1
        }
      },
      {
        ""id"": ""00000002"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""5"",
        ""dockPosition"": {
          ""targetBlockId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 2
        }
      },
      {
        ""id"": ""00000000"",
        ""type"": ""expression"",
        ""doc"": """",
        ""content"": """",
        ""canvasPosition"": {
          ""x"": 0,
          ""y"": 0
        }
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Error, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("DuplicateBlockId 00000002",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
      // The BxfReader should report errors and still returns a correct module.
      Assert.NotNull(moduleParsed);
      Assert.Equal(3, moduleParsed.Blocks.Count);
      Assert.Equal(1, moduleParsed.RootBlockCount);
      var block = moduleParsed.GetBlock("00000000");
      Assert.Equal("3.14*", (block as IEditable).GetEditableText());
    }

    [Fact]
    public void TestNoPosition() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": ""00000000"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14""
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Error, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("BlockHasNoPosition",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
      // The BxfReader should report errors and still returns a correct module.
      Assert.NotNull(moduleParsed);
      Assert.Equal(0, moduleParsed.Blocks.Count);
      Assert.Equal(0, moduleParsed.RootBlockCount);
    }

    [Fact]
    public void TestInvalidTargetId() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": ""00000001"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""InvalidId"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 0
        }
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("TargetBlockIdNotExist InvalidId",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
    }

    [Fact]
    public void TestTargetBlockNotDockable1() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": ""00000000"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""5"",
        ""canvasPosition"": {
          ""x"": ""0"",
          ""y"": ""0""
        }
      },
      {
        ""id"": ""00000001"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 0
        }
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("TargetBlockNotDockable 00000001 00000000 Input 0",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
    }

    [Fact]
    public void TestTargetBlockNotDockable2() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": ""00000000"",
        ""type"": ""expression"",
        ""doc"": """",
        ""content"": """",
        ""canvasPosition"": {
          ""x"": 0,
          ""y"": 0
        }
      },
      {
        ""id"": ""00000001"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 1
        }
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("TargetBlockNotDockable 00000001 00000000 Input 1",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
    }

    [Fact]
    public void TestTargetBlockNotDockable3() {
      string json = @"
{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""Main"",
    ""blocks"": [
      {
        ""id"": ""00000000"",
        ""type"": ""expression"",
        ""doc"": """",
        ""content"": """",
        ""canvasPosition"": {
          ""x"": 0,
          ""y"": 0
        }
      },
      {
        ""id"": ""00000001"",
        ""type"": ""number"",
        ""doc"": """",
        ""content"": ""3.14"",
        ""dockPosition"": {
          ""targetBlockId"": ""00000000"",
          ""dockType"": ""ChildStatement"",
          ""dockSlotIndex"": 0
        }
      }
    ]
  }
}";
      var diagnosticCollection = new DiagnosticCollection();
      var moduleParsed = BxfReader.ReadFromString(json, diagnosticCollection);
      Assert.Null(moduleParsed);
      Assert.Single(diagnosticCollection.Diagnostics);
      Assert.Equal(Severity.Fatal, diagnosticCollection.Diagnostics[0].Severity);
      Assert.Equal("TargetBlockNotDockable 00000001 00000000 ChildStatement 0",
                   diagnosticCollection.Diagnostics[0].LocalizedMessage);
    }
  }
}
