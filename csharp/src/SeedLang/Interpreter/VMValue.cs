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
  // The value type used in the SeedLang virtual machine.
  //
  // It's designed as a value type (struct) to avoid object creating frequently. A coresponding
  // runtime Value is genrated and sent to the visualizer center when visualizers need be notified.
  internal struct VMValue {
    private readonly ValueType _type;
    private readonly double _number;
    private readonly object _object;

    internal VMValue(double number) {
      _type = ValueType.Number;
      _number = number;
      _object = null;
    }

    internal VMValue(string str) {
      _type = ValueType.String;
      _number = 0;
      _object = str;
    }

    public override string ToString() {
      switch (_type) {
        case ValueType.Number:
          return $"{_number}";
        case ValueType.String:
          return $"{_object}";
        default:
          throw new System.NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal double ToNumber() {
      switch (_type) {
        case ValueType.Number:
          return _number;
        case ValueType.String:
          // TODO: need parse string to number?
          return 0;
        default:
          throw new System.NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal Value ToValue() {
      switch (_type) {
        case ValueType.Number:
          return new NumberValue(_number);
        case ValueType.String:
          return new StringValue(_object as string);
        default:
          throw new System.NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    public static VMValue operator +(VMValue lhs, VMValue rhs) {
      return new VMValue(lhs._number + rhs._number);
    }

    public static VMValue operator -(VMValue lhs, VMValue rhs) {
      return new VMValue(lhs._number - rhs._number);
    }

    public static VMValue operator *(VMValue lhs, VMValue rhs) {
      return new VMValue(lhs._number * rhs._number);
    }

    public static VMValue operator /(VMValue lhs, VMValue rhs) {
      return new VMValue(lhs._number / rhs._number);
    }
  }
}
