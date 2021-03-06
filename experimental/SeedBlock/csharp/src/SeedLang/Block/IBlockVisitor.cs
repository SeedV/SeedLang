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

namespace SeedLang.Block {
  // The interface to visit a block when walking a group of docked blocks.
  public interface IBlockVisitor {
    // Invoked as soon as a block is being visited, before its docked blocks are reached. This is
    // the first callback when a block is being visited.
    void VisitEnter(BaseBlock block);

    // Invoked after all the docked blocks and the block itself have been visited. This is the last
    // callback when a block is being visited.
    void VisitExit(BaseBlock block);

    // Invoked when an arithmetic operator block is visited.
    void VisitArithmeticOperatorBlock(ArithmeticOperatorBlock block);

    // Invoked when an expression block is visited, after its docked blocks have been visited.
    void VisitExpressionBlock(ExpressionBlock block);

    // Invoked when a number block is visited.
    void VisitNumberBlock(NumberBlock block);

    // Invoked when a parenthesis block is visited.
    void VisitParenthesisBlock(ParenthesisBlock block);
  }
}
