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

using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SeedLang.Block {
  // The utility to generate Bxf JSON strings or files.
  public static class BxfWriter {
    private class BxfBlockCollector : IBlockVisitor {
      private readonly List<BxfBlock> _bxfBlocks;

      public BxfBlockCollector(List<BxfBlock> bxfBlocks) {
        _bxfBlocks = bxfBlocks;
      }

      public void VisitEnter(BaseBlock block) {
      }

      public void VisitExit(BaseBlock block) {
      }

      public void VisitArithmeticOperatorBlock(ArithmeticOperatorBlock block) {
        var bxfBlock = new BxfBlock(block.Id, BxfConstants.BlockType.ArithmeticOperator,
                                    block.Doc, block.Name, block.Pos);
        _bxfBlocks.Add(bxfBlock);
      }

      public void VisitExpressionBlock(ExpressionBlock block) {
        var bxfBlock = new BxfBlock(block.Id, BxfConstants.BlockType.Expression,
                                    block.Doc, "", block.Pos);
        _bxfBlocks.Add(bxfBlock);
      }

      public void VisitNumberBlock(NumberBlock block) {
        var bxfBlock = new BxfBlock(block.Id, BxfConstants.BlockType.Number,
                                    block.Doc, block.Value, block.Pos);
        _bxfBlocks.Add(bxfBlock);
      }

      public void VisitParenthesisBlock(ParenthesisBlock block) {
        var bxfBlock = new BxfBlock(block.Id, BxfConstants.BlockType.Parenthsis,
                                    block.Doc, block.Name, block.Pos);
        _bxfBlocks.Add(bxfBlock);
      }
    }

    // Serializes a SeedBlock module to a JSON string.
    public static string WriteToString(Module module) {
      var obj = new BxfObject();
      // TODO: Fill in "author", "copyright", etc. when the info is available.
      obj.Module.Name = module.Name;
      obj.Module.Doc = module.Doc;
      var blockCollector = new BxfBlockCollector(obj.Module.Blocks);
      foreach (var rootBlock in module.RootBlockIterator) {
        rootBlock.Accept(blockCollector);
      }
      try {
        return JsonSerializer.Serialize(obj, BxfConstants.JsonOptions);
      } catch (JsonException e) {
        // Must not run into this since WriteToString is a pure internal data serialization.
        Debug.Fail($"Serialization failed: {e}");
        return null;
      }
    }

    // Serializes a SeedBlock module to a JSON file.
    //
    // Throws an exception when there are IO errors. See the API doc of File.WriteAllText. It's the
    // client code's duty to catch or re-throw the exception.
    public static void WriteToFile(string path, Module module) {
      string json = WriteToString(module);
      Debug.Assert(!string.IsNullOrEmpty(json));
      File.WriteAllText(path, json);
    }
  }
}
