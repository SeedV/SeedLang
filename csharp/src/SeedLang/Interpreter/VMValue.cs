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
  // Design consideration:
  // 1) It's designed as a value type (struct) to avoid object creating frequently. It's boxed into
  //    an IValue object and sent to the visualizer center when visualizers need be notified.
  // 2) "in" keyword is used when passing VMValue as a parameter of functions to avoid copying.
  // 3) "ref readonly" keywords are used when returning VMValue from a function to avoid copying.
  internal readonly struct VMValue : IValue, System.IEquatable<VMValue> {
    public ValueType Type { get; }

    public bool Boolean {
      get {
        switch (Type) {
          case ValueType.Null:
            return false;
          case ValueType.Boolean:
          case ValueType.Number:
            return ValueHelper.NumberToBoolean(_number);
          case ValueType.String:
            return ValueHelper.StringToBoolean(_object as string);
          default:
            throw new System.NotImplementedException($"Unsupported value type: {Type}.");
        }
      }
    }

    public double Number {
      get {
        switch (Type) {
          case ValueType.Null:
            return 0;
          case ValueType.Boolean:
          case ValueType.Number:
            return _number;
          case ValueType.String:
            return ValueHelper.StringToNumber(_object as string);
          default:
            throw new System.NotImplementedException($"Unsupported value type: {Type}.");
        }
      }
    }

    public string String {
      get {
        switch (Type) {
          case ValueType.Null:
            return "";
          case ValueType.Boolean:
            return ValueHelper.BooleanToString(ValueHelper.NumberToBoolean(_number));
          case ValueType.Number:
            return ValueHelper.NumberToString(_number);
          case ValueType.String:
            return _object as string;
          default:
            throw new System.NotImplementedException($"Unsupported value type: {Type}.");
        }
      }
    }

    private readonly double _number;
    private readonly object _object;

    internal VMValue(bool value) {
      Type = ValueType.Boolean;
      _number = ValueHelper.BooleanToNumber(value);
      _object = null;
    }

    internal VMValue(double value) {
      Type = ValueType.Number;
      _number = value;
      _object = null;
    }

    internal VMValue(string value) {
      Type = ValueType.String;
      _number = 0;
      _object = value;
    }

    public static bool operator ==(VMValue lhs, VMValue rhs) {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(VMValue lhs, VMValue rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(VMValue other) {
      if (Type != other.Type) {
        return false;
      }
      switch (Type) {
        case ValueType.Null:
          return true;
        case ValueType.Boolean:
        case ValueType.Number:
          return _number == other._number;
        case ValueType.String:
          return _object as string == other._object as string;
        default:
          throw new System.NotImplementedException($"Unsupported value type: {Type}.");
      }
    }

    public override bool Equals(object obj) {
      return obj is VMValue other && Equals(other);
    }

    public override int GetHashCode() {
      switch (Type) {
        case ValueType.Null:
          return Type.GetHashCode();
        case ValueType.Boolean:
        case ValueType.Number:
          return (Type, _number).GetHashCode();
        case ValueType.String:
          return (Type, _object as string).GetHashCode();
        default:
          throw new System.NotImplementedException($"Unsupported value type: {Type}.");
      }
    }

    public override string ToString() {
      return String;
    }
  }
}
