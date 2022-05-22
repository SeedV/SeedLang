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
comparison: bitwise_or (comparison_op bitwise_or)*;
comparison_op:
  EQ_EQUAL
  | NOT_EQUAL
  | LESS_EQUAL
  | LESS
  | GREATER_EQUAL
  | GREATER
  | IN;

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
  primary DOT identifier                       # attribute
  | primary OPEN_BRACK slice_index CLOSE_BRACK # subscript
  | primary OPEN_PAREN arguments? CLOSE_PAREN  # call
  | atom                                       # atom_as_primary;

slice_index:
  expression? COLON expression? (COLON expression?)? # slice
  | expression                                       # index;

atom:
  identifier # identifier_as_atom
  | TRUE     # true
  | FALSE    # false
  | NONE     # none
  | NUMBER   # number
  | STRING+  # strings
  | group    # group_as_atom
  | dict     # dict_as_atom
  | list     # list_as_atom
  | tuple    # tuple_as_atom;

identifier: NAME;

arguments: expression (COMMA expression)*;

group: OPEN_PAREN expression CLOSE_PAREN;

dict: OPEN_BRACE kvpairs? CLOSE_BRACE;

list: OPEN_BRACK expressions? CLOSE_BRACK;

tuple: OPEN_PAREN expressions? CLOSE_PAREN;

kvpairs: kvpair (COMMA kvpair)*;
kvpair: expression COLON expression;

/*
 * Lexer rules
 */

TRUE: 'True';
FALSE: 'False';
NONE: 'None';

AND: 'and';
OR: 'or';
NOT: 'not';
IN: 'in';

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

ADD_ASSIGN: '+=';
SUBSTRACT_ASSIGN: '-=';
MULTIPLY_ASSIGN: '*=';
DIVIDE_ASSIGN: '/=';
MODULO_ASSIGN: '%=';

OPEN_PAREN: '(';
CLOSE_PAREN: ')';
OPEN_BRACK: '[';
CLOSE_BRACK: ']';
OPEN_BRACE: '{';
CLOSE_BRACE: '}';

DOT: '.';
COMMA: ',';
COLON: ':';

NAME: ID_START ID_CONTINUE*;

NUMBER: INTEGER | FLOAT_NUMBER;

STRING: SHORT_STRING;

INTEGER: DECIMAL_INTEGER;

DECIMAL_INTEGER: NON_ZERO_DIGIT DIGIT* | '0'+;

FLOAT_NUMBER: POINT_FLOAT | EXPONENT_FLOAT;

NEWLINE: ('\r'? '\n' | '\r' | '\f') SPACES?;

SKIP_: (SPACES | LINE_JOINING) -> skip;

UNKNOWN_CHAR: .;

/*
 * Fragments
 */

fragment SHORT_STRING:
  '\'' (STRING_ESCAPE_SEQ | ~[\\\r\n\f'])* '\''
  | '"' (STRING_ESCAPE_SEQ | ~[\\\r\n\f"])* '"';

fragment STRING_ESCAPE_SEQ: '\\' . | '\\' NEWLINE;

fragment POINT_FLOAT:
  INT_PART? FRACTION
  | INT_PART DOT;

fragment EXPONENT_FLOAT: (INT_PART | POINT_FLOAT) EXPONENT;

fragment INT_PART: DIGIT+;

fragment FRACTION: DOT DIGIT+;

fragment EXPONENT: [eE] [+-]? DIGIT+;

fragment NON_ZERO_DIGIT: [1-9];

fragment DIGIT: [0-9];

fragment SPACES: [ \t]+;

fragment LINE_JOINING:
  '\\' SPACES? ('\r'? '\n' | '\r' | '\f');

fragment ID_START: '_' | [A-Z] | [a-z];

fragment ID_CONTINUE: ID_START | [0-9];
