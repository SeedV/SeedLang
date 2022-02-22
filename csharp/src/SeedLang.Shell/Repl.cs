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

using System;
using System.Collections.Generic;
using System.Linq;
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
        Read();
        if (_source.Source == "quit" + Environment.NewLine) {
          break;
        }
        var syntaxTokens = Executor.ParseSyntaxTokens(_source.Source, "", _language);
        _source.WriteSourceWithSyntaxTokens(syntaxTokens);
        Console.WriteLine("---------- Run ----------");
        var collection = new DiagnosticCollection();
        string result = executor.Run(_source.Source, "", _language, _runType, collection);
        if (!(result is null)) {
          Console.WriteLine(result);
        }
        foreach (var diagnostic in collection.Diagnostics) {
          if (diagnostic.Range is TextRange range) {
            _source.WriteSourceWithHighlight(range);
          }
          Console.WriteLine($"{diagnostic}");
        }
      }
      _visualizerManager.UnregisterFromExecutor(executor);
    }

    // Reads the source code from console. Continues to read if there is a ':' character in the end
    // of this line.
    private void Read() {
      _source.Reset();
      string line = null;
      while (string.IsNullOrEmpty(line)) {
        line = ReadLine.Read(">>> ").TrimEnd();
      }
      _source.AddLine(line);
      if (line.Last() == ':') {
        while (true) {
          line = ReadLine.Read("... ").TrimEnd();
          if (string.IsNullOrEmpty(line)) {
            break;
          }
          _source.AddLine(line);
        }
      }
    }
  }
}
