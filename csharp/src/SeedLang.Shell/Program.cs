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
using System.IO;
using CommandLine;
using CommandLine.Text;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Shell {
  internal class Program {
    internal class Options {
      [Option('l', "language", Required = false, Default = SeedXLanguage.SeedPython,
        HelpText = "The programming language of the source code.")]
      public SeedXLanguage Language { get; set; }

      [Option('t', "type", Required = false, Default = RunType.Bytecode, HelpText = "Run type.")]
      public RunType RunType { get; set; }

      [Option('v', "visualizers", Required = false, Separator = ',',
              HelpText = "The Visualizers to be enabled. " +
                         "Valid values: Assignment, Binary, Boolean, Comparison, Unary, All.\n" +
                         "* Use \"--visualizers=Binary,Comparison\" or \"-v Assignment,Binary\" " +
                         "to enable multiple visualizers.\n" +
                         "* Use \"-v All\" to enable all visualizers.")]
      public IEnumerable<VisualizerType> VisualizerTypes { get; set; }

      [Option('f', "file", Required = false, Default = null,
              HelpText = "Path of the file to be executed.")]
      public string Filename { get; set; }
    }

    static void Main(string[] args) {
      var parser = new Parser(with => with.HelpWriter = null);
      var result = parser.ParseArguments<Options>(args);
      var helpText = HelpText.AutoBuild(result, h => {
        h.AddEnumValuesToHelpText = true;
        return HelpText.DefaultParsingErrorsHandler(result, h);
      }, e => e);
      result.WithParsed(options => {
        Console.WriteLine(helpText.Heading);
        Console.WriteLine(helpText.Copyright);
        Console.WriteLine();
        Run(options);
      }).WithNotParsed(errors => {
        Console.Error.Write(helpText);
      });
    }

    private static void Run(Options options) {
      if (!string.IsNullOrEmpty(options.Filename)) {
        RunScript(options.Filename, options.Language, options.RunType, options.VisualizerTypes);
      } else {
        var repl = new Repl(options.Language, options.RunType, options.VisualizerTypes);
        repl.Execute();
      }
    }

    private static void RunScript(string filename, SeedXLanguage language, RunType runType,
                                  IEnumerable<VisualizerType> visualizerTypes) {
      var source = new SourceCode();
      try {
        foreach (string line in File.ReadLines(filename)) {
          source.AddLine(line);
        }
      } catch (Exception ex) {
        Console.WriteLine($"Read file error: {ex}.");
        return;
      }
      Executor.ParseSyntaxTokens(source.Source, "", language,
                                 out IReadOnlyList<TokenInfo> syntaxTokens);
      source.WriteSourceWithSyntaxTokens(syntaxTokens);

      Console.WriteLine();
      Console.WriteLine("---------- Run ----------");
      var visualizerManager = new VisualizerManager(source, visualizerTypes);
      var executor = new Executor();
      visualizerManager.RegisterToExecutor(executor);
      var collection = new DiagnosticCollection();

      string result = executor.Run(source.Source, "", language, runType, RunMode.Script,
                                   collection);
      if (!(result is null)) {
        Console.WriteLine(result);
      }

      foreach (var diagnostic in collection.Diagnostics) {
        if (diagnostic.Range is TextRange range) {
          source.WriteSourceWithHighlight(range);
        }
        Console.WriteLine($": {diagnostic}");
      }
      visualizerManager.UnregisterFromExecutor(executor);
    }
  }
}
