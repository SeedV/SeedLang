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
using System.Collections.Generic;
using System.Globalization;

namespace SeedLang.Common {
  // A singleton class to retrieve the localized strings of SeedLang messages.
  //
  // Compared to .Net's ResourceManager and Unity's Localization package, this implementation is
  // simpler and more straightforward. Hence it can be used in different hosting environments
  // without redefining localized strings.
  internal sealed class LocalizedMessages {
    public static LocalizedMessages Instance => _instance.Value;

    private static readonly Lazy<LocalizedMessages> _instance =
        new Lazy<LocalizedMessages>(() => new LocalizedMessages());

    // The locale that is used as the final fallback.
    private const string _defaultLocale = "en";

    // Defines localized messages here. Each message must contain a string of _defaultLocale.
    private static readonly Dictionary<Message, Dictionary<string, string>> _messages =
        new Dictionary<Message, Dictionary<string, string>>() {
          [Message.Okay] = new Dictionary<string, string>() {
            ["en"] = "Okay",
            ["zh-CN"] = "好",
          },

          [Message.Yes] = new Dictionary<string, string>() {
            ["en"] = "Yes",
            ["zh-CN"] = "是",
          },

          [Message.No] = new Dictionary<string, string>() {
            ["en"] = "No",
            ["zh-CN"] = "否",
          },

          // Please keep the following message IDs in alphabetical order.

          [Message.EngineNotPaused] = new Dictionary<string, string>() {
            ["en"] = "The program is already running",
            ["zh-CN"] = "程序已在运行中",
          },

          [Message.EngineProgramNotCompiled] = new Dictionary<string, string>() {
            ["en"] = "The program hasn't been compiled",
            ["zh-CN"] = "程序尚未编译",
          },

          [Message.RuntimeErrorBreakOutsideLoop] = new Dictionary<string, string>() {
            ["en"] = "'break' outside loop",
            ["zh-CN"] = "'break' 语句位于循环之外",
          },

          [Message.RuntimeErrorContinueOutsideLoop] = new Dictionary<string, string>() {
            ["en"] = "'continue' outside loop",
            ["zh-CN"] = "'continue' 语句位于循环之外",
          },

          [Message.RuntimeErrorDivideByZero] = new Dictionary<string, string>() {
            ["en"] = "Division by zero",
            ["zh-CN"] = "除 0 错",
          },

          [Message.RuntimeErrorIncorrectArgsCount] = new Dictionary<string, string>() {
            ["en"] = "Mismatched number of arguments",
            ["zh-CN"] = "参数个数不匹配",
          },

          [Message.RuntimeErrorIncorrectSliceAssignCount] = new Dictionary<string, string>() {
            ["en"] = "Attempt to assign sequence to a slice with different length",
            ["zh-CN"] = "为切片赋值时，值序列的长度不匹配",
          },

          [Message.RuntimeErrorIncorrectUnpackCount] = new Dictionary<string, string>() {
            ["en"] = "Mismatched number of values to unpack",
            ["zh-CN"] = "要解包的值的个数不匹配",
          },

          [Message.RuntimeErrorInvalidCast] = new Dictionary<string, string>() {
            ["en"] = "Invalid cast",
            ["zh-CN"] = "不支持的类型转换",
          },

          [Message.RuntimeErrorInvalidInteger] = new Dictionary<string, string>() {
            ["en"] = "Need an integer number",
            ["zh-CN"] = "需要一个整数",
          },

          [Message.RuntimeErrorNoKey] = new Dictionary<string, string>() {
            ["en"] = "Key error",
            ["zh-CN"] = "键错误",
          },

          [Message.RuntimeErrorNotCallable] = new Dictionary<string, string>() {
            ["en"] = "The object is not callable",
            ["zh-CN"] = "对象不可调用",
          },

          [Message.RuntimeErrorNotCountable] = new Dictionary<string, string>() {
            ["en"] = "The object is not countable",
            ["zh-CN"] = "对象不可计数",
          },

          [Message.RuntimeErrorNotSubscriptable] = new Dictionary<string, string>() {
            ["en"] = "The object is not subscriptable",
            ["zh-CN"] = "对象不可用下标访问",
          },

          [Message.RuntimeErrorNotSupportAssignment] = new Dictionary<string, string>() {
            ["en"] = "The object is not assignable",
            ["zh-CN"] = "对象不可赋值",
          },

          [Message.RuntimeErrorOutOfRange] = new Dictionary<string, string>() {
            ["en"] = "Out of range",
            ["zh-CN"] = "越界错误",
          },

          [Message.RuntimeErrorOverflow] = new Dictionary<string, string>() {
            ["en"] = "Overflow",
            ["zh-CN"] = "溢出错误",
          },

          [Message.RuntimeErrorSliceAssignNotIterable] = new Dictionary<string, string>() {
            ["en"] = "Attempt to assign not-iterable value to slice",
            ["zh-CN"] = "为切片赋值时，值不是可迭代对象",
          },

          [Message.RuntimeErrorUnhashableType] = new Dictionary<string, string>() {
            ["en"] = "Not a hashable type",
            ["zh-CN"] = "不可哈希的类型",
          },

          [Message.RuntimeErrorUnsupportedOperands] = new Dictionary<string, string>() {
            ["en"] = "Unsupported operands",
            ["zh-CN"] = "不支持的运算符",
          },

          [Message.RuntimeErrorVariableNotDefined] = new Dictionary<string, string>() {
            ["en"] = "The variable is not defined",
            ["zh-CN"] = "变量未定义",
          },

          [Message.SyntaxErrorFailedPredicate1] = new Dictionary<string, string>() {
            ["en"] = "Failed predicate {0}",
            ["zh-CN"] = "未成功的谓词 {0}",
          },

          [Message.SyntaxErrorInputMismatch2] = new Dictionary<string, string>() {
            ["en"] = "Mismatched input. Found token: {0}. Expected token: {1}",
            ["zh-CN"] = "不匹配的输入. 遇到的 token: {0}. 期望的 token: {1}",
          },

          [Message.SyntaxErrorMissingToken2] = new Dictionary<string, string>() {
            ["en"] = "Missing token. Found token: {0}. Expected token: {1}",
            ["zh-CN"] = "缺少 token. 遇到的 token: {0}. 期望的 token: {1}",
          },

          [Message.SyntaxErrorNoViableAlternative1] = new Dictionary<string, string>() {
            ["en"] = "No viable alternative at input {0}",
            ["zh-CN"] = "{0} 处没有可用备选",
          },

          [Message.SyntaxErrorUnwantedToken2] = new Dictionary<string, string>() {
            ["en"] = "Unwanted token. Found token: {0}. Expected token: {1}",
            ["zh-CN"] = "非期望 token. 遇到的 token: {0}. 期望的 token: {1}",
          },

          [Message.UnsupportedEvalSyntax] = new Dictionary<string, string>() {
            ["en"] = "Unsupported syntax",
            ["zh-CN"] = "不支持的语法",
          },
        };

