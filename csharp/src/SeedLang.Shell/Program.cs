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
using CommandLine;
using CommandLine.Text;
using SeedLang.Common;

namespace SeedLang.Shell {
  class Program {
    class Options {
      [Option('r', "rule", Required = false, Default = "statement",
        HelpText = "The parse rule of the source code."
                   + " Allowed values are statement, expression, number or identifier.")]
      public string Rule { get; set; }

      [Option('l', "language", Required = false, Default = "python",
        HelpText =
          "The language of the source code. Allowed values are python, lua or javascript.")]
      public string Language { get; set; }

      [Option('d', "dryrun", Required = false,
        HelpText = "Verify the source code without running it.")]
      public bool Dryrun { get; set; }
    }

    static void Main(string[] args) {
      var parserResult = Parser.Default.ParseArguments<Options>(args);
      var helpText = HelpText.AutoBuild<Options>(parserResult, h => h, e => e);
      parserResult.WithParsed(options => {
        Console.Error.WriteLine(helpText.Heading);
        Console.Error.WriteLine(helpText.Copyright);
        Console.Error.WriteLine();
        RunOptions(options);
      });
    }

    private static void RunOptions(Options options) {
      // TODO: add a REPL class to encapsulate the input and execution process.
      while (true) {
        Console.Write("> ");
        string line = Console.ReadLine();
        if (line == "quit") {
          break;
        }
        var collection = new DiagnosticCollection();
        if (!Engine.Instance.Dryrun(line,
                                    "",
                                    ParseRule.Expression,
                                    Language.Python,
                                    collection)) {
          foreach (var diagnostic in collection.Diagnostics) {
            Console.WriteLine(diagnostic);
          }
        }
      }
    }
  }
}
