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

using SeedLang.Runtime;

namespace SeedLang.Ast {
  // An immutable number value class.
  internal class NumberValue : BaseValue {
    private readonly double _value;

    public override ValueType Type {
      get {
        return ValueType.Number;
      }
    }

    internal NumberValue() {
      _value = 0;
    }

    internal NumberValue(double value) {
      _value = value;
    }

    public override double ToNumber() {
      return _value;
    }

    public override string ToString() {
      return $"{_value}";
    }
  }
}
