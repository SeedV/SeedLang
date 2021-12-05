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

using System;
using System.Collections.Generic;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Shell {
  // A Read-Evaluate-Print-Loop class to execute SeedX programs interactively.
  internal sealed class Repl {
    private readonly SeedXLanguage _language;
    private readonly RunType _runType;
    private readonly VisualizerManager _visualizerManager;
    private readonly SourceCode _source = new SourceCode();

    internal Repl(SeedXLanguage language, RunType runType,
                  IEnumerable<VisualizerType> visualizerTypes) {
      _language = language;
      _runType = runType;
      _visualizerManager = new VisualizerManager(_source, visualizerTypes);
    }

    internal void Execute() {
      ReadLine.HistoryEnabled = true;
      var executor = new Executor();
      _visualizerManager.RegisterToExecutor(executor);
      while (true) {
        _source.Read();
        if (_source.Source == "quit") {
          break;
        }
        var syntaxTokens = Executor.ParseSyntaxTokens(_source.Source, "", _language);
        _source.WriteSourceWithSyntaxTokens(syntaxTokens);
        Console.WriteLine("---------- Run ----------");
        var runCollection = new DiagnosticCollection();
        executor.Run(_source.Source, "", _language, _runType, runCollection);
        foreach (var diagnostic in runCollection.Diagnostics) {
          if (diagnostic.Range is TextRange range) {
            _source.WriteSourceWithHighlight(range);
          }
          Console.WriteLine($": {diagnostic}");
        }
        Console.WriteLine();
      }
      _visualizerManager.UnregisterFromExecutor(executor);
    }
  }
}
