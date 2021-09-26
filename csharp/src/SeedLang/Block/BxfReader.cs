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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using SeedLang.Common;
using System.IO;

namespace SeedLang.Block {
  // The utilities to parse BXF JSON strings or files.
  public static class BxfReader {
    // Parses a BXF JSON string and returns the module object.
    //
    // Returns null if one of the following critical issues is found:
    //
    // * The input string is not a valid BXF JSON.
    // * Failed to compose a valid SeedBlock module with the parsed object due to a fatal issue.
    //
    // The parser tries to tolerate non-critical issues in the input string and collect as many
    // diagnostic messages as possible, until a fatal issue is met. Hence, even when the return
    // value is a valid SeedBlock module, diagnosticCollection may still contain one or more
    // detected issues.
    public static Module ReadFromString(string json, DiagnosticCollection diagnosticCollection) {
      // TODO: Consider supporting async operation since this operation may cost some time.
      BxfObject bxfObject;
      try {
        bxfObject = JsonConvert.DeserializeObject<BxfObject>(json);
      } catch (JsonException e) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Fatal, null, null,
                                    Message.InvalidJson1, e.Message);
        return null;
      }
      Module module = ConvertBxfToModule(bxfObject, diagnosticCollection);
      return module;
    }

    // Parses a BXF JSON file and returns the module object.
    //
    // Returns null if one of the following critical issues is found:
    //
    // * Failed to read the input file.
    // * The input file is not a valid BXF JSON.
    // * Failed to compose a valid SeedBlock module with the parsed object due to a fatal issue.
    //
    // The parser tries to tolerate non-critical issues in the input string and collect as many
    // diagnostic messages as possible, until a fatal issue is met. Hence, even when the return
    // value is a valid SeedBlock module, diagnosticCollection may still contain one ore more
    // detected issues.
    public static Module ReadFromFile(string path, DiagnosticCollection diagnosticCollection) {
      string json;
      try {
        json = File.ReadAllText(path);
      } catch (Exception e) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Fatal, null, null,
                                    Message.FailedToReadFile2, path, e.Message);
        return null;
      }
      return ReadFromString(json, diagnosticCollection);
    }

    private static Module ConvertBxfToModule(BxfObject bxfObject,
                                             DiagnosticCollection diagnosticCollection) {
      if (bxfObject.Schema != BxfConstants.Schema) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Fatal, null, null,
                                    Message.InvalidBxfSchema1, bxfObject.Schema);
        return null;
      }
      // TODO: Here is an exact version matching. Change it to a version compatibility check when
      // new version numbers are supported.
      if (bxfObject.Version != BxfConstants.Version) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Fatal, null, null,
                                    Message.InvalidBxfVersion1, bxfObject.Version);
        return null;
      }

      var module = new Module();
      if (string.IsNullOrEmpty(bxfObject.Module.Name)) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Error, null, null,
                                    Message.EmptyModuleName);
      }

      module.Name = bxfObject.Module.Name;
      module.Doc = bxfObject.Module.Doc ?? "";
      var idSet = new HashSet<string>();
      int maxIdNumber = -1;
      var cachedBlocks = new List<BaseBlock>();

      foreach (var bxfBlock in bxfObject.Module.Blocks) {
        var block = ConvertBxfToBlock(module, bxfBlock, diagnosticCollection);
        if (block is null) {
          continue;
        }
        int idNumber = ParseBlockId(module, block.Id, diagnosticCollection);
        if (idNumber < 0) {
          continue;
        }
        if (idNumber > maxIdNumber) {
          maxIdNumber = idNumber;
        }
        if (idSet.Contains(block.Id)) {
          diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Error, module.Name, null,
                                      Message.DuplicateBlockId1, block.Id);
          continue;
        }
        idSet.Add(block.Id);
        cachedBlocks.Add(block);
      }

      try {
        module.BatchLoadBlocks(cachedBlocks, maxIdNumber);
      } catch (DiagnosticException e) {
        diagnosticCollection.Report(e.Diagnostic);
        return null;
      }
      return module;
    }

    private static BaseBlock ConvertBxfToBlock(Module module,
                                               BxfBlock bxfBlock,
                                               DiagnosticCollection diagnosticCollection) {
      BaseBlock block;
      switch (bxfBlock.Type) {
        case BxfConstants.BlockType.ArithmeticOperator:
          block = new ArithmeticOperatorBlock();
          break;
        case BxfConstants.BlockType.Expression:
          block = new ExpressionBlock();
          break;
        case BxfConstants.BlockType.Number:
          block = new NumberBlock();
          break;
        case BxfConstants.BlockType.Parenthsis:
          block = new ParenthesisBlock();
          break;
        default:
          diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Error, module.Name, null,
                                      Message.InvalidBlockType1, bxfBlock.Type);
          return null;
      }
      if (string.IsNullOrEmpty(bxfBlock.Id)) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Error, module.Name, null,
                                    Message.EmptyBlockId);
        return null;
      }
      block.Id = bxfBlock.Id;
      block.Doc = bxfBlock.Doc ?? "";
      block.Pos = bxfBlock.ToBlockPosition();
      if (block.Pos is null) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Error, module.Name, null,
                                    Message.BlockHasNoPosition);
        return null;
      }
      if (block is IEditable) {
        try {
          (block as IEditable).UpdateText(bxfBlock.Content ?? "");
        } catch (DiagnosticException e) {
          if (e.Diagnostic.Module is null) {
            e.Diagnostic.Module = module.Name;
          }
          diagnosticCollection.Report(e.Diagnostic);
        }
      }
      return block;
    }

    private static int ParseBlockId(Module module,
                                    string id,
                                    DiagnosticCollection diagnosticCollection) {
      try {
        return int.Parse(id);
      } catch (Exception) {
        diagnosticCollection.Report(SystemReporters.SeedBlock, Severity.Error, module.Name, null,
                                    Message.InvalidBlockId1, id);
        return -1;
      }
    }
  }
}
