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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace SeedLang.Block {
  // The intermediate data structure for serializing or parsing a SeedBlock module.
  //
  // BxfWriter uses the following types to prepare in-memory objects then serializes them to JSON
  // format. BxfReader reads JSON format then stores the parsed result with the following types as
  // an intermediate structure.
  //
  // TODO: It's possible to build an auto-generator to create the following C# code based on
  // /schemas/bxf.schema.json. Consider this possibility in the future.
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore,
              NamingStrategyType = typeof(CamelCaseNamingStrategy))]
  internal class BxfObject {
    public string Schema = BxfConstants.Schema;
    public string Version = BxfConstants.Version;
    public string Author = null;
    public string Copyright = null;
    public string License = null;
    public BxfModule Module = new BxfModule();
  }

  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore,
              NamingStrategyType = typeof(CamelCaseNamingStrategy))]
  internal class BxfModule {
    public string Name = null;
    public string Doc = null;
    public List<BxfBlock> Blocks = new List<BxfBlock>();
  }

  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore,
              NamingStrategyType = typeof(CamelCaseNamingStrategy))]
  internal class BxfBlock {
    public string Id = null;
    public string Type = null;
    public string Doc = null;
    public string Content = null;
    public BxfCanvasPosition CanvasPosition = null;
    public BxfDockPosition DockPosition = null;

    public BxfBlock() {
    }

    public BxfBlock(string id, string type, string doc, string content, Position pos) {
      Id = id;
      Type = type;
      Doc = doc;
      Content = content;
      FromBlockPosition(pos);
    }

    public Position ToBlockPosition() {
      if (!(CanvasPosition is null)) {
        return new Position(CanvasPosition.X, CanvasPosition.Y);
      } else if (!(DockPosition is null)) {
        return new Position(DockPosition.DockType,
                            DockPosition.TargetBlockId,
                            DockPosition.DockSlotIndex);
      } else {
        return null;
      }
    }

    private void FromBlockPosition(Position pos) {
      if (!pos.IsDocked) {
        CanvasPosition = new BxfCanvasPosition {
          X = pos.CanvasPosition.X,
          Y = pos.CanvasPosition.Y
        };
        DockPosition = null;
      } else {
        DockPosition = new BxfDockPosition {
          TargetBlockId = pos.TargetBlockId,
          DockType = pos.Type,
          DockSlotIndex = pos.DockSlotIndex
        };
        CanvasPosition = null;
      }
    }
  }

  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore,
              NamingStrategyType = typeof(CamelCaseNamingStrategy))]
  internal class BxfCanvasPosition {
    public int X = 0;
    public int Y = 0;
  }

  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore,
              NamingStrategyType = typeof(CamelCaseNamingStrategy))]
  internal class BxfDockPosition {
    public string TargetBlockId = null;
    // Uses camelCased enum name when serializing Position.DockType.
    [JsonConverter(typeof(StringEnumConverter), true)]
    public Position.DockType DockType;
    public int DockSlotIndex = 0;
  }
}
