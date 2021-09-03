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

using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;

namespace SeedLang.X {
  // The parser of SeedPython language.
  //
  // The BlockParser inherits the interfaces of BaseParser.
  internal class PythonParser : BaseParser {
    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedPythonLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedPythonParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor() {
      return new PythonVisitor();
    }

    protected override ParserRuleContext SingleExpr(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).single_expr();
    }

    protected override ParserRuleContext SingleIdentifier(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).single_identifier();
    }

    protected override ParserRuleContext SingleNumber(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).single_number();
    }

    protected override ParserRuleContext SingleStmt(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).single_stmt();
    }

    protected override ParserRuleContext SingleString(Parser parser) {
      Debug.Assert(parser is SeedPythonParser, $"Incorrect parser type: {parser}");
      return (parser as SeedPythonParser).single_string();
    }
  }
}
