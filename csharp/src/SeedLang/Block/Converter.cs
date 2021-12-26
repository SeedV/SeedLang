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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.X;

namespace SeedLang.Block {
  // The converter that converts a block program to an AST tree, an expression inline text to a list
  // of blocks, or an expression inline text to an AST tree.
  public class Converter {
    // Checks if an inline text is a valid expression. Detailed diagnostic info will be stored in
    // collection if the text is invalid.
    public static bool IsValidInlineTextExpression(string text, DiagnosticCollection collection) {
      if (string.IsNullOrEmpty(text)) {
        collection.Report(SystemReporters.SeedBlock, Severity.Error, null, null,
                          Message.EmptyInlineText);
        return false;
      }
      var parser = new SeedBlockInlineText();
      return parser.Validate(text, "", collection);
    }

    // Converts an expression inline text to a list of blocks. Returns null if the inline text is
    // null, empty or invalid.
    public static IReadOnlyList<BaseBlock> InlineTextToBlocks(string text,
                                                              DiagnosticCollection collection) {
      if (string.IsNullOrEmpty(text)) {
        collection.Report(SystemReporters.SeedBlock, Severity.Error, null, null,
                          Message.EmptyInlineText);
        return null;
      }
      var parser = new SeedBlockInlineText();
      if (!parser.Parse(text, "", collection, out _, out IReadOnlyList<SyntaxToken> tokens)) {
        return null;
      }

      int i = 0;
      var blocks = new List<BaseBlock>();
      while (i < tokens.Count) {
        switch (tokens[i].Type) {
          case SyntaxType.Number:
            blocks.Add(new NumberBlock {
              Value = TextOfRange(text, tokens[i].Range),
              InlineTextReference = tokens[i].Range,
            });
            break;
          case SyntaxType.Operator:
            TextRange range = tokens[i].Range;
            string op = TextOfRange(text, range);
            if (op == "-") {
              if (i < tokens.Count - 1 && tokens[i + 1].Type == SyntaxType.Number) {
                TextRange newRange = CodeReferenceUtils.CombineRanges(range, tokens[i + 1].Range);
                string number = TextOfRange(text, newRange);
                blocks.Add(new NumberBlock { Value = number, InlineTextReference = newRange });
                ++i;
                break;
              }
            }
            blocks.Add(new ArithmeticOperatorBlock { Name = op, InlineTextReference = range });
            break;
          case SyntaxType.Parenthesis:
            string paren = TextOfRange(text, tokens[i].Range);
            var type = paren == "(" ? ParenthesisBlock.Type.Left : ParenthesisBlock.Type.Right;
            blocks.Add(new ParenthesisBlock(type) { InlineTextReference = tokens[i].Range });
            break;
          case SyntaxType.Keyword:
          case SyntaxType.String:
          case SyntaxType.Symbol:
          case SyntaxType.Unknown:
          case SyntaxType.Variable:
            break;
        }
        ++i;
      }
      return blocks;
    }

    // Converts a block program to a list of AST trees.
    internal static IReadOnlyList<AstNode> Convert(Program program,
                                                   DiagnosticCollection collection) {
      var nodes = new List<AstNode>();
      foreach (var module in program.Modules) {
        foreach (var rootBlock in module.RootBlockIterator) {
          // TODO: implement a visitor to parse other kinds of blocks.
          if (rootBlock is ExpressionBlock expressionBlock) {
            var parser = new SeedBlockInlineText();
            if (parser.Parse(expressionBlock.GetEditableText(), module.Name, collection,
                             out AstNode node, out IReadOnlyList<SyntaxToken> _)) {
              nodes.Add(node);
            }
          }
        }
      }
      return nodes;
    }

    private static string TextOfRange(string text, TextRange range) {
      return text.Substring(range.Start.Column, range.End.Column - range.Start.Column + 1);
    }
  }
}
