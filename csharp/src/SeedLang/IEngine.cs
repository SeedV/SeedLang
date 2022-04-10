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

using System.Collections.Generic;
using System.IO;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang {
  public interface IEngine {
    void RedirectStdout(TextWriter stdout);

    void Register<Visualizer>(Visualizer visualizer);
    void Unregister<Visualizer>(Visualizer visualizer);

    void ParseSyntaxTokens(string source, string module, SeedXLanguage language,
                           out IReadOnlyList<TokenInfo> syntaxTokens);

    bool ParseSemanticTokens(string source, string module, SeedXLanguage language,
                             out IReadOnlyList<TokenInfo> semanticTokens,
                             DiagnosticCollection collection = null);

    string Run(string source, string module, SeedXLanguage language, RunType runType,
               RunMode runMode = RunMode.Script, DiagnosticCollection collection = null);
  }
}
