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

namespace SeedLang.X {
  // The parser of SeedPython language.
  internal class SeedPython : BaseParser {
    // The dictionary that maps from token types of SeedPython to syntax token types.
    private readonly Dictionary<int, SyntaxType> _syntaxTypes = new Dictionary<int, SyntaxType> {
      { SeedPythonParser.NAME, SyntaxType.Variable },
      { SeedPythonParser.NUMBER, SyntaxType.Number },
      { SeedPythonParser.ADD, SyntaxType.Operator },
      { SeedPythonParser.SUBTRACT, SyntaxType.Operator },
      { SeedPythonParser.MULTIPLY, SyntaxType.Operator },
      { SeedPythonParser.DIVIDE, SyntaxType.Operator },
      { SeedPythonParser.FLOOR_DIVIDE, SyntaxType.Operator },
      { SeedPythonParser.POWER, SyntaxType.Operator },
      { SeedPythonParser.MODULO, SyntaxType.Operator },
      { SeedPythonParser.EQUAL, SyntaxType.Operator },
      { SeedPythonParser.EQ_EQUAL, SyntaxType.Operator },
      { SeedPythonParser.NOT_EQUAL, SyntaxType.Operator },
      { SeedPythonParser.LESS_EQUAL, SyntaxType.Operator },
      { SeedPythonParser.LESS, SyntaxType.Operator },
      { SeedPythonParser.GREATER_EQUAL, SyntaxType.Operator },
      { SeedPythonParser.GREATER, SyntaxType.Operator },
      { SeedPythonParser.OPEN_PAREN, SyntaxType.Parenthesis },
      { SeedPythonParser.CLOSE_PAREN, SyntaxType.Parenthesis },
      { SeedPythonParser.COLON, SyntaxType.Symbol },
      { SeedPythonParser.UNKNOWN_CHAR, SyntaxType.Unknown },
    };

    // The dictionary that maps from token types of SeedPython to syntax token types.
    protected override IReadOnlyDictionary<int, SyntaxType> _syntaxTypeMap => _syntaxTypes;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedPythonDentLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedPythonParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<SyntaxToken> tokens) {
      return new SeedPythonVisitor(tokens);
    }

    protected override ParserRuleContext Program(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).program();
    }
  }
}
