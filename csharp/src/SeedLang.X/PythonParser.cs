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

using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;

namespace SeedLang.X {
  // The parser of SeedPython language.
  //
  // The PythonParser provides interfaces to validate the SeedPython source code, and parse it into
  // the AST tree based on the predefined rules.
  public sealed class PythonParser {
    // Validate SeedPython source code based on the parse rule.
    public static bool Validate(string source, string module, ParseRule rule,
                                DiagnosticCollection collection) {
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      SeedPythonParser parser = SetupParser(source, module, localCollection);
      switch (rule) {
        case ParseRule.Identifier:
          parser.single_identifier();
          break;
        case ParseRule.Number:
          parser.single_number();
          break;
        case ParseRule.String:
          parser.single_string();
          break;
        case ParseRule.Expression:
          parser.single_expr();
          break;
        case ParseRule.Statement:
          parser.single_stmt();
          break;
        default:
          Debug.Assert(false, $"Not implemented parse rule: {rule}");
          break;
      }
      return localCollection.Diagnostics.Count == 0;
    }

    // Parses SeedPython source code into the AST tree based on the parse rule.
    public static AstNode Parse(string source, string module, ParseRule rule,
                                DiagnosticCollection collection) {
      SeedPythonParser parser = SetupParser(source, module, collection);
      var visitor = new PythonVisitor();
      switch (rule) {
        case ParseRule.Identifier:
          return visitor.Visit(parser.single_identifier());
        case ParseRule.Number:
          return visitor.Visit(parser.single_number());
        case ParseRule.String:
          return visitor.Visit(parser.single_string());
        case ParseRule.Expression:
          return visitor.Visit(parser.single_expr());
        case ParseRule.Statement:
          return visitor.Visit(parser.single_stmt());
        default:
          Debug.Assert(false, $"Not implemented parse rule: {rule}");
          return null;
      }
    }

    private static SeedPythonParser SetupParser(string source, string module,
                                                DiagnosticCollection collection) {
      var inputStream = new AntlrInputStream(source);
      var lexer = new SeedPythonLexer(inputStream);
      var tokenStream = new CommonTokenStream(lexer);
      var parser = new SeedPythonParser(tokenStream);
      // Removes default error listerners which include a console error reporter.
      parser.RemoveErrorListeners();
      if (!(collection is null)) {
        var errorListener = new SyntaxErrorListener(module, collection);
        parser.AddErrorListener(errorListener);
      }
      return parser;
    }
  }
}
