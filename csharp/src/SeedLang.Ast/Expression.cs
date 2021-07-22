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

namespace SeedLang.Ast {
  // The base class of all expression nodes.
  public abstract class Expression : AstNode {
    // Creates the binary expression.
    public static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right) {
      return new BinaryExpression(left, op, right);
    }

    // Creates the number constant expression.
    public static NumberConstantExpression Number(double value) {
      return new NumberConstantExpression(value);
    }

    // Creates the string constant expression.
    public static StringConstantExpression String(string value) {
      return new StringConstantExpression(value);
    }
  }
}
