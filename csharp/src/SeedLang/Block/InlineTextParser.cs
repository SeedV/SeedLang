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
    // The listener interface to be notified when the tokens of an expression inline text are
    // visited.
    internal interface IInlineTextListener {
      void VisitArithmeticOperator(string op, TextRange range);
      void VisitIdentifier(string name, TextRange range);
      void VisitNumber(string number, TextRange range);
      void VisitString(string str, TextRange range);
      void VisitOpenParen(TextRange range);
      void VisitCloseParen(TextRange range);
      void VisitInvalidToken(TextRange range);
    }

    // Visits an inline text of block programs. The given listener is notified when each token of
    // the inline text is visited. The negative sign token will be combined with the following
    // number to form a negative number.
    //
    // The input can be either a valid inline text or an invalid inline text. If the text is
    // invalid, this method will still try to parse it and notify the listener about both valid and
    // invalid tokens.
    internal void VisitInlineText(string text, IInlineTextListener listener) {
      Lexer lexer = SetupLexer(text);
      int lastTokenType = SeedBlockParser.UNKNOWN_CHAR;
      IToken negativeToken = null;
      foreach (var token in lexer.GetAllTokens()) {
        if (!(negativeToken is null) && token.Type != SeedBlockLexer.NUMBER) {
          // TODO: define a const string for the negative sign?
          listener.VisitArithmeticOperator("-", CodeReferenceUtils.RangeOfToken(negativeToken));
          negativeToken = null;
        }
        switch (token.Type) {
          case SeedBlockLexer.ADD:
          case SeedBlockLexer.MUL:
          case SeedBlockLexer.DIV:
            listener.VisitArithmeticOperator(token.Text, CodeReferenceUtils.RangeOfToken(token));
            break;
          case SeedBlockLexer.SUB:
            if (lastTokenType == SeedBlockParser.NUMBER ||
                lastTokenType == SeedBlockParser.CLOSE_PAREN) {
              listener.VisitArithmeticOperator(token.Text, CodeReferenceUtils.RangeOfToken(token));
            } else {
              negativeToken = token;
            }
            break;
          case SeedBlockLexer.IDENTIFIER:
            listener.VisitIdentifier(token.Text, CodeReferenceUtils.RangeOfToken(token));
            break;
          case SeedBlockLexer.NUMBER:
            TextRange combinedRange =
                negativeToken is null ?
                CodeReferenceUtils.RangeOfToken(token) :
                CodeReferenceUtils.RangeOfTokens(negativeToken, token);
            // TODO: define a const string for the negative sign?
            listener.VisitNumber((negativeToken is null ? "" : "-") + token.Text, combinedRange);
            negativeToken = null;
            break;
          case SeedBlockLexer.STRING:
            listener.VisitString(token.Text, CodeReferenceUtils.RangeOfToken(token));
            break;
          case SeedBlockLexer.OPEN_PAREN:
            listener.VisitOpenParen(CodeReferenceUtils.RangeOfToken(token));
            break;
          case SeedBlockLexer.CLOSE_PAREN:
            listener.VisitCloseParen(CodeReferenceUtils.RangeOfToken(token));
            break;
          default:
            // Invlid tokens.
            listener.VisitInvalidToken(CodeReferenceUtils.RangeOfToken(token));
            break;
        }
        lastTokenType = token.Type;
      }
      if (!(negativeToken is null)) {
        // TODO: define a const string for the negative sign?
        listener.VisitArithmeticOperator("-", CodeReferenceUtils.RangeOfToken(negativeToken));
      }
    }

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
