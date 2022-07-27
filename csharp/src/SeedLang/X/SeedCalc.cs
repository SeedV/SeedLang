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
  // The parser of SeedCalc language.
  internal class SeedCalc : BaseParser {
    // The dictionary that maps from ANTLR4 token types of SeedCalc to syntax token types.
    private readonly Dictionary<int, TokenType> _typeMap = new Dictionary<int, TokenType> {
      // The keys are ordered by the constant names defined in the ANTLR-generated source
      // SeedCalcParser.cs. Please keep this dictionary up-to-date once the grammar is updated.
      { SeedCalcParser.ADD, TokenType.Operator },
      { SeedCalcParser.SUBTRACT, TokenType.Operator },
      { SeedCalcParser.MULTIPLY, TokenType.Operator },
      { SeedCalcParser.DIVIDE, TokenType.Operator },
      { SeedCalcParser.OPEN_PAREN, TokenType.OpenParenthesis },
      { SeedCalcParser.CLOSE_PAREN, TokenType.CloseParenthesis },
      { SeedCalcParser.NUMBER, TokenType.Number },
      { SeedCalcParser.INTEGER, TokenType.Number },
      { SeedCalcParser.DECIMAL_INTEGER, TokenType.Number },
      { SeedCalcParser.FLOAT_NUMBER, TokenType.Number },
      // Ignore: NEWLINE
      // Ignore: SKIP_
      { SeedCalcParser.UNKNOWN_CHAR, TokenType.Unknown },
    };

    // The dictionary that maps from token types of SeedCalc to syntax token types.
    protected override IReadOnlyDictionary<int, TokenType> _syntaxTypeMap => _typeMap;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedCalcLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedCalcParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<TokenInfo> tokens) {
      return new SeedCalcVisitor(tokens);
    }

    protected override ParserRuleContext Program(Parser parser) {
      Debug.Assert(parser is SeedCalcParser, $"Incorrect parser type: {parser}");
      return (parser as SeedCalcParser).expression_stmt();
    }
  }
}
