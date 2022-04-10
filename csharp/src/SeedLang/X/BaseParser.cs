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
    // The dictionary that maps from ANTLR4 token types to syntax token types.
    protected abstract IReadOnlyDictionary<int, TokenType> _syntaxTypeMap { get; }

    // Validates source code based on the parse rule. The concrete ANTLR4 lexer and parser are
    // created by the derived class.
    internal bool Validate(string source, string module, DiagnosticCollection collection) {
      int diagnosticCount = collection.Diagnostics.Count;
      Parser parser = SetupParser(source, module, collection);
      Program(parser);
      return collection.Diagnostics.Count == diagnosticCount;
    }

    // Parses source code into syntax tokens. The concrete ANTLR4 lexer and parser are created by
    // the derived class.
    internal void ParseSyntaxTokens(string source, out IReadOnlyList<TokenInfo> tokens) {
      var tokenList = new List<TokenInfo>();
      Lexer lexer = SetupLexer(source);
      IList<IToken> lexerTokens = lexer.GetAllTokens();
      foreach (var lexerToken in lexerTokens) {
        if (_syntaxTypeMap.ContainsKey(lexerToken.Type)) {
          TextRange range = CodeReferenceUtils.RangeOfToken(lexerToken);
          var token = new TokenInfo(_syntaxTypeMap[lexerToken.Type], range);
          tokenList.Add(token);
        }
      }
      tokens = tokenList;
      return;
    }

    // Parses a valid source code into an AST tree and a list of semantic tokens. The concrete
    // ANTLR4 lexer and parser are created by the derived class. The out parameters will be set to
    // null if the given source code is not valid.
    internal bool Parse(string source, string module, DiagnosticCollection collection,
                        out AstNode node, out IReadOnlyList<TokenInfo> semanticTokens) {
      int diagnosticCount = collection.Diagnostics.Count;
      Parser parser = SetupParser(source, module, collection);
      var tokenList = new List<TokenInfo>();
      semanticTokens = tokenList;
      AbstractParseTreeVisitor<AstNode> visitor = MakeVisitor(tokenList);
      ParserRuleContext program = Program(parser);
      if (collection.Diagnostics.Count > diagnosticCount) {
        node = null;
        semanticTokens = null;
        return false;
      }
      try {
        // The visitor will throw an overflow runtime exception if any constant number in the source
        // code is overflowed.
        node = visitor.Visit(program);
        return true;
      } catch (DiagnosticException e) {
        collection.Report(e.Diagnostic);
        node = null;
        semanticTokens = null;
        return false;
      }
    }

    protected abstract Lexer MakeLexer(ICharStream stream);

    protected abstract Parser MakeParser(ITokenStream stream);

    protected abstract AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<TokenInfo> tokens);

    // Returns the parser rule context of a program.
    protected abstract ParserRuleContext Program(Parser parser);

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
  }
}
