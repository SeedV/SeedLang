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

using System.Collections.Generic;
using System.Text;

namespace SeedLang.Common {
  // Token types are typically used for syntax or semantic highlighting in code editors or IDEs.
  //
  // SeedLang provides the ability to classify source code into both syntax tokens and semantic
  // tokens. See the BaseParser class for the parsing interfaces.
  //
  // SeedLang uses an extended version of the standard set of token types defined by VS Code for all
  // the SeedLang scripting languages. See
  // https://code.visualstudio.com/api/language-extensions/semantic-highlight-guide for a detailed
  // list of VS Code's standard set.
  //
  // Although different SeedLang scripting languages use different subsets of the following
  // classification, it's still required to be consistent when classifying similar tokens across
  // languages.
  public enum TokenType {
    // VS Code standard token types.
    Class,             // For identifiers that declare or reference a class type.
    Comment,           // For tokens that represent a comment.
    Decorator,         // For identifiers that declare or reference decorators and annotations.
    Enum,              // For identifiers that declare or reference an enumeration type.
    EnumMember,        // For identifiers that declare or reference an enumeration property,
                       // constant, or member.
    Event,             // For identifiers that declare an event property.
    Function,          // For identifiers that declare a function.
    Interface,         // For identifiers that declare or reference an interface type.
    Keyword,           // For tokens that represent a language keyword.
    Label,             // For identifiers that declare a label.
    Macro,             // For identifiers that declare a macro.
    Method,            // For identifiers that declare a member function or method.
    Namespace,         // For identifiers that declare or reference a namespace, module, or package.
    Number,            // For tokens that represent a number literal.
    Operator,          // For tokens that represent an operator.
    Parameter,         // For identifiers that declare or reference a function or method parameters.
    Property,          // For identifiers that declare or reference a member property, member field,
                       // or member variable.
    Regexp,            // For tokens that represent a regular expression literal.
    String,            // For tokens that represent a string literal.
    Struct,            // For identifiers that declare or reference a struct type.
    Type,              // For identifiers that declare or reference a type that is not covered
                       // above.
    TypeParameter,     // For identifiers that declare or reference a type parameter.
    Variable,          // For identifiers that declare or reference a local or global variable.

    // Extended token types.
    Boolean,           // For tokens that represent a boolean literal.
    CloseBrace,        // For tokens that represent a curly bracket.
    CloseBracket,      // For tokens that represent a square bracket.
    CloseParenthesis,  // For tokens that represent a parenthesis.
    None,              // For tokens that represent a None literal.
    OpenBrace,         // For tokens that represent a curly bracket.
    OpenBracket,       // For tokens that represent a square bracket.
    OpenParenthesis,   // For tokens that represent a parenthesis.
    Symbol,            // For tokens that represent an unclassified symbol.
    Unknown,           // Unknown token type.
  }

  // Represents syntax or semantic tokens parsed from source code.
  public class TokenInfo {
    public TokenType Type { get; }
    public TextRange Range { get; }

    public TokenInfo(TokenType type, TextRange range) {
      Type = type;
      Range = range;
    }

    public override string ToString() {
      return $"{Type} {Range}";
    }
  }
}
