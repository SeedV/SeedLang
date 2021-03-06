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
  // Common settings used by the Block Exchange Format.
  public static class BxfConstants {
    // Implementation-independent block type names for the BXF format.
    public static class BlockType {
      public const string ArithmeticOperator = "arithmeticOperator";
      public const string Expression = "expression";
      public const string Number = "number";
      public const string Parenthsis = "parenthesis";
    }

    // Constant values for the BXF format.
    public const string Schema = "bxf";
    public const string Version = "v0.1";
    public const int IdLength = 8;
    public const string DefaultOperatorName = "+";
    public const string LeftParenthsis = "(";
    public const string RightParenthsis = ")";
  }
}
