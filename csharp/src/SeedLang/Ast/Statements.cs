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

using SeedLang.Common;

namespace SeedLang.Ast {
  // The base class of all statement nodes.
  internal abstract class Statement : AstNode {
    // The factory method to create an assignment statement.
    internal static AssignmentStatement Assignment(IdentifierExpression identifier, Expression expr,
                                                   Range range) {
      return new AssignmentStatement(identifier, expr, range);
    }

    // The factory method to create a block statement.
    internal static BlockStatement Block(Statement[] statements, Range range) {
      return new BlockStatement(statements, range);
    }

    // The factory method to create an expression statement.
    internal static ExpressionStatement Expression(Expression expr, Range range) {
      return new ExpressionStatement(expr, range);
    }

    // The factory method to create an if statement.
    internal static IfStatement If(Expression test, Statement thenBody, Statement elseBody,
                                   Range range) {
      return new IfStatement(test, thenBody, elseBody, range);
    }

    // The factory method to create an while statement.
    internal static WhileStatement While(Expression test, Statement body, Range range) {
      return new WhileStatement(test, body, range);
    }

    internal Statement(Range range) : base(range) {
    }
  }

  internal class AssignmentStatement : Statement {
    public IdentifierExpression Identifier { get; }
    public Expression Expr { get; }

    internal AssignmentStatement(IdentifierExpression identifier, Expression expr,
                                 Range range) : base(range) {
      Identifier = identifier;
      Expr = expr;
    }
  }

  internal class BlockStatement : Statement {
    public Statement[] Statements { get; }

    internal BlockStatement(Statement[] statements, Range range) : base(range) {
      Statements = statements;
    }
  }

  internal class ExpressionStatement : Statement {
    public Expression Expr { get; }

    internal ExpressionStatement(Expression expr, Range range) : base(range) {
      Expr = expr;
    }
  }

  internal class IfStatement : Statement {
    public Expression Test { get; }
    public Statement ThenBody { get; }
    public Statement ElseBody { get; }

    internal IfStatement(Expression test, Statement thenBody, Statement elseBody, Range range) :
        base(range) {
      Test = test;
      ThenBody = thenBody;
      ElseBody = elseBody;
    }
  }

  internal class WhileStatement : Statement {
    public Expression Test { get; }
    public Statement Body { get; }

    internal WhileStatement(Expression test, Statement body, Range range) : base(range) {
      Test = test;
      Body = body;
    }
  }
}
