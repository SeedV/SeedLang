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
using SeedLang.X;

namespace SeedLang.Block {
  // The parser that converts a block program to an AST tree, an inline expression text to a list of
  // blocks, or an inline expression text to an AST tree.
  //
  // This class is different from SeedLang.X.BlockParser, which is only responsible for parsing text
  // source code of block programs. This class invokes the interfaces of SeedLang.X.BlockParser to
  // convert from an inline expression text to a list of blocks or an AST tree.
  public class Parser {
    private class ExressionListener : BlockTextParser.IExpressionListener {
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

    // Converts an expression text to a list of blocks.
    public static IEnumerable<BaseBlock> ExpressionTextToBlocks(string text) {
      Debug.Assert(!string.IsNullOrEmpty(text), "Expression text shall not be null or empty.");
      var blockTextParser = new BlockTextParser();
      var listener = new ExressionListener();
      blockTextParser.VisitExpression(text, listener);
      return listener.Blocks;
    }

    // Parses a block program to an list of AST trees.
    internal static IEnumerable<AstNode> TryParse(Program program,
                                                  DiagnosticCollection collection) {
      var nodes = new List<AstNode>();
      foreach (var module in program.Modules) {
        foreach (var rootBlock in module.RootBlockIterator) {
          // TODO: implement a visitor to parse other kinds of blocks.
          if (rootBlock is ExpressionBlock expressionBlock) {
            var blockTextParser = new BlockTextParser();
            if (blockTextParser.TryParse(expressionBlock.GetEditableText(), module.Name,
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
