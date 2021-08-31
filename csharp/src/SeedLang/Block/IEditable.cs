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
  // The common interface of all the editable blocks.
  public interface IEditable {
    // Gets the editable block content, a plain text code.
    string GetEditableText();

    // Updates the block content with a new plain text code.
    //
    // Throws a new ArgumentException if the input is not a valid text.
    void UpdateText(string text);
  }
}
