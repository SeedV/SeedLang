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
using System.IO;

namespace SeedLang.Block {
  // The utility to generate Bxf JSON strings or files.
  public static class BxfWriter {
    private class BlockInfo {
      public string Id { get; }
      public string Type { get; }
      public string Value { get; }
      public Position Pos { get; }
      public BlockInfo(string id, string type, string value, Position pos) {
        Id = id;
        Type = type;
        Value = value;
        Pos = pos;
      }
    }

    private class BlockInfoCollector : IBlockVisitor {
      private readonly List<BlockInfo> _blocks = new List<BlockInfo>();

      public IReadOnlyList<BlockInfo> Blocks => _blocks;

      public void VisitEnter(BaseBlock block) {
      }

      public void VisitExit(BaseBlock block) {
      }

      public void VisitExpressionBlock(ExpressionBlock block) {
        _blocks.Add(new BlockInfo(block.Id, "expression", "", block.Pos));
      }

      public void VisitNumberBlock(NumberBlock block) {
        _blocks.Add(new BlockInfo(block.Id, "number", block.Value, block.Pos));
      }

      public void VisitArithmeticOperatorBlock(ArithmeticOperatorBlock block) {
        _blocks.Add(new BlockInfo(block.Id, "operator", block.Name, block.Pos));
      }

      public void VisitParenthesisBlock(ParenthesisBlock block) {
        _blocks.Add(new BlockInfo(block.Id, "operator", block.Name, block.Pos));
      }
    }

    public static string WriteToString(Module module) {
      var sb = new StringWriter();
      sb.WriteLine('{');
      sb.WriteLine($"  \"schema\": \"{BxfConstants.Schema}\",");
      sb.WriteLine($"  \"version\": \"{BxfConstants.Version}\",");
      sb.WriteLine("  \"module\": {");
      sb.WriteLine($"    \"name\": \"{module.Name}\",");
      sb.WriteLine($"    \"doc\": \"{module.Doc}\",");
      sb.WriteLine("    \"blocks\": [");
      foreach (var rootBlock in module.RootBlockIterator) {
        var collector = new BlockInfoCollector();
        rootBlock.Accept(collector);
        for (int i = 0; i < collector.Blocks.Count; i++) {
          var block = collector.Blocks[i];
          sb.WriteLine("      {");
          sb.WriteLine($"        \"id\": \"{block.Id}\",");
          sb.WriteLine($"        \"type\": \"{block.Type}\",");
          sb.WriteLine($"        \"value\": \"{block.Value}\",");
          sb.WriteLine("        \"position\": {");
          if (!block.Pos.IsDocked) {
            sb.WriteLine($"          \"x\": {block.Pos.CanvasPosition.X},");
            sb.WriteLine($"          \"y\": {block.Pos.CanvasPosition.Y}");
          } else {
            string docType =
              block.Pos.Type == Position.DockType.Input ? "input" :
              (block.Pos.Type == Position.DockType.ChildStatement ?
               "childStatement" : "nextStatement");
            sb.WriteLine($"          \"targetId\": \"{block.Pos.TargetBlockId}\",");
            sb.WriteLine($"          \"dockType\": \"{docType}\",");
            sb.WriteLine($"          \"dockSlotIndex\": {block.Pos.DockSlotIndex}");
          }
          sb.WriteLine("        }");
          sb.WriteLine(i == collector.Blocks.Count - 1 ? "      }" : "      },");
        }
      }
      sb.WriteLine("    ]");
      sb.WriteLine("  }");
      sb.WriteLine('}');
      return sb.ToString();
    }
  }
}

