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

namespace SeedLang.Runtime {
  // An immutable string value class.
  internal class StringValue : Value {
    private readonly string _value;

    public override ValueType Type => ValueType.String;

    internal StringValue(string value) {
      _value = value;
    }

    // TODO: decide if implicit cast from a string to a number is allowed (Python does not support)
    public override double ToNumber() {
      try {
        return double.Parse(_value);
      } catch (Exception) {
        return 0;
      }
    }

    public override string ToString() {
      return _value;
    }
  }
}
