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
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;

namespace SeedLang.X {
  // The parser to parse a block inline text of SeedBlock programs.
  internal class SeedBlockInlineText : BaseParser {
    // The dictionary that maps from token types of SeedBlock to syntax token types.
    private readonly Dictionary<int, TokenType> _typeMap = new Dictionary<int, TokenType> {
      // The keys are ordered by the constant names defined in the ANTLR-generated source
      // SeedBlockInlineTextParser.cs. Please keep this dictionary up-to-date once the SeedPython
      // grammar is updated.
      { SeedBlockInlineTextParser.TRUE, TokenType.Boolean},
      { SeedBlockInlineTextParser.FALSE, TokenType.Boolean},
      { SeedBlockInlineTextParser.NONE, TokenType.None},
      { SeedBlockInlineTextParser.AND, TokenType.Operator},
      { SeedBlockInlineTextParser.OR, TokenType.Operator},
      { SeedBlockInlineTextParser.NOT, TokenType.Operator},
      { SeedBlockInlineTextParser.EQUAL, TokenType.Operator},
      { SeedBlockInlineTextParser.EQ_EQUAL, TokenType.Operator},
      { SeedBlockInlineTextParser.NOT_EQUAL, TokenType.Operator},
      { SeedBlockInlineTextParser.LESS_EQUAL, TokenType.Operator},
      { SeedBlockInlineTextParser.LESS, TokenType.Operator},
      { SeedBlockInlineTextParser.GREATER_EQUAL, TokenType.Operator},
      { SeedBlockInlineTextParser.GREATER, TokenType.Operator},
      { SeedBlockInlineTextParser.ADD, TokenType.Operator},
      { SeedBlockInlineTextParser.SUBTRACT, TokenType.Operator},
      { SeedBlockInlineTextParser.MULTIPLY, TokenType.Operator},
      { SeedBlockInlineTextParser.DIVIDE, TokenType.Operator},
      { SeedBlockInlineTextParser.FLOOR_DIVIDE, TokenType.Operator},
      { SeedBlockInlineTextParser.POWER, TokenType.Operator},
      { SeedBlockInlineTextParser.MODULO, TokenType.Operator},
      { SeedBlockInlineTextParser.ADD_ASSIGN, TokenType.Operator},
      { SeedBlockInlineTextParser.SUBSTRACT_ASSIGN, TokenType.Operator},
      { SeedBlockInlineTextParser.MULTIPLY_ASSIGN, TokenType.Operator},
      { SeedBlockInlineTextParser.DIVIDE_ASSIGN, TokenType.Operator},
      { SeedBlockInlineTextParser.MODULO_ASSIGN, TokenType.Operator},
      { SeedBlockInlineTextParser.OPEN_PAREN, TokenType.OpenParenthesis},
      { SeedBlockInlineTextParser.CLOSE_PAREN, TokenType.CloseParenthesis},
      { SeedBlockInlineTextParser.OPEN_BRACK, TokenType.OpenBracket},
      { SeedBlockInlineTextParser.CLOSE_BRACK, TokenType.CloseBracket},
      // Ignore: OPEN_BRACE
      // Ignore: CLOSE_BRACE
      { SeedBlockInlineTextParser.DOT, TokenType.Symbol},
      { SeedBlockInlineTextParser.COMMA, TokenType.Symbol},
      { SeedBlockInlineTextParser.NAME, TokenType.Variable},
      { SeedBlockInlineTextParser.NUMBER, TokenType.Number},
      { SeedBlockInlineTextParser.STRING, TokenType.String},
      { SeedBlockInlineTextParser.INTEGER, TokenType.Number },
      { SeedBlockInlineTextParser.DECIMAL_INTEGER, TokenType.Number },
      { SeedBlockInlineTextParser.FLOAT_NUMBER, TokenType.Number },
      // Ignore: NEWLINE
      // Ignore: SKIP_
      { SeedBlockInlineTextParser.UNKNOWN_CHAR, TokenType.Unknown },
    };

    // The dictionary that maps from token types of SeedBlock to syntax token types.
    protected override IReadOnlyDictionary<int, TokenType> _syntaxTypeMap => _typeMap;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedBlockInlineTextLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedBlockInlineTextParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<TokenInfo> tokens) {
      return new SeedBlockInlineTextVisitor(tokens);
    }

    protected override ParserRuleContext Program(Parser parser) {
      Debug.Assert(parser is SeedBlockInlineTextParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockInlineTextParser).expressionStatement();
    }
  }
}
