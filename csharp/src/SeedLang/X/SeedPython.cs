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
  // The parser of SeedPython language.
  internal class SeedPython : BaseParser {
    // The dictionary that maps from token types of SeedPython to syntax token types.
    private readonly Dictionary<int, TokenType> _typeMap = new Dictionary<int, TokenType> {
      // The keys are ordered by the constant names defined in the ANTLR-generated source
      // SeedPythonParser.cs. Please keep this dictionary up-to-date once the SeedPython grammar is
      // updated.

      // Ignored: T__0
      // Ignored: T__1
      { SeedPythonParser.IF, TokenType.Keyword },
      { SeedPythonParser.ELIF, TokenType.Keyword },
      { SeedPythonParser.ELSE, TokenType.Keyword },
      { SeedPythonParser.FOR, TokenType.Keyword },
      { SeedPythonParser.IN, TokenType.Keyword },
      { SeedPythonParser.WHILE, TokenType.Keyword },
      { SeedPythonParser.DEF, TokenType.Keyword },
      { SeedPythonParser.RETURN, TokenType.Keyword },
      { SeedPythonParser.PASS, TokenType.Keyword },
      { SeedPythonParser.COLON, TokenType.Symbol },
      { SeedPythonParser.SEMICOLON, TokenType.Symbol },
      { SeedPythonParser.TRUE, TokenType.Boolean },
      { SeedPythonParser.FALSE, TokenType.Boolean },
      { SeedPythonParser.NONE, TokenType.None },
      { SeedPythonParser.AND, TokenType.Operator },
      { SeedPythonParser.OR, TokenType.Operator },
      { SeedPythonParser.NOT, TokenType.Operator },
      { SeedPythonParser.EQUAL, TokenType.Operator },
      { SeedPythonParser.EQ_EQUAL, TokenType.Operator },
      { SeedPythonParser.NOT_EQUAL, TokenType.Operator },
      { SeedPythonParser.LESS_EQUAL, TokenType.Operator },
      { SeedPythonParser.LESS, TokenType.Operator },
      { SeedPythonParser.GREATER_EQUAL, TokenType.Operator },
      { SeedPythonParser.GREATER, TokenType.Operator },
      { SeedPythonParser.ADD, TokenType.Operator },
      { SeedPythonParser.SUBTRACT, TokenType.Operator },
      { SeedPythonParser.MULTIPLY, TokenType.Operator },
      { SeedPythonParser.DIVIDE, TokenType.Operator },
      { SeedPythonParser.FLOOR_DIVIDE, TokenType.Operator },
      { SeedPythonParser.POWER, TokenType.Operator },
      { SeedPythonParser.MODULO, TokenType.Operator },
      { SeedPythonParser.ADD_ASSIGN, TokenType.Operator },
      { SeedPythonParser.SUBSTRACT_ASSIGN, TokenType.Operator },
      { SeedPythonParser.MULTIPLY_ASSIGN, TokenType.Operator },
      { SeedPythonParser.DIVIDE_ASSIGN, TokenType.Operator },
      { SeedPythonParser.MODULO_ASSIGN, TokenType.Operator },
      { SeedPythonParser.OPEN_PAREN, TokenType.OpenParenthesis },
      { SeedPythonParser.CLOSE_PAREN, TokenType.CloseParenthesis },
      { SeedPythonParser.OPEN_BRACK, TokenType.OpenBracket },
      { SeedPythonParser.CLOSE_BRACK, TokenType.CloseBracket },
      // Ignore: OPEN_BRACE
      // Ignore: CLOSE_BRACE
      { SeedPythonParser.DOT, TokenType.Symbol },
      { SeedPythonParser.COMMA, TokenType.Symbol },
      { SeedPythonParser.NAME, TokenType.Variable },
      { SeedPythonParser.NUMBER, TokenType.Number },
      { SeedPythonParser.STRING, TokenType.String },
      { SeedPythonParser.INTEGER, TokenType.Number },
      { SeedPythonParser.DECIMAL_INTEGER, TokenType.Number },
      { SeedPythonParser.FLOAT_NUMBER, TokenType.Number },
      // Ignore: NEWLINE
      // Ignore: SKIP_
      { SeedPythonParser.UNKNOWN_CHAR, TokenType.Unknown },
      // Ignore: INDENT
      // Ignore: DEDENT
    };

    // The dictionary that maps from token types of SeedPython to syntax token types.
    protected override IReadOnlyDictionary<int, TokenType> _syntaxTypeMap => _typeMap;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedPythonDentLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedPythonParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<TokenInfo> tokens) {
      return new SeedPythonVisitor(tokens);
    }

    protected override ParserRuleContext Program(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).program();
    }
  }
}
