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
using System.Diagnostics;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The class to cache the constants and notifications. It only adds the unique constant and
  // notification into the constant and notification list of the chunk.
  internal class ChunkCache {
    // The list to collect constant values during compilation.
    private readonly List<VMValue> _constants = new List<VMValue>();
    private readonly Dictionary<double, uint> _numbers = new Dictionary<double, uint>();
    private readonly Dictionary<string, uint> _strings = new Dictionary<string, uint>();

    // The list to collect notification information during compilation.
    private readonly List<Notification.AbstractNotification> _notifications =
        new List<Notification.AbstractNotification>();
    private readonly Dictionary<Notification.AbstractNotification, uint> _notificationMap =
        new Dictionary<Notification.AbstractNotification, uint>();

    internal VMValue[] ConstantArray() {
      return _constants.ToArray();
    }

    internal Notification.AbstractNotification[] NotificationArray() {
      return _notifications.ToArray();
    }

    // Returns the id of a given number constant. The number is added into the constant list if it
    // is not exist.
    internal uint IdOfConstant(double number) {
      if (!_numbers.ContainsKey(number)) {
        _constants.Add(new VMValue(number));
        _numbers[number] = IdOfLastConst();
      }
      return _numbers[number];
    }

    // Returns the id of a given string constant. The string is added into the constant list if it
    // is not exist.
    internal uint IdOfConstant(string str) {
      if (!_strings.ContainsKey(str)) {
        _constants.Add(new VMValue(str));
        _strings[str] = IdOfLastConst();
      }
      return _strings[str];
    }

    internal uint IdOfConstant(Function func) {
      _constants.Add(new VMValue(func));
      return IdOfLastConst();
    }

    internal uint IdOfNotification(Notification.AbstractNotification notification) {
      if (!_notificationMap.ContainsKey(notification)) {
        _notifications.Add(notification);
        _notificationMap[notification] = (uint)_notifications.Count - 1;
      }
      return _notificationMap[notification];
    }

    private uint IdOfLastConst() {
      Debug.Assert(_constants.Count >= 1);
      // Id of constants starts from MaxRegisterCount defined in Chunk.
      return (uint)_constants.Count - 1 + Chunk.MaxRegisterCount;
    }
  }
}
