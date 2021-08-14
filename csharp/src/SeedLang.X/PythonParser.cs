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
      if (string.IsNullOrEmpty(source) || module is null) {
        return false;
      }
      var localCollection = collection ?? new DiagnosticCollection();
      int diagnosticCount = localCollection.Diagnostics.Count;
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
      return localCollection.Diagnostics.Count == diagnosticCount;
    }

    // Parses SeedPython source code into the AST tree based on the parse rule. Returns null if
    // any parsing error happens.
    public static AstNode Parse(string source, string module, ParseRule rule,
                                DiagnosticCollection collection) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return null;
      }
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      int diagnosticCount = localCollection.Diagnostics.Count;
      SeedPythonParser parser = SetupParser(source, module, localCollection);
      var visitor = new PythonVisitor();
      AstNode node = null;
      switch (rule) {
        case ParseRule.Identifier:
          node = visitor.Visit(parser.single_identifier());
          break;
        case ParseRule.Number:
          node = visitor.Visit(parser.single_number());
          break;
        case ParseRule.String:
          node = visitor.Visit(parser.single_string());
          break;
        case ParseRule.Expression:
          node = visitor.Visit(parser.single_expr());
          break;
        case ParseRule.Statement:
          node = visitor.Visit(parser.single_stmt());
          break;
        default:
          Debug.Assert(false, $"Unsupported parse rule: {rule}");
          break;
      }
      return localCollection.Diagnostics.Count > diagnosticCount ? null : node;
    }

    private static SeedPythonParser SetupParser(string source, string module,
                                                DiagnosticCollection collection) {
      var inputStream = new AntlrInputStream(source);
      var lexer = new SeedPythonLexer(inputStream);
      var tokenStream = new CommonTokenStream(lexer);
      var parser = new SeedPythonParser(tokenStream);
      // Removes default error listerners which include a console error reporter.
      lexer.RemoveErrorListeners();
      parser.RemoveErrorListeners();
      if (!(collection is null)) {
        parser.ErrorHandler = new SyntaxErrorStrategy(module, collection);
      }
      return parser;
    }
  }
}
