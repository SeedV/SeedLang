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
  // An abstract base class to traverse an expression tree.
  //
  // All node types in the AST tree are either Expression or Statement. So the seperated
  // ExpressionWalker and StatementWalker classes are enough to visit all nodes in AST trees.
  internal abstract class ExpressionWalker {
    // Dispatches to the correspoding visit method based on the type of the expression node.
    internal void Visit(Expression expression) {
      Enter(expression);
      switch (expression) {
        case BinaryExpression binary:
          VisitBinary(binary);
          break;
        case BooleanExpression boolean:
          VisitBoolean(boolean);
          break;
        case BooleanConstantExpression booleanConstant:
          VisitBooleanConstant(booleanConstant);
          break;
        case CallExpression call:
          VisitCall(call);
          break;
        case ComparisonExpression comparison:
          VisitComparison(comparison);
          break;
        case DictExpression dict:
          VisitDict(dict);
          break;
        case IdentifierExpression identifier:
          VisitIdentifier(identifier);
          break;
        case ListExpression list:
          VisitList(list);
          break;
        case NilConstantExpression nilConstant:
          VisitNilConstant(nilConstant);
          break;
        case NumberConstantExpression numberConstant:
          VisitNumberConstant(numberConstant);
          break;
        case StringConstantExpression stringConstant:
          VisitStringConstant(stringConstant);
          break;
        case SubscriptExpression subscript:
          VisitSubscript(subscript);
          break;
        case TupleExpression tuple:
          VisitTuple(tuple);
          break;
        case UnaryExpression unary:
          VisitUnary(unary);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented expression type: {expression.GetType()}");
      }
      Exit(expression);
    }

    protected virtual void Enter(Expression expr) { }
    protected virtual void Exit(Expression expr) { }

    protected abstract void VisitBinary(BinaryExpression binary);
    protected abstract void VisitBoolean(BooleanExpression boolean);
    protected abstract void VisitBooleanConstant(BooleanConstantExpression booleanConstant);
    protected abstract void VisitCall(CallExpression call);
    protected abstract void VisitComparison(ComparisonExpression comparison);
    protected abstract void VisitDict(DictExpression dict);
    protected abstract void VisitIdentifier(IdentifierExpression identifier);
    protected abstract void VisitList(ListExpression list);
    protected abstract void VisitNilConstant(NilConstantExpression nilConstant);
    protected abstract void VisitNumberConstant(NumberConstantExpression numberConstant);
    protected abstract void VisitStringConstant(StringConstantExpression stringConstant);
    protected abstract void VisitSubscript(SubscriptExpression subscript);
    protected abstract void VisitTuple(TupleExpression tuple);
    protected abstract void VisitUnary(UnaryExpression unary);
  }
}
