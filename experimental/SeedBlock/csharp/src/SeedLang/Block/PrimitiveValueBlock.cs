// Copyright 2021-2022 The SeedV Lab.
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

using SeedLang.Common;

namespace SeedLang.Block {
  // The common class for all the block types that only have a primitive value as their content.
  public abstract class PrimitiveValueBlock : BaseBlock, IEditable {
    private string _value = "";

    public string Value {
      get {
        return GetEditableText();
      }
      set {
        UpdateText(value);
      }
    }

    public virtual string GetEditableText() {
      return _value;
    }

    public virtual void UpdateText(string text) {
      if (ValidateText(text)) {
        _value = text;
      } else {
        // The thrown exception has no module name associated, since there is no block-to-module
        // mapping for now. It depends on the outside logic to fill the info in.
        //
        // TODO: a better solution to fill the module name in.
        throw new DiagnosticException(SystemReporters.SeedBlock, Severity.Error,
                                      null,
                                      new BlockRange(Id),
                                      Message.InvalidPrimitiveValue1, text);
      }
    }

    protected abstract bool ValidateText(string text);
  }
}
