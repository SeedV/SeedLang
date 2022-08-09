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

          [Message.SyntaxErrorMissingToken2] = new Dictionary<string, string>() {
            ["en"] = "Missing token. Found token: {0}. Expected token: {1}",
            ["zh-CN"] = "缺少 token. 遇到的 token: {0}. 期望的 token: {1}",
          },

          // TODO: localize all other message strings.
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
