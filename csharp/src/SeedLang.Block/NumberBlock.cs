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

namespace SeedLang.Block {
  // The block that holds a literal number.
  public class NumberBlock : PrimitiveValueBlock {
    public NumberBlock(string initialCode) {
      UpdateCode(initialCode);
    }

    public override string GetEditableCode() {
      return Value;
    }

    public override bool UpdateCode(string code) {
      // TODO: Validates the input code by the underlying SeedLand engine.

      Value = code;
      return true;
    }
  }
}
