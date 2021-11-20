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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.X;

namespace SeedLang.Block {
  // The visitor class to visit a block inline text of SeedBlock programs and generate the
  // corresponding AST tree.
  //
  // The default implement of SeedBlockInlineTextBaseVisitor is to visit all the children and return
  // the result of the last one. BlockInlineTextVisitor overrides the method if the default
  // implement is not correct.
  internal class BlockInlineTextVisitor : SeedBlockInlineTextBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public BlockInlineTextVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    public override AstNode VisitSingleStatement(
        [NotNull] SeedBlockInlineTextParser.SingleStatementContext context) {
      return Visit(context.expressionStatement());
    }
  }
}
