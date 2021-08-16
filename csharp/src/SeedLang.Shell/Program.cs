﻿// Copyright 2021 The Aha001 Team.
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
  internal class Program {
    internal class Options {
      [Option('l', "language", Required = false, Default = ProgrammingLanguage.Python,
        HelpText = "The programming language of the source code.")]
      public ProgrammingLanguage Language { get; set; }

      [Option('r', "rule", Required = false, Default = ParseRule.Statement,
        HelpText = "The parse rule of the source code.")]
      public ParseRule ParseRule { get; set; }

      [Option('t', "type", Required = false, Default = RunType.Ast,
        HelpText = "Run type.")]
      public RunType RunType { get; set; }
    }

    static void Main(string[] args) {
      var parser = new CommandLine.Parser(with => with.HelpWriter = null);
      var result = parser.ParseArguments<Options>(args);
      var helpText = HelpText.AutoBuild(result, h => {
        h.AddEnumValuesToHelpText = true;
        return HelpText.DefaultParsingErrorsHandler(result, h);
      }, e => e);
      result.WithParsed(options => {
        Console.Error.WriteLine(helpText.Heading);
        Console.Error.WriteLine(helpText.Copyright);
        Console.Error.WriteLine();
        Run(options);
      }).WithNotParsed(errors => {
        Console.Error.WriteLine(helpText);
      });
    }

    private static void Run(Options options) {
      var repl = new Repl(options.Language, options.ParseRule, options.RunType);
      repl.Execute();
    }
  }
}
