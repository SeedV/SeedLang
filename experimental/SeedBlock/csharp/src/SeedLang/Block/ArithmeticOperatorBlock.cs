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
  // The block that represents an arithmetic operator.
  public class ArithmeticOperatorBlock : OperatorBlock {
    // TODO: suppot name -> display name conversion. E.g., arithmetic operations "*" will be
    // converted to "\u00D7" (the multiplication sign) for display, "/" will be converted to
    // "\u00F7" (the division sign) for display.

    public override void Accept(IBlockVisitor visitor) {
      visitor.VisitEnter(this);
      visitor.VisitArithmeticOperatorBlock(this);
      visitor.VisitExit(this);
    }

    protected override bool ValidateText(string text) {
      // TODO: Validate the input text with the underlying parser.
      return true;
    }
  }
}
