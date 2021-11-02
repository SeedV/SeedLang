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
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;

namespace SeedLang.X {
  // The abstract base class of all SeedX language parsers and the SeedBlock inline text parser.
  //
  // It provides interfaces to validate the source code, and parse it into an AST tree based on the
  // predefined rules.
  internal abstract class BaseParser {
    // The dictionary that maps from token types to syntax token types.
    protected abstract IReadOnlyDictionary<int, SyntaxType> _syntaxTypeMap { get; }

    // Validates source code based on the parse rule. The concrete ANTLR4 lexer and parser are
    // created by the derived class.
    internal bool Validate(string source, string module, DiagnosticCollection collection) {
      int diagnosticCount = collection.Diagnostics.Count;
      Parser parser = SetupParser(source, module, collection);
      SingleStmt(parser);
      return collection.Diagnostics.Count == diagnosticCount;
    }

    // Parses source code into an AST tree based on the parse rule. The concrete ANTLR4 lexer and
    // parser are created by the derived class. The out node is set to null if the given source code
    // is not valid.
    internal bool Parse(string source, string module, DiagnosticCollection collection,
                        out AstNode node, out IReadOnlyList<SyntaxToken> tokens) {
      int diagnosticCount = collection.Diagnostics.Count;
      Parser parser = SetupParser(source, module, collection);
      var tokenList = new List<SyntaxToken>();
      tokens = tokenList;
      AbstractParseTreeVisitor<AstNode> visitor = MakeVisitor(tokenList);
      node = visitor.Visit(SingleStmt(parser));
      if (collection.Diagnostics.Count > diagnosticCount) {
        ParseMissingSyntaxTokens(source, tokenList);
        node = null;
        return false;
      }
      return true;
    }

    protected abstract Lexer MakeLexer(ICharStream stream);

    protected abstract Parser MakeParser(ITokenStream stream);

    protected abstract AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<SyntaxToken> tokens);

    // Returns the parser rule context of the single statement. This method must be implemented by
    // the derived class.
    protected virtual ParserRuleContext SingleStmt(Parser parser) {
      throw new NotImplementedException();
    }

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

    // Parses missing syntax tokens from all lexer tokens.
    //
    // Normally syntax tokens are collected during parsing based on the syntax meaning of tokens.
    // But there will be some syntax tokens missing if syntax errors happen during parsing. Adds
    // these missing tokens from all lexer tokens.
    private void ParseMissingSyntaxTokens(string source, IList<SyntaxToken> tokens) {
      Lexer lexer = SetupLexer(source);
      IList<IToken> lexerTokens = lexer.GetAllTokens();
      for (int i = 0; i < lexerTokens.Count; ++i) {
        if (_syntaxTypeMap.ContainsKey(lexerTokens[i].Type)) {
          TextRange range = CodeReferenceUtils.RangeOfToken(lexerTokens[i]);
          var syntaxToken = new SyntaxToken(_syntaxTypeMap[lexerTokens[i].Type], range);
          if (i >= tokens.Count) {
            tokens.Add(syntaxToken);
          } else if (tokens[i].Range != range) {
            tokens.Insert(i, syntaxToken);
          }
        }
      }
    }
  }
}
