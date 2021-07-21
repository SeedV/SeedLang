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

using System;
using System.Text;

namespace SeedLang.Common {
  // The ID list of string messages.
  //
  // SeedLang uses an intermediate <MessageId, MessageString> system to separate the client usages
  // and the underlying implementation of localized messages.
  //
  // When developers come to a string which might be seen by the users, they simply add a new
  // PascalCased ID to the following Enum. The last digit of the ID indicates the number of
  // arguments that the string formatter requires.
  //
  // During the development stage, developers use code patterns
  //
  //    Message.SomeId.Get()
  //
  //    Message.SomeIdWithThreeArguments3.Format(arg1, arg2, arg3)
  //
  // to fill a string message in their code. The default behavior of the MessageHelper utility
  // generates a mock string according to the message ID. This is good enough for development and
  // debugging.
  //
  // Once the message strings are translated and served by an underlying implementation, such as
  // .Net's default ResourceManager, the MessageHelper utility will be updated to hide the
  // implementation details so that the client code need no change to adapt a real localization
  // mechanism.
  //
  // With this intermediate layer, we will be able to introduce lightweighted or cross-platform
  // localization solutions other than .Net's default ResourceManager when necessary.
  public enum Message {
    Okay,
    Yes,
    No,
    ExampleMessageWithOneArgument1,
    ExampleMessageWithTwoArguments2,
    ExampleMessageWithNineArguments9,
  }

  public static class MessageHelper {
    // Returns the original message string without formatting it. The required arguments will be
    // rendered as {n} inside the string. The trailing decimal digit of the message ID indicates the
    // number of the required arguments, ranging from 0 to 9.
    public static string Get(this Message message) {
      // TODO: Support the real localization system once the strings are localized.
      (string name, int requiredArgumentNumber) = Parse(message);
      var sb = new StringBuilder(name);
      for (var i = 0; i < requiredArgumentNumber; i++) {
        sb.AppendFormat(" {{{0}}}", i);
      }
      return sb.ToString();
    }

    // Formats the message string with the input arguments. The trailing decimal digit of the messag
    // ID indicates the number of the required arguments, ranging from 0 to 9.
    public static string Format(this Message message, params string[] arguments) {
      // TODO: Support the real localization system once the strings are localized.
      (string name, int requiredArgumentNumber) = Parse(message);
      if (requiredArgumentNumber > arguments.Length) {
        throw new ArgumentException($"Not enough arguments to format the string message: {name}.");
      }
      var sb = new StringBuilder(name);
      for (var i = 0; i < requiredArgumentNumber; i++) {
        sb.AppendFormat(" {0}", arguments[i]);
      }
      return sb.ToString();
    }

    static (string name, int requiredArgumentNumber) Parse(Message message) {
      string name = Enum.GetName(message);
      if (name.Length <= 0) {
        return ("", 0);
      }
      var lastChar = name[name.Length - 1];
      if (!char.IsDigit(lastChar)) {
        return (name, 0);
      } else {
        return (name.Substring(0, name.Length - 1), (int)char.GetNumericValue(lastChar));
      }
    }
  }
}
