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

namespace SeedLang.Interpreter {
  // The value type used in Seed Virtual Machine.
  internal struct Value : IValue {
    public ValueType Type { get; }

    private readonly double _number;

    internal Value(double number) {
      Type = ValueType.Number;
      _number = number;
    }

    public double ToNumber() {
      return _number;
    }

    public override string ToString() {
      return $"{_number}";
    }
  }
}
