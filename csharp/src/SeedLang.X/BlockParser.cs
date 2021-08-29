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
using SeedLang.Common;

namespace SeedLang.X {
  // The text parser of SeedBlock language.
  //
  // The BlockParser inherits the interfaces of BaseParser and provides an additional interface to
  // visit expression source code of block programs.
  public class BlockParser : BaseParser {
    // The listener interface to be notified when the tokens of expression source code are visited.
    public interface IExpressionListener {
      void VisitArithmeticOperator(string op);
      void VisitIdentifier(string name);
      void VisitNumber(string number);
      void VisitString(string str);
      void VisitOpenParen();
      void VisitCloseParen();
    }

    // Visits expression source code of block programs. The given listener is notified when each
    // token of the expression is visited. The negative sign token will be combined with the
    // following number to form a negative number.
    public void VisitExpression(string source, IExpressionListener listener) {
      if (!Validate(source, "", ParseRule.Expression, null)) {
        return;
      }
      Lexer lexer = SetupLexer(source);
      int lastTokenType = SeedBlockParser.UNKNOWN_CHAR;
      bool negative = false;
      foreach (var token in lexer.GetAllTokens()) {
        switch (token.Type) {
          case SeedBlockLexer.ADD:
          case SeedBlockLexer.MUL:
          case SeedBlockLexer.DIV:
            listener.VisitArithmeticOperator(token.Text);
            break;
          case SeedBlockLexer.SUB:
            if (lastTokenType != SeedBlockParser.NUMBER) {
              negative = true;
            } else {
              listener.VisitArithmeticOperator(token.Text);
            }
            break;
          case SeedBlockLexer.IDENTIFIER:
            listener.VisitIdentifier(token.Text);
            break;
          case SeedBlockLexer.NUMBER:
            listener.VisitNumber((negative ? "-" : "") + token.Text);
            negative = false;
            break;
          case SeedBlockLexer.STRING:
            listener.VisitString(token.Text);
            break;
          case SeedBlockLexer.OPEN_PAREN:
            listener.VisitOpenParen();
            break;
          case SeedBlockLexer.CLOSE_PAREN:
            listener.VisitCloseParen();
            break;
          default:
            break;
        }
        lastTokenType = token.Type;
      }
    }

    protected override Lexer MakeLexer(ICharStream stream) {
      return new SeedBlockLexer(stream);
    }

    protected override Parser MakeParser(ITokenStream stream) {
      return new SeedBlockParser(stream);
    }

    protected override AbstractParseTreeVisitor<AstNode> MakeVisitor() {
      return new BlockVisitor();
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

    protected override ParserRuleContext SingleStmt(Parser parser) {
      Debug.Assert(parser is SeedBlockParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockParser).single_stmt();
    }

    protected override ParserRuleContext SingleString(Parser parser) {
      Debug.Assert(parser is SeedBlockParser, $"Incorrect parser type: {parser}");
      return (parser as SeedBlockParser).single_string();
    }
  }
}
