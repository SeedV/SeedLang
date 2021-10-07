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
  internal class PythonParser : BaseParser {
    private readonly Dictionary<int, SyntaxType> _syntaxTypes = new Dictionary<int, SyntaxType> {
      { SeedPythonParser.EVAL, SyntaxType.Keyword},
      { SeedPythonParser.IDENTIFIER, SyntaxType.Variable},
      { SeedPythonParser.NUMBER, SyntaxType.Number},
      { SeedPythonParser.ADD, SyntaxType.Operator},
      { SeedPythonParser.SUB, SyntaxType.Operator},
      { SeedPythonParser.MUL, SyntaxType.Operator},
      { SeedPythonParser.DIV, SyntaxType.Operator},
      { SeedPythonParser.EQUAL, SyntaxType.Operator},
      { SeedPythonParser.OPEN_PAREN, SyntaxType.Parenthesis},
      { SeedPythonParser.CLOSE_PAREN, SyntaxType.Parenthesis},
      { SeedPythonParser.UNKNOWN_CHAR, SyntaxType.Unknown },
    };

    protected override IReadOnlyDictionary<int, SyntaxType> _syntaxTypeMapping => _syntaxTypes;

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedPythonLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedPythonParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor(IList<SyntaxToken> tokens) {
      return new PythonVisitor(tokens);
    }

    protected override ParserRuleContext SingleStmt(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).single_stmt();
    }
  }
}
