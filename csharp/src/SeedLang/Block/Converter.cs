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
using SeedLang.Ast;
using SeedLang.Common;

namespace SeedLang.Block {
  // The converter that converts a block program to an AST tree, an expression inline text to a list
  // of blocks, or an expression inline text to an AST tree.
  public class Converter {
    private class InlineTextListener : InlineTextParser.IInlineTextListener {
      public readonly List<BaseBlock> Blocks = new List<BaseBlock>();

      public void VisitArithmeticOperator(string op) {
        Blocks.Add(new ArithmeticOperatorBlock { Name = op });
      }

      public void VisitCloseParen() {
        Blocks.Add(new ParenthesisBlock(ParenthesisBlock.Type.Right));
      }

      public void VisitIdentifier(string name) {
        throw new System.NotImplementedException();
      }

      public void VisitNumber(string number) {
        Blocks.Add(new NumberBlock { Value = number });
      }

      public void VisitOpenParen() {
        Blocks.Add(new ParenthesisBlock(ParenthesisBlock.Type.Left));
      }

      public void VisitString(string str) {
        throw new System.NotImplementedException();
      }
    }

    // Converts an expression inline text to a list of blocks.
    public static IEnumerable<BaseBlock> InlineTextToBlocks(string text) {
      Debug.Assert(!string.IsNullOrEmpty(text), "Inline text shall not be null or empty.");
      var parser = new InlineTextParser();
      var listener = new InlineTextListener();
      parser.VisitInlineText(text, listener);
      return listener.Blocks;
    }

    // Converts a block program to a list of AST trees.
    internal static IEnumerable<AstNode> TryConvert(Program program,
                                                    DiagnosticCollection collection) {
      var nodes = new List<AstNode>();
      foreach (var module in program.Modules) {
        foreach (var rootBlock in module.RootBlockIterator) {
          // TODO: implement a visitor to parse other kinds of blocks.
          if (rootBlock is ExpressionBlock expressionBlock) {
            var parser = new InlineTextParser();
            if (parser.TryParse(expressionBlock.GetEditableText(), module.Name,
                                ParseRule.Expression, collection, out var node)) {
              nodes.Add(node);
            }
          }
        }
      }
      return nodes;
    }
  }
}
