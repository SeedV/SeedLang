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
  // A base class to traverse an AST tree.
  internal abstract class AstWalker {
    // Dispatches to the expression or statement visit method based on the type of the AST node.
    internal void Visit(AstNode node) {
      switch (node) {
        case Expression expression:
          Visit(expression);
          break;
        case Statement statement:
          Visit(statement);
          break;
        default:
          throw new NotImplementedException($"Not implemented node type: {node.GetType()}");
      }
    }

    // Dispatches to the correspoding visit method based on the type of the expression node.
    internal void Visit(Expression expression) {
      switch (expression) {
        case BinaryExpression binary:
          Visit(binary);
          break;
        case CompareExpression compare:
          Visit(compare);
          break;
        case IdentifierExpression identifier:
          Visit(identifier);
          break;
        case NumberConstantExpression number:
          Visit(number);
          break;
        case StringConstantExpression str:
          Visit(str);
          break;
        case UnaryExpression unary:
          Visit(unary);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented expression type: {expression.GetType()}");
      }
    }

    // Dispatches to the correspoding visit method based on the type of the statement node.
    internal void Visit(Statement statement) {
      switch (statement) {
        case AssignmentStatement assignment:
          Visit(assignment);
          break;
        case ExpressionStatement expr:
          Visit(expr);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented statement type: {statement.GetType()}");
      }
    }

    protected abstract void Visit(BinaryExpression binary);

    protected abstract void Visit(CompareExpression compare);

    protected abstract void Visit(IdentifierExpression identifier);

    protected abstract void Visit(NumberConstantExpression number);

    protected abstract void Visit(StringConstantExpression str);

    protected abstract void Visit(UnaryExpression unary);

    protected abstract void Visit(AssignmentStatement assignment);

    protected abstract void Visit(ExpressionStatement expr);
  }
}
