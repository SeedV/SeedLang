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
        case BooleanExpression boolean:
          Visit(boolean);
          break;
        case ComparisonExpression comparison:
          Visit(comparison);
          break;
        case UnaryExpression unary:
          Visit(unary);
          break;
        case IdentifierExpression identifier:
          Visit(identifier);
          break;
        case BooleanConstantExpression booleanConstant:
          Visit(booleanConstant);
          break;
        case NoneConstantExpression noneConstant:
          Visit(noneConstant);
          break;
        case NumberConstantExpression numberConstant:
          Visit(numberConstant);
          break;
        case StringConstantExpression stringConstant:
          Visit(stringConstant);
          break;
        case ListExpression list:
          Visit(list);
          break;
        case TupleExpression tuple:
          Visit(tuple);
          break;
        case SubscriptExpression subscript:
          Visit(subscript);
          break;
        case CallExpression call:
          Visit(call);
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
        case BlockStatement block:
          Visit(block);
          break;
        case ExpressionStatement expr:
          Visit(expr);
          break;
        case ForInStatement forIn:
          Visit(forIn);
          break;
        case FuncDefStatement funcDef:
          Visit(funcDef);
          break;
        case IfStatement @if:
          Visit(@if);
          break;
        case ReturnStatement @return:
          Visit(@return);
          break;
        case WhileStatement @while:
          Visit(@while);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented statement type: {statement.GetType()}");
      }
    }

    protected abstract void Visit(BinaryExpression binary);
    protected abstract void Visit(BooleanExpression boolean);
    protected abstract void Visit(ComparisonExpression comparison);
    protected abstract void Visit(UnaryExpression unary);
    protected abstract void Visit(IdentifierExpression identifier);
    protected abstract void Visit(BooleanConstantExpression booleanConstant);
    protected abstract void Visit(NoneConstantExpression noneConstant);
    protected abstract void Visit(NumberConstantExpression numberConstant);
    protected abstract void Visit(StringConstantExpression stringConstant);
    protected abstract void Visit(ListExpression list);
    protected abstract void Visit(TupleExpression tuple);
    protected abstract void Visit(SubscriptExpression subscript);
    protected abstract void Visit(CallExpression call);

    protected abstract void Visit(AssignmentStatement assignment);
    protected abstract void Visit(BlockStatement block);
    protected abstract void Visit(ExpressionStatement expr);
    protected abstract void Visit(ForInStatement forIn);
    protected abstract void Visit(FuncDefStatement funcDef);
    protected abstract void Visit(IfStatement @if);
    protected abstract void Visit(ReturnStatement @return);
    protected abstract void Visit(WhileStatement @while);
  }
}
