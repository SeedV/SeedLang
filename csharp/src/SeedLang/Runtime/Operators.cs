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

namespace SeedLang.Runtime {
  // The operators of binary expressions.
  public enum BinaryOperator {
    Add,
    Subtract,
    Multiply,
    Divide,
    FloorDivide,
    Power,
    Modulo,
  }

  // The operators of boolean expressions.
  public enum BooleanOperator {
    And,
    Or,
  }

  // The operators of comparison expressions.
  public enum ComparisonOperator {
    Less,
    Greater,
    LessEqual,
    GreaterEqual,
    EqEqual,
    NotEqual,
    In,
  }

  // The operators of unary expressions.
  public enum UnaryOperator {
    Positive,
    Negative,
    Not,
  }
}
