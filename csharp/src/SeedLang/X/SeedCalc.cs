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
    // The dictionary that maps from token types of SeedCalc to syntax token types.
    private readonly Dictionary<int, SyntaxType> _syntaxTypes = new Dictionary<int, SyntaxType> {
      { SeedCalcParser.NUMBER, SyntaxType.Number },
      { SeedCalcParser.ADD, SyntaxType.Operator },
      { SeedCalcParser.SUBTRACT, SyntaxType.Operator },
      { SeedCalcParser.MULTIPLY, SyntaxType.Operator },
      { SeedCalcParser.DIVIDE, SyntaxType.Operator },
      { SeedCalcParser.OPEN_PAREN, SyntaxType.Parenthesis },
      { SeedCalcParser.CLOSE_PAREN, SyntaxType.Parenthesis },
      { SeedCalcParser.UNKNOWN_CHAR, SyntaxType.Unknown },
    };

    // The dictionary that maps from token types of SeedCalc to syntax token types.
    protected override IReadOnlyDictionary<int, SyntaxType> _syntaxTypeMap => _syntaxTypes;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedCalcLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedCalcParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<SyntaxToken> tokens) {
      return new SeedCalcVisitor(tokens);
    }

    protected override ParserRuleContext Program(Parser parser) {
      Debug.Assert(parser is SeedCalcParser, $"Incorrect parser type: {parser}");
      return (parser as SeedCalcParser).expressionStatement();
    }
  }
}
