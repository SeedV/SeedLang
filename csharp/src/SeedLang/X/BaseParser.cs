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
  // The abstract base class of the block parser and all SeedX language parsers.
  //
  // It provides interfaces to validate the text source code, and parse it into an AST tree based on
  // the predefined rules.
  internal abstract class BaseParser {
    // Validates source code based on the parse rule. The concrete ANTLR4 lexer and parser are
    // created by the derived class.
    internal bool Validate(string source, string module, ParseRule rule,
                           DiagnosticCollection collection) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return false;
      }
      var localCollection = collection ?? new DiagnosticCollection();
      int diagnosticCount = localCollection.Diagnostics.Count;
      Parser parser = SetupParser(source, module, localCollection);
      GetContext(parser, rule);
      return localCollection.Diagnostics.Count == diagnosticCount;
    }

    // Parses source code into an AST tree based on the parse rule. The concrete ANTLR4 lexer and
    // parser are created by the derived class. The out node is set to null if the given source code
    // is not valid.
    internal bool TryParse(string source, string module, ParseRule rule,
                           DiagnosticCollection collection, out AstNode node) {
      if (string.IsNullOrEmpty(source) || module is null) {
        node = null;
        return false;
      }
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      int diagnosticCount = localCollection.Diagnostics.Count;
      Parser parser = SetupParser(source, module, localCollection);
      ParserRuleContext context = GetContext(parser, rule);
      var visitor = MakeVisitor();
      node = visitor.Visit(context);
      if (localCollection.Diagnostics.Count > diagnosticCount) {
        node = null;
      }
      return localCollection.Diagnostics.Count == diagnosticCount;
    }

    protected abstract Lexer MakeLexer(ICharStream stream);

    protected abstract Parser MakeParser(ITokenStream stream);

    protected abstract AbstractParseTreeVisitor<AstNode> MakeVisitor();

    protected abstract ParserRuleContext SingleIdentifier(Parser parser);

    protected abstract ParserRuleContext SingleNumber(Parser parser);

    protected abstract ParserRuleContext SingleString(Parser parser);

    protected abstract ParserRuleContext SingleExpr(Parser parser);

    protected abstract ParserRuleContext SingleStmt(Parser parser);

    protected Lexer SetupLexer(string source) {
      var inputStream = new AntlrInputStream(source);
      var lexer = MakeLexer(inputStream);
      lexer.RemoveErrorListeners();
      return lexer;
    }

    protected Parser SetupParser(string source, string module, DiagnosticCollection collection) {
      var lexer = SetupLexer(source);
      var tokenStream = new CommonTokenStream(lexer);
      Parser parser = MakeParser(tokenStream);
      // Removes default error listerners which include a console error reporter.
      parser.RemoveErrorListeners();
      if (!(collection is null)) {
        parser.ErrorHandler = new SyntaxErrorStrategy(module, collection);
      }
      return parser;
    }

    protected ParserRuleContext GetContext(Parser parser, ParseRule rule) {
      switch (rule) {
        case ParseRule.Identifier:
          return SingleIdentifier(parser);
        case ParseRule.Number:
          return SingleNumber(parser);
        case ParseRule.String:
          return SingleString(parser);
        case ParseRule.Expression:
          return SingleExpr(parser);
        case ParseRule.Statement:
          return SingleStmt(parser);
        default:
          Debug.Assert(false, $"Not implemented parse rule: {rule}");
          return null;
      }
    }
  }
}
