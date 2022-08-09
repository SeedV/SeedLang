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
using System.Globalization;
using System.Text;

namespace SeedLang.Common {
  // The ID list of string messages.
  //
  // The trailing decimal digit of the message ID indicates the number of the required arguments,
  // ranging from 0 to 9.
  //
  // To retrieve the raw localized string with argument placeholders such as "{0}":
  //
  //    Message.SomeId.Get()
  //    Message.SomeId.Get(culture)
  //
  // To localizes and formats a message with given arguments:
  //
  //    Message.SomeIdWithThreeArguments3.Format(arg1, arg2, arg3)
  //    Message.SomeIdWithThreeArguments3.Format(culture, arg1, arg2, arg3)
  //
  // Both methods accept a CultureInfo object that specifies the expected locale. If the culture
  // parameter is omitted, the current CultureInfo object will be used.
  public enum Message {
    // Example message strings.

    Okay,
    Yes,
    No,
    ExampleMessageWithOneArgument1,
    ExampleMessageWithTwoArguments2,
    ExampleMessageWithNineArguments9,

    // SeedLang message strings.
    //
    // Please keep the following message IDs in alphabetical order.

    EngineNotPaused,                        // Program execution is not paused.
    EngineProgramNotCompiled,               // Program is not compiled to AST and bytecode.
    RuntimeErrorBreakOutsideLoop,           // The Break statement is outside of loop statements.
    RuntimeErrorContinueOutsideLoop,        // The Continue statement is outside of loop statements.
    RuntimeErrorDivideByZero,               // Divide by zero.
    RuntimeErrorIncorrectArgsCount,         // Incorrect arguments count.
    RuntimeErrorIncorrectSliceAssignCount,  // Incorrect count of the sequence for slice assignment.
    RuntimeErrorIncorrectUnpackCount,       // Incorrect number of values to be unpack.
    RuntimeErrorInvalidCast,                // Invalid cast of value types.
    RuntimeErrorInvalidInteger,             // Not a valid integer.
    RuntimeErrorNoKey,                      // Couldn't find the given key in the dictionary.
    RuntimeErrorNotCallable,                // Value type not callable.
    RuntimeErrorNotCountable,               // Value type not countable.
    RuntimeErrorNotSubscriptable,           // Value type not subscriptable.
    RuntimeErrorNotSupportAssignment,       // The value type does not support assignment.
    RuntimeErrorOutOfRange,                 // Index out of range.
    RuntimeErrorOverflow,                   // Overflow.
    RuntimeErrorSliceAssignNotIterable,     // The value assigned to a sliced list is not iterable.
    RuntimeErrorUnhashableType,             // The type of keys is unhashable.
    RuntimeErrorUnsupportedOperands,        // The type of operands isn't supported by the operator.
    RuntimeErrorVariableNotDefined,         // Variable not defined.
    SyntaxErrorFailedPredicate1,            // Semantic predicate failed syntax error.
    SyntaxErrorInputMismatch2,              // Input mismatch syntax error.
    SyntaxErrorMissingToken2,               // Missing token syntax error.
    SyntaxErrorNoViableAlternative1,        // No viable alternative path syntax error.
    SyntaxErrorUnwantedToken2,              // Unwanted token syntax error.
    UnsupportedEvalSyntax,                  // The input string cannot be evaluated. Only supports
                                            // expressions without side effects.
  }

  public static class MessageHelper {
    // Returns the raw message string without formatting it, using the current CultureInfo. The
    // required arguments will be rendered as placeholders, in the "{n}" format.
    public static string Get(this Message message) {
      return Get(message, CultureInfo.CurrentCulture);
    }

    // Returns the raw message string without formatting it, using the specified CultureInfo.
    public static string Get(this Message message, CultureInfo culture) {
      var ret = LocalizedMessages.Instance.Get(message, culture);
      if (ret is null) {
        // If the message has not been localized, returns the enum name with its argument
        // placeholders as the result.
        (string name, int requiredArgumentNumber) = Parse(message);
        var sb = new StringBuilder(name);
        for (var i = 0; i < requiredArgumentNumber; i++) {
          sb.AppendFormat(" {{{0}}}", i);
        }
        return sb.ToString();
      } else {
        return ret;
      }
    }

    // Formats the message string with the input arguments, using the current CultureInfo
    public static string Format(this Message message, params string[] arguments) {
      return string.Format(Get(message), arguments);
    }

    // Formats the message string with the input arguments, using the specified CultureInfo.
    public static string Format(this Message message, CultureInfo culture,
                                params string[] arguments) {
      return string.Format(Get(message, culture), arguments);
    }

    private static (string name, int requiredArgumentNumber) Parse(Message message) {
      string name = Enum.GetName(typeof(Message), message);
      if (name.Length <= 0) {
        return ("", 0);
      }
      var lastChar = name[^1];
      if (!char.IsDigit(lastChar)) {
        return (name, 0);
      } else {
        return (name[..^1], (int)char.GetNumericValue(lastChar));
      }
    }
  }
}
