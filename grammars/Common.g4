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

grammar Common;

@header {
  #pragma warning disable 3021
}

/*
 * Parser rules
 */

expressions: expression (COMMA expression)* COMMA?;

expression: disjunction;

disjunction: conjunction (OR conjunction)*;
conjunction: inversion (AND inversion)*;
inversion:
  NOT inversion # not
  | comparison  # comparison_as_inversion;
comparison: bitwise_or compare_op_bitwise_or_pair*;
compare_op_bitwise_or_pair:
  EQ_EQUAL bitwise_or        # eq_equal
  | NOT_EQUAL bitwise_or     # not_equal
  | LESS_EQUAL bitwise_or    # less_equal
  | LESS bitwise_or          # less
  | GREATER_EQUAL bitwise_or # greater_equal
  | GREATER bitwise_or       # greater;

// TODO: add bitwise parsing rule
bitwise_or: sum;

sum:
  sum ADD term        # add
  | sum SUBTRACT term # subtract
  | term              # term_as_sum;
term:
  term MULTIPLY factor       # multiply
  | term DIVIDE factor       # divide
  | term FLOOR_DIVIDE factor # floor_divide
  | term MODULO factor       # modulo
  | factor                   # factor_as_term;
factor:
  ADD factor             # positive
  | SUBTRACT factor      # negative
  | primary POWER factor # power
  | primary              # primary_as_factor;
primary:
  primary OPEN_BRACK expression CLOSE_BRACK   # subscript
  | primary OPEN_PAREN arguments? CLOSE_PAREN # call
  | atom                                      # atom_as_primary;
atom:
  identifier # identifier_as_atom
  | TRUE     # true
  | FALSE    # false
  | NONE     # none
  | NUMBER   # number
  | group    # group_as_atom
  | list     # list_as_atom;
identifier: NAME;

arguments: expression (COMMA expression)*;

group: OPEN_PAREN expression CLOSE_PAREN;

list: OPEN_BRACK expressions? CLOSE_BRACK;

/*
 * Lexer rules
 */

TRUE: 'True';
FALSE: 'False';
NONE: 'None';

AND: 'and';
OR: 'or';
NOT: 'not';

EQUAL: '=';

EQ_EQUAL: '==';
NOT_EQUAL: '!=';
LESS_EQUAL: '<=';
LESS: '<';
GREATER_EQUAL: '>=';
GREATER: '>';

ADD: '+';
SUBTRACT: '-';
MULTIPLY: '*';
DIVIDE: '/';
FLOOR_DIVIDE: '//';
POWER: '**';
MODULO: '%';

OPEN_PAREN: '(';
CLOSE_PAREN: ')';
OPEN_BRACK: '[';
CLOSE_BRACK: ']';
OPEN_BRACE: '{';
CLOSE_BRACE: '}';

COMMA: ',';

NAME: ID_START ID_CONTINUE*;

NUMBER: INTEGER | FLOAT_NUMBER;

STRING: '"' .*? '"';

INTEGER: DECIMAL_INTEGER;

DECIMAL_INTEGER: NON_ZERO_DIGIT DIGIT* | '0'+;

FLOAT_NUMBER: POINT_FLOAT | EXPONENT_FLOAT;

NEWLINE: ('\r'? '\n' | '\r' | '\f') SPACES?;

SKIP_: (SPACES | COMMENT | LINE_JOINING) -> skip;

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

fragment COMMENT: '#' ~[\r\n\f]*;

fragment LINE_JOINING:
  '\\' SPACES? ('\r'? '\n' | '\r' | '\f');

fragment ID_START: '_' | [A-Z] | [a-z];

fragment ID_CONTINUE: ID_START | [0-9];
