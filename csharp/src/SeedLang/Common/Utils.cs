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

namespace SeedLang.Common {
  // Static common utilities.
  internal static class Utils {
    // Generates a timestamp string based on the current time.
    internal static string Timestamp() {
      return Timestamp(DateTime.Now);
    }

    // Generates a timestamp string based on the specified time.
    internal static string Timestamp(DateTime time) {
      string dateString = $"{time.Year:D4}{time.Month:D2}{time.Day:D2}";
      string timeString = $"{time.Hour:D2}{time.Minute:D2}{time.Second:D2}{time.Millisecond:D3}";
      return dateString + timeString;
    }
  }
}
