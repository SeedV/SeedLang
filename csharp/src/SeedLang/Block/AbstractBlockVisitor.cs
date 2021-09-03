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

namespace SeedLang.Block {
  // A convenient abstract class that hides the concrete block dispatchers with empty callback
  // methods. Only VisitEnter() and VisitExit() have to be provided when implementing this abstract
  // class.
  //
  // Some simple visitors only need to handle the cases that the walker enters or exits a block,
  // without knowing the concrete block type. For example, a block counter can implement this
  // abstract class and increase its inner counter in VisitEnter().
  //
  // Visitors can still override one or more concrete block dispatcher methods if it's necessary.
  public abstract class AbstractBlockVisitor : IBlockVisitor {
    // Invoked as soon as a block is being visited, before its docked blocks are reached. This is
    // the first callback when a block is being visited.
    public abstract void VisitEnter(BaseBlock block);

    // Invoked after all the docked blocks and the block itself have been visited. This is the last
    // callback when a block is being visited.
    public abstract void VisitExit(BaseBlock block);

    // Invoked when an arithmetic operator block is visited.
    public virtual void VisitArithmeticOperatorBlock(ArithmeticOperatorBlock block) {
    }

    // Invoked when an expression block is visited, after its docked blocks have been visited.
    public virtual void VisitExpressionBlock(ExpressionBlock block) {
    }

    // Invoked when a number block is visited.
    public virtual void VisitNumberBlock(NumberBlock block) {
    }

    // Invoked when a parenthesis block is visited.
    public virtual void VisitParenthesisBlock(ParenthesisBlock block) {
    }
  }
}
