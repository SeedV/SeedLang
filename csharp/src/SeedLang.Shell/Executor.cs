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
using System.IO;
using SeedLang.Common;

namespace SeedLang.Shell {
  internal static class Executor {
    internal static void Repl(SeedXLanguage language, RunType runType) {
      ReadLine.HistoryEnabled = true;
      var engine = new Engine(language, RunMode.Interactive);
      VisualizerManager.Source = new SourceCode();
      var source = VisualizerManager.Source;
      VisualizerManager.RegisterToEngine(engine);
      while (true) {
        Read(source);
        if (source.Source == "quit" + Environment.NewLine) {
          break;
        }
        RunSource(engine, source, runType);
      }
      VisualizerManager.UnregisterFromEngine(engine);
    }

    internal static void RunScript(string filename, SeedXLanguage language, RunType runType) {
      VisualizerManager.Source = new SourceCode();
      var source = VisualizerManager.Source;
      try {
        foreach (string line in File.ReadLines(filename)) {
          source.AddLine(line);
        }
      } catch (Exception ex) {
        Console.WriteLine($"Read file error: {ex}.");
        return;
      }
      var engine = new Engine(language, RunMode.Script);
      VisualizerManager.RegisterToEngine(engine);
      RunSource(engine, source, runType);
      VisualizerManager.UnregisterFromEngine(engine);
    }

    private static void RunSource(Engine engine, SourceCode source, RunType runType) {
      var collection = new DiagnosticCollection();
      if (engine.Compile(source.Source, "", collection)) {
        source.WriteSourceWithTokens(engine.SemanticTokens);
        Console.WriteLine();
        switch (runType) {
          case RunType.DumpAst:
            Console.WriteLine("---------- Ast ----------");
            Console.WriteLine(engine.DumpAst());
            break;
          case RunType.Disassemble:
            Console.WriteLine("---------- Disassemble ----------");
            Console.WriteLine(engine.Disassemble());
            break;
          case RunType.Execute:
            Console.WriteLine("---------- Run ----------");
            engine.Run(collection);
            break;
        }
      } else {
        source.WriteSourceWithTokens(engine.ParseSyntaxTokens(source.Source, ""));
        Console.WriteLine();
        Console.WriteLine("---------- Compile Error ----------");
      }
      foreach (var diagnostic in collection.Diagnostics) {
        if (diagnostic.Range is TextRange range) {
          source.WriteSourceWithHighlight(range);
        }
        Console.WriteLine($": {diagnostic}");
      }
    }

    // Reads the source code from console. Continues to read if the statement is not complete.
    private static void Read(SourceCode source) {
      source.Reset();
      string line = null;
      while (string.IsNullOrEmpty(line)) {
        line = ReadLine.Read(">>> ").TrimEnd();
      }
      source.AddLine(line);
      while (!source.IsCompleteStatement()) {
        line = ReadLine.Read("... ").TrimEnd();
        source.AddLine(line);
      }
    }
  }
}
