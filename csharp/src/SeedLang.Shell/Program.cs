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
using CommandLine;
using CommandLine.Text;

namespace SeedLang.Shell {
  internal class Program {
    internal class Options {
      [Option('l', "language", Required = false, Default = SeedXLanguage.SeedPython,
        HelpText = "The programming language of the source code.")]
      public SeedXLanguage Language { get; set; }

      [Option('t', "type", Required = false, Default = RunType.Execute, HelpText = "Run type.")]
      public RunType RunType { get; set; }

      [Option('v', "visualizers", Required = false, Separator = ',',
              HelpText = "The Visualizers to be enabled.\n" +
                         "* Use '--visualizers=Binary,Comparison' or '-v Assignment,Binary' " +
                         "to enable multiple visualizers.\n" +
                         "* Use \"*\" to match any characters in the visualizer name, e.g. " +
                         "'-v \"*\"' for all visualizers, or '-v \"Func*\"' for all visualizers " +
                         "that start with \"Func\".")]
      public IEnumerable<string> Visualizers { get; set; }

      [Option("verbose", Required = false, Default = false,
        HelpText = "Print visualization info in verbose mode.")]
      public bool Verbose { get; set; }

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
        var visualizers = VisualizerManager.CreateVisualizers(options.Visualizers, options.Verbose);
        Console.WriteLine($"Enabled Visualizers: {string.Join(", ", visualizers)}");
        Console.WriteLine();
        Run(options);
      }).WithNotParsed(errors => {
        helpText.AddPostOptionsText(
            $"Valid visualizers: {string.Join(", ", VisualizerManager.EventNames)}");
        Console.Error.WriteLine(helpText);
        Console.WriteLine();
      });
    }

    private static void Run(Options options) {
      if (!string.IsNullOrEmpty(options.Filename)) {
        Executor.RunScript(options.Filename, options.Language, options.RunType);
      } else {
        Executor.Repl(options.Language, options.RunType);
      }
    }
  }
}
