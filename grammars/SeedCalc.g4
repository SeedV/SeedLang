/**
 * Copyright 2021-2022 The SeedV Lab.
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

grammar SeedCalc;

@header {
  #pragma warning disable 3021
}

/*
 * Parser rules
 */

expression_stmt: expression NEWLINE* EOF;

expression: sum;

sum:
  sum ADD term        # add
  | sum SUBTRACT term # subtract
  | term              # term_as_sum;
term:
  term MULTIPLY factor # multiply
  | term DIVIDE factor # divide
  | factor             # factor_as_term;
factor:
  ADD atom        # positive
  | SUBTRACT atom # negative
  | atom          # atom_as_factor;
atom: NUMBER # number | group # group_as_atom;

group: OPEN_PAREN expression CLOSE_PAREN;

/*
 * Lexer rules
 */

ADD: '+';
SUBTRACT: '-';
MULTIPLY: '*';
DIVIDE: '/';

OPEN_PAREN: '(';
CLOSE_PAREN: ')';

NUMBER: INTEGER | FLOAT_NUMBER;

INTEGER: DECIMAL_INTEGER;

DECIMAL_INTEGER: NON_ZERO_DIGIT DIGIT* | '0'+;

FLOAT_NUMBER: POINT_FLOAT | EXPONENT_FLOAT;

NEWLINE: ('\r'? '\n' | '\r' | '\f') SPACES?;

SKIP_: SPACES -> skip;

UNKNOWN_CHAR: .;

/*
 * Fragments
 */

fragment POINT_FLOAT:
  INT_PART? FRACTION
  | INT_PART '.';

fragment EXPONENT_FLOAT: (INT_PART | POINT_FLOAT) EXPONENT;

fragment INT_PART: DIGIT+;

fragment FRACTION: '.' DIGIT+;

fragment EXPONENT: [eE] [+-]? DIGIT+;

fragment NON_ZERO_DIGIT: [1-9];

fragment DIGIT: [0-9];

fragment SPACES: [ \t]+;
