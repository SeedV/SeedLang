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
using CommandLine;
using CommandLine.Text;
using SeedLang.Runtime;

namespace SeedLang.Shell {
  internal enum VisualizerType {
    Assignment,
    Binary,
    Comparison,
    Eval,
  }

  internal class Program {
    internal class Options {
      [Option('l', "language", Required = false, Default = SeedXLanguage.SeedPython,
        HelpText = "The programming language of the source code.")]
      public SeedXLanguage Language { get; set; }

      [Option('t', "type", Required = false, Default = RunType.Ast, HelpText = "Run type.")]
      public RunType RunType { get; set; }

      [Option('v', "visualizers", Required = false,
              Default = new VisualizerType[] { VisualizerType.Eval },
              HelpText = "Enabled Visualizers")]
      public IEnumerable<VisualizerType> VisualizerTypes { get; set; }
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
        Console.Error.WriteLine(helpText);
      });
    }

    private static void Run(Options options) {
      var repl = new Repl(options.Language, options.RunType, options.VisualizerTypes);
      repl.Execute();
    }
  }
}
