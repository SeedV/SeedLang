/*
 * Copyright 2021 The Aha001 Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * SeedPyhton grammar is referred and modified from:
 * https://docs.python.org/3.10/reference/grammar.html
*/

grammar SeedPython;

import Common;

program: statements? EOF;

statements: statement+;
statement: compound_stmt | simple_stmts;
simple_stmts:
  simple_stmt (SEMICOLON simple_stmt)+ SEMICOLON? NEWLINE # multiple_simple_stmts
  | simple_stmt NEWLINE                                   # single_simple_stmt;
simple_stmt:
  assignment    # assignment_placeholder
  | expressions # expression_stmt
  | return_stmt # return_stmt_placeholder
  | 'pass'      # pass
  | 'break'     # break
  | 'continue'  # continue;
compound_stmt: if_stmt # if | while_stmt # while;

assignment: NAME EQUAL expression;

if_stmt:
  IF expression COLON block elif_stmt       # if_elif
  | IF expression COLON block (else_block)? # if_else;
elif_stmt:
  ELIF expression COLON block elif_stmt       # elif_elif
  | ELIF expression COLON block (else_block)? # elif_else;
else_block: ELSE COLON block;

while_stmt: WHILE expression COLON block;

return_stmt: RETURN expressions?;

block:
  NEWLINE INDENT statements DEDENT # block_statements
  | simple_stmts                   # block_simple_stmts;

/*
 * Lexer rules
 */

IF: 'if';
ELIF: 'elif';
ELSE: 'else';
WHILE: 'while';
RETURN: 'return';

COLON: ':';
SEMICOLON: ';';
