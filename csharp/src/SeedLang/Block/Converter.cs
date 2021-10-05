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
using SeedLang.Ast;
using SeedLang.Common;

namespace SeedLang.Block {
  // The converter that converts a block program to an AST tree, an expression inline text to a list
  // of blocks, or an expression inline text to an AST tree.
  public class Converter {
    private class InlineTextListener : InlineTextParser.IInlineTextListener {
      public readonly List<BaseBlock> Blocks = new List<BaseBlock>();
      public IList<TextRange> InvalidTokenRanges { get; private set; }

      public InlineTextListener(IList<TextRange> invalidTokenRanges) {
        InvalidTokenRanges = invalidTokenRanges;
      }

      public void VisitArithmeticOperator(string op, TextRange range) {
        Blocks.Add(new ArithmeticOperatorBlock { Name = op, InlineTextReference = range });
      }

      public void VisitCloseParen(TextRange range) {
        Blocks.Add(new ParenthesisBlock(ParenthesisBlock.Type.Right) {
          InlineTextReference = range
        });
      }

      public void VisitIdentifier(string name, TextRange range) {
        throw new System.NotImplementedException();
      }

      public void VisitNumber(string number, TextRange range) {
        Blocks.Add(new NumberBlock { Value = number, InlineTextReference = range });
      }

      public void VisitOpenParen(TextRange range) {
        Blocks.Add(new ParenthesisBlock(ParenthesisBlock.Type.Left) {
          InlineTextReference = range
        });
      }

      public void VisitString(string str, TextRange range) {
        throw new System.NotImplementedException();
      }

      public void VisitInvalidToken(TextRange range) {
        InvalidTokenRanges?.Add(range);
      }
    }

    // Checks if an inline text is a valid expression. Detailed diagnostic info will be stored in
    // collection if the text is invalid.
    public static bool IsValidInlineTextExpression(string text, DiagnosticCollection collection) {
      if (string.IsNullOrEmpty(text)) {
        collection.Report(SystemReporters.SeedBlock, Severity.Error, null, null,
                          Message.EmptyInlineText);
        return false;
      }
      var parser = new InlineTextParser();
      return parser.Validate(text, "", ParseRule.Expression, collection);
    }

    // Converts an expression inline text to a list of blocks. A list of invalidTokenRanges can be
    // passed in, to collect the text ranges of invalid tokens.
    public static IReadOnlyList<BaseBlock> InlineTextToBlocks(
        string text, IList<TextRange> invalidTokenRanges, DiagnosticCollection collection) {
      if (string.IsNullOrEmpty(text)) {
        collection.Report(SystemReporters.SeedBlock, Severity.Error, null, null,
                          Message.EmptyInlineText);
        return null;
      }
      var parser = new InlineTextParser();
      var listener = new InlineTextListener(invalidTokenRanges);
      parser.VisitInlineText(text, listener);
      return listener.Blocks;
    }

    // Converts a block program to a list of AST trees.
    internal static IReadOnlyList<AstNode> TryConvert(Program program,
                                                      DiagnosticCollection collection) {
      var nodes = new List<AstNode>();
      foreach (var module in program.Modules) {
        foreach (var rootBlock in module.RootBlockIterator) {
          // TODO: implement a visitor to parse other kinds of blocks.
          if (rootBlock is ExpressionBlock expressionBlock) {
            var parser = new InlineTextParser();
            if (parser.Parse(expressionBlock.GetEditableText(), module.Name, ParseRule.Expression,
                             collection, out AstNode node, out IReadOnlyList<SyntaxToken> _)) {
              nodes.Add(node);
            }
          }
        }
      }
      return nodes;
    }
  }
}
