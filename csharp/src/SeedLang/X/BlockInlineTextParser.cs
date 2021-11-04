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
  // The parser to parse a block inline text of SeedBlock programs.
  //
  // The BlockInlineTextParser uses generated ANTLR4 SeedBlockInlineTextParser to parse the inline
  // text of block programs.
  internal class BlockInlineTextParser : BaseParser {
    // The dictionary that maps from token types of SeedBlock to syntax token types.
    private readonly Dictionary<int, SyntaxType> _syntaxTypes = new Dictionary<int, SyntaxType> {
      { SeedBlockInlineTextParser.NUMBER, SyntaxType.Number },
      { SeedBlockInlineTextParser.ADD, SyntaxType.Operator },
      { SeedBlockInlineTextParser.SUB, SyntaxType.Operator },
      { SeedBlockInlineTextParser.MUL, SyntaxType.Operator },
      { SeedBlockInlineTextParser.DIV, SyntaxType.Operator },
      { SeedBlockInlineTextParser.OPEN_PAREN, SyntaxType.Parenthesis },
      { SeedBlockInlineTextParser.CLOSE_PAREN, SyntaxType.Parenthesis },
      { SeedBlockInlineTextParser.UNKNOWN_CHAR, SyntaxType.Unknown },
    };

    // The dictionary that maps from token types of SeedBlock to syntax token types.
    protected override IReadOnlyDictionary<int, SyntaxType> _syntaxTypeMap => _syntaxTypes;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedBlockInlineTextLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedBlockInlineTextParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<SyntaxToken> tokens) {
      return new BlockInlineTextVisitor(tokens);
    }

    protected override ParserRuleContext SingleStmt(Parser parser) {
      Debug.Assert(parser is SeedBlockInlineTextParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockInlineTextParser).single_stmt();
    }
  }
}
