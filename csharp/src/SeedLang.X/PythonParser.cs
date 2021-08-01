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
using Antlr4.Runtime;
using SeedLang.Ast;
using SeedLang.Common;

namespace SeedLang.X {
  // The parser of SeedPython language.
  //
  // The PythonParser provides interfaces to validate the SeedPython source code, and parse it into
  // the AST tree based on the predefined rules.
  public sealed class PythonParser {
    // Parses SeedPython source code into the AST tree based on the parse rule.
    public static AstNode Parse(string source, ParseRule rule, DiagnosticCollection diagnostics) {
      var inputStream = new AntlrInputStream(source);
      var lexer = new SeedPythonLexer(inputStream);
      var tokenStream = new CommonTokenStream(lexer);
      var parser = new SeedPythonParser(tokenStream);

      // Remove default error listerners which include a console error reporter.
      parser.RemoveErrorListeners();
      var errorListener = new SyntaxErrorListener(diagnostics);
      parser.AddErrorListener(errorListener);
      var visitor = new PythonVisitor();

      switch (rule) {
        case ParseRule.Expression:
          return visitor.Visit(parser.expr());
        case ParseRule.Statement:
          return visitor.Visit(parser.stmt());
        default:
          throw new ArgumentException("Unknown parse rule: " + rule);
      }
    }
  }
}
