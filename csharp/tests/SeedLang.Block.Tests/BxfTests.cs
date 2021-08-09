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
  public class BxfTests {
    private const string _bxfJson = @"{
  ""schema"": ""bxf"",
  ""version"": ""v0.1"",
  ""module"": {
    ""name"": ""test"",
    ""doc"": """",
    ""blocks"": [
      {
        ""id"": ""00000004"",
        ""type"": ""operator"",
        ""value"": ""("",
        ""position"": {
          ""targetId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 0
        }
      },
      {
        ""id"": ""00000001"",
        ""type"": ""number"",
        ""value"": ""3.14"",
        ""position"": {
          ""targetId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 1
        }
      },
      {
        ""id"": ""00000003"",
        ""type"": ""operator"",
        ""value"": ""*"",
        ""position"": {
          ""targetId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 2
        }
      },
      {
        ""id"": ""00000002"",
        ""type"": ""number"",
        ""value"": ""5"",
        ""position"": {
          ""targetId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 3
        }
      },
      {
        ""id"": ""00000005"",
        ""type"": ""operator"",
        ""value"": "")"",
        ""position"": {
          ""targetId"": ""00000000"",
          ""dockType"": ""input"",
          ""dockSlotIndex"": 4
        }
      },
      {
        ""id"": ""00000000"",
        ""type"": ""expression"",
        ""value"": """",
        ""position"": {
          ""x"": 0,
          ""y"": 0
        }
      }
    ]
  }
}
";

    [Fact]
    public void TestWriter() {
      var module = new Module("test");
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

      string jsonString = BxfWriter.WriteToString(module);
      Assert.NotEmpty(jsonString);
      Assert.Equal(_bxfJson, jsonString);
    }
  }
}
