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
  // The block that holds a grouping operator, i.e., either side of a pair of parentheses.
  public class ParenthesisBlock : OperatorBlock {
    public enum Type {
      Left,
      Right
    }

    public ParenthesisBlock() {
      Name = BxfConstants.LeftParenthsis;
    }

    public ParenthesisBlock(Type type) {
      Name = type == Type.Left ? BxfConstants.LeftParenthsis : BxfConstants.RightParenthsis;
    }

    public override void Accept(IBlockVisitor visitor) {
      visitor.VisitEnter(this);
      visitor.VisitParenthesisBlock(this);
      visitor.VisitExit(this);
    }

    protected override bool ValidateText(string text) {
      return text == BxfConstants.LeftParenthsis || text == BxfConstants.RightParenthsis;
    }
  }
}
