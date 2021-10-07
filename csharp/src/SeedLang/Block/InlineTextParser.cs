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

using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.X;

namespace SeedLang.Block {
  // The parser to parse an inline text of SeedBlock programs.
  //
  // The InlineTextParser used generated ANTLR4 SeedBlockParser to parse the inline text of block
  // programs.
  internal class InlineTextParser : BaseParser {
    private readonly Dictionary<int, SyntaxType> _syntaxTypes = new Dictionary<int, SyntaxType> {
      { SeedBlockParser.NUMBER, SyntaxType.Number },
      { SeedBlockParser.ADD, SyntaxType.Operator },
      { SeedBlockParser.SUB, SyntaxType.Operator },
      { SeedBlockParser.MUL, SyntaxType.Operator },
      { SeedBlockParser.DIV, SyntaxType.Operator },
      { SeedBlockParser.OPEN_PAREN, SyntaxType.Parenthesis },
      { SeedBlockParser.CLOSE_PAREN, SyntaxType.Parenthesis },
      { SeedBlockParser.UNKNOWN_CHAR, SyntaxType.Unknown },
    };

    protected override IReadOnlyDictionary<int, SyntaxType> _syntaxTypeMapping => _syntaxTypes;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedBlockLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedBlockParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<SyntaxToken> tokens) {
      return new InlineTextVisitor(tokens);
    }

    protected override ParserRuleContext SingleExpr(Parser parser) {
      Debug.Assert(parser is SeedBlockParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockParser).single_expr();
    }

    protected override ParserRuleContext SingleIdentifier(Parser parser) {
      Debug.Assert(parser is SeedBlockParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockParser).single_identifier();
    }

    protected override ParserRuleContext SingleNumber(Parser parser) {
      Debug.Assert(parser is SeedBlockParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockParser).single_number();
    }

    protected override ParserRuleContext SingleString(Parser parser) {
      Debug.Assert(parser is SeedBlockParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockParser).single_string();
    }
  }
}
