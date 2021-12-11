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

namespace SeedLang.Runtime {
  // The value type used in the SeedLang virtual machine.
  //
  // Design consideration:
  // 1) It's designed as a value type (struct) to avoid object creating frequently. It's boxed into
  //    an IValue object and sent to the visualizer center when visualizers need be notified.
  // 2) "in" keyword is used when passing VMValue as a parameter of functions to avoid copying.
  // 3) "ref readonly" keywords are used when returning VMValue from a function to avoid copying.
  internal readonly struct Value : IValue, System.IEquatable<Value> {
    internal enum ValueType {
      None,
      Boolean,
      Number,
      String,
    }

    public bool IsNone => _type == ValueType.None;
    public bool IsBoolean => _type == ValueType.Boolean;
    public bool IsNumber => _type == ValueType.Number;
    public bool IsString => _type == ValueType.String;

    public bool Boolean {
      get {
        switch (_type) {
          case ValueType.None:
            return false;
          case ValueType.Boolean:
          case ValueType.Number:
            return ValueHelper.NumberToBoolean(_number);
          case ValueType.String:
            return ValueHelper.StringToBoolean(_object as string);
          default:
            throw new System.NotImplementedException($"Unsupported value type: {_type}.");
        }
      }
    }

    public double Number {
      get {
        switch (_type) {
          case ValueType.None:
            return 0;
          case ValueType.Boolean:
          case ValueType.Number:
            return _number;
          case ValueType.String:
            return ValueHelper.StringToNumber(_object as string);
          default:
            throw new System.NotImplementedException($"Unsupported value type: {_type}.");
        }
      }
    }

    public string String {
      get {
        switch (_type) {
          case ValueType.None:
            return "None";
          case ValueType.Boolean:
            return ValueHelper.BooleanToString(ValueHelper.NumberToBoolean(_number));
          case ValueType.Number:
            return ValueHelper.NumberToString(_number);
          case ValueType.String:
            return _object as string;
          default:
            throw new System.NotImplementedException($"Unsupported value type: {_type}.");
        }
      }
    }

    private readonly ValueType _type;
    private readonly double _number;
    private readonly object _object;

    internal Value(bool value) {
      _type = ValueType.Boolean;
      _number = ValueHelper.BooleanToNumber(value);
      _object = null;
    }

    internal Value(double value) {
      _type = ValueType.Number;
      _number = value;
      _object = null;
    }

    internal Value(string value) {
      _type = ValueType.String;
      _number = 0;
      _object = value;
    }

    public static bool operator ==(in Value lhs, in Value rhs) {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(in Value lhs, in Value rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(Value other) {
      switch (_type) {
        case ValueType.None:
          return other._type == ValueType.None;
        case ValueType.Boolean:
        case ValueType.Number:
          return (other._type == ValueType.Boolean || other._type == ValueType.Number) &&
                 _number == other._number;
        case ValueType.String:
          return other._type == ValueType.String && _object as string == other._object as string;
        default:
          throw new System.NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    public override bool Equals(object obj) {
      return obj is Value other && Equals(other);
    }

    public override int GetHashCode() {
      switch (_type) {
        case ValueType.None:
          return _type.GetHashCode();
        case ValueType.Boolean:
        case ValueType.Number:
          return (_type, _number).GetHashCode();
        case ValueType.String:
          return (_type, _object as string).GetHashCode();
        default:
          throw new System.NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    public override string ToString() {
      return String;
    }
  }
}
