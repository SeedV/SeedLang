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
using System.Diagnostics;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.X;

namespace SeedLang {
  // A facade class to implement the SeedLang engine interface.
  //
  // This is a singleton class. The interfaces include validating and running a SeedBlock or SeedX
  // program, registering visualizers to visualize program execution, etc.
  public sealed class Engine : IEngine {
    private static readonly Lazy<IEngine> _lazyInstance = new Lazy<IEngine>(() => new Engine());

    public static IEngine Instance => _lazyInstance.Value;

    private readonly VisualizerCenter _visualizerCenter = new VisualizerCenter();

    // TODO: check the lifetime of the executor. It shall not be as long as the engine, but need be
    // longer than a run method.
    private readonly Executor _executor;

    private Engine() {
      _executor = new Executor(_visualizerCenter);
    }

    public void Register<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Register(visualizer);
    }

    public void Unregister<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Unregister(visualizer);
    }

    public bool Run(string source, string module, ProgrammingLanguage language, ParseRule rule,
                    RunType runType, DiagnosticCollection collection = null) {
      if (runType == RunType.DryRun) {
        switch (language) {
          case ProgrammingLanguage.Python:
            return PythonParser.Validate(source, module, rule, collection);
          default:
            Debug.Assert(false, $"Not implemented SeedX language: {language}");
            return false;
        }
      }

      AstNode node = null;
      switch (language) {
        case ProgrammingLanguage.Python:
          node = PythonParser.Parse(source, module, rule, collection);
          break;
        default:
          Debug.Assert(false, $"Not implemented SeedX language: {language}");
          break;
      }
      if (!(node is null)) {
        _executor.Run(node);
        return true;
      }
      return false;
    }
  }
}
