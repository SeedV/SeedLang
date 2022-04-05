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

using SeedLang.Common;

namespace SeedLang.Ast {
  // The base class of all statement nodes.
  internal abstract class Statement : AstNode {
    // The factory method to create assignment statements.
    internal static AssignmentStatement Assignment(Expression[] targets, Expression[] exprs,
                                                   Range range) {
      return new AssignmentStatement(targets, exprs, range);
    }

    // The factory method to create block statements.
    internal static BlockStatement Block(Statement[] statements, Range range) {
      return new BlockStatement(statements, range);
    }

    // The factory method to create expression statements.
    internal static ExpressionStatement Expression(Expression expr, Range range) {
      return new ExpressionStatement(expr, range);
    }

    // The factory method to create for in statements.
    internal static ForInStatement ForIn(IdentifierExpression id, Expression expr, Statement body,
                                         Range range) {
      return new ForInStatement(id, expr, body, range);
    }

    // The factory method to create function define statements.
    internal static FuncDefStatement FuncDef(string name, string[] parameters, Statement body,
                                             Range range) {
      return new FuncDefStatement(name, parameters, body, range);
    }

    // The factory method to create if statements.
    internal static IfStatement If(Expression test, Statement thenBody, Statement elseBody,
                                   Range range) {
      return new IfStatement(test, thenBody, elseBody, range);
    }

    // The factory method to create pass statements.
    internal static PassStatement Pass(Range range) {
      return new PassStatement(range);
    }

    // The factory method to create return statements.
    internal static ReturnStatement Return(Expression[] exprs, Range range) {
      return new ReturnStatement(exprs, range);
    }

    // The factory method to create while statements.
    internal static WhileStatement While(Expression test, Statement body, Range range) {
      return new WhileStatement(test, body, range);
    }

    // The factory method to create VTag statements.
    internal static VTagStatement VTag(VTagStatement.VTagInfo[] vTags, Statement[] statements,
                                       Range range) {
      return new VTagStatement(vTags, statements, range);
    }

    internal Statement(Range range) : base(range) { }
  }

  internal class AssignmentStatement : Statement {
    // The targets of assignment statements. The class of each target could be IdentifierExpression
    // or SubscriptExpression.
    public Expression[] Targets { get; }
    // The right values of assignment statements. The lengths of targets and exprs are not necessary
    // to be the same.
    public Expression[] Exprs { get; }

    internal AssignmentStatement(Expression[] targets, Expression[] exprs, Range range) :
        base(range) {
      Targets = targets;
      Exprs = exprs;
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

  internal class ForInStatement : Statement {
    public IdentifierExpression Id { get; }
    public Expression Expr { get; }
    public Statement Body { get; }

    internal ForInStatement(IdentifierExpression id, Expression expr, Statement body, Range range) :
        base(range) {
      Id = id;
      Expr = expr;
      Body = body;
    }
  }

  internal class FuncDefStatement : Statement {
    public string Name { get; }
    public string[] Parameters { get; }
    public Statement Body { get; }

    internal FuncDefStatement(string name, string[] parameters, Statement body, Range range) :
        base(range) {
      Name = name;
      Parameters = parameters;
      Body = body;
    }
  }

  internal class IfStatement : Statement {
    public Expression Test { get; }
    public Statement ThenBody { get; }
    // The else body of if statements, could be null.
    public Statement ElseBody { get; }

    internal IfStatement(Expression test, Statement thenBody, Statement elseBody, Range range) :
        base(range) {
      Test = test;
      ThenBody = thenBody;
      ElseBody = elseBody;
    }
  }

  internal class PassStatement : Statement {
    internal PassStatement(Range range) : base(range) { }
  }

  internal class ReturnStatement : Statement {
    public Expression[] Exprs { get; }

    internal ReturnStatement(Expression[] exprs, Range range) : base(range) {
      Exprs = exprs;
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

  internal class VTagStatement : Statement {
    internal class VTagInfo {
      public string Name { get; }
      public Expression[] Parameters { get; }

      internal VTagInfo(string name, Expression[] parameters) {
        Name = name;
        Parameters = parameters;
      }

      // TODO: append the parameters into the result.
      public override string ToString() {
        return $"{Name}";
      }
    }

    public VTagInfo[] VTags { get; }
    // The statements enclosed in the VTag.
    public Statement[] Statements { get; }

    internal VTagStatement(VTagInfo[] vTags, Statement[] statements, Range range) : base(range) {
      VTags = vTags;
      Statements = statements;
    }
  }
}