    private readonly Dictionary<(Message message, int LCID), string> _index =
        new Dictionary<(Message message, int LCID), string>();

    public string Get(Message message, CultureInfo culture) {
      while (true) {
        if (_index.TryGetValue((message, culture.LCID), out string result)) {
          return result;
        } else if (culture == CultureInfo.InvariantCulture) {
          // Every message defined in _messages has a CultureInfo.InvariantCulture entry. Thus the
          // lack of the CultureInfo.InvariantCulture entry implies that the message is not defined
          // at all.
          return null;
        } else {
          // In case "pt-PT" is not defined, its parent locale, "pt", will be used as a fallback.
          // And if "pt" is not defined, CultureInfo.InvariantCulture will be used as the final
          // fallback.
          culture = culture.Parent;
        }
      }
    }

    private LocalizedMessages() {
      // Initializes the index table.
      foreach (var message in _messages.Keys) {
        foreach (var locale in _messages[message].Keys) {
          var culture = new CultureInfo(locale);
          var localizedString = _messages[message][locale];
          _index[(message, culture.LCID)] = localizedString;
          if (locale == _defaultLocale) {
            // Uses CultureInfo.InvariantCulture as the final fallback entry, which holds the
            // localized string of _defaultLocale.
            _index[(message, CultureInfo.InvariantCulture.LCID)] = localizedString;
          }
        }
      }
    }
  }
}
