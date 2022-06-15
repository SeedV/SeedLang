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
using SeedLang.Runtime;

namespace SeedLang.Visualization {
  // A wrapper class of the underlying VMValue. It's used to represent runtime values in the
  // notification events of visualizers.
  public class Value : IEquatable<Value> {
    public bool IsNil => _value.IsNil;

    public bool IsBoolean => _value.IsBoolean;
    public bool IsNumber => _value.IsNumber;
    public bool IsString => _value.IsString;

    public bool IsDict => _value.IsDict;
    public bool IsList => _value.IsList;
    public bool IsTuple => _value.IsTuple;

    public int Length => _value.Length;

    private readonly VMValue _value;

    public Value() {
      _value = new VMValue();
    }

    public Value(bool boolean) {
      _value = new VMValue(boolean);
    }

    public Value(double number) {
      _value = new VMValue(number);
    }

    public Value(string str) {
      _value = new VMValue(str);
    }

    // Internal constructor that is only used by SeedVM to wrap the underlying VMValue.
    internal Value(VMValue value) {
      _value = value;
    }

    public static bool operator ==(in Value lhs, in Value rhs) {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(in Value lhs, in Value rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(Value other) {
      return _value.Equals(other._value);
    }

    public override bool Equals(object obj) {
      return obj is Value other && Equals(other);
    }

    public override int GetHashCode() {
      return _value.GetHashCode();
    }

    public bool AsBoolean() { return _value.AsBoolean(); }
    public double AsNumber() { return _value.AsNumber(); }
    public string AsString() { return _value.AsString(); }

    public Value this[Value key] {
      get {
        return new Value(_value[key._value]);
      }
    }

    public override string ToString() {
      return _value.ToString();
    }

    internal ref readonly VMValue GetRawValue() {
      return ref _value;
    }
  }
}
