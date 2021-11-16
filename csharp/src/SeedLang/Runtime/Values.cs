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
  public enum ValueType {
    Null,
    Boolean,
    Number,
    String,
  }

  public interface IValue {
    ValueType Type { get; }
    bool Boolean { get; }
    double Number { get; }
    string String { get; }
  }

  // An immutable null value class.
  internal class NullValue : IValue, IEquatable<IValue> {
    public ValueType Type => ValueType.Null;
    public bool Boolean => false;
    public double Number => 0;
    public string String => "";

    public static bool operator ==(NullValue lhs, NullValue rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(NullValue lhs, NullValue rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(IValue other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (other.Type != ValueType.Null) {
        return false;
      }
      return true;
    }

    public override bool Equals(object obj) {
      return Equals(obj as IValue);
    }

    public override int GetHashCode() {
      return Type.GetHashCode();
    }

    public override string ToString() {
      return String;
    }
  }

  // An immutable boolean value class.
  internal class BooleanValue : IValue, IEquatable<IValue> {
    public ValueType Type => ValueType.Boolean;
    public bool Boolean { get; }
    public double Number => ValueHelper.BooleanToNumber(Boolean);
    public string String => ValueHelper.BooleanToString(Boolean);

    internal BooleanValue(bool value) {
      Boolean = value;
    }

    public static bool operator ==(BooleanValue lhs, BooleanValue rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(BooleanValue lhs, BooleanValue rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(IValue other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (other.Type != ValueType.Boolean && other.Type != ValueType.Number) {
        return false;
      }
      return Number == other.Number;
    }

    public override bool Equals(object obj) {
      return Equals(obj as IValue);
    }

    public override int GetHashCode() {
      return Type.GetHashCode();
    }

    public override string ToString() {
      return String;
    }
  }

  // An immutable number value class.
  internal class NumberValue : IValue, IEquatable<IValue> {
    public ValueType Type => ValueType.Number;
    public bool Boolean => ValueHelper.NumberToBoolean(Number);
    public double Number { get; }
    public string String => ValueHelper.NumberToString(Number);

    internal NumberValue(double value = 0) {
      Number = value;
    }

    public static bool operator ==(NumberValue lhs, NumberValue rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(NumberValue lhs, NumberValue rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(IValue other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (other.Type != ValueType.Boolean && other.Type != ValueType.Number) {
        return false;
      }
      return Number == other.Number;
    }

    public override bool Equals(object obj) {
      return Equals(obj as IValue);
    }

    public override int GetHashCode() {
      return Type.GetHashCode();
    }

    public override string ToString() {
      return String;
    }
  }

  // An immutable string value class.
  internal class StringValue : IValue, IEquatable<IValue> {
    public ValueType Type => ValueType.String;
    public bool Boolean => ValueHelper.StringToBoolean(String);
    public double Number => ValueHelper.StringToNumber(String);
    public string String { get; }

    internal StringValue(string value = "") {
      String = value;
    }

    public static bool operator ==(StringValue lhs, StringValue rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(StringValue lhs, StringValue rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(IValue other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (other.Type != ValueType.String) {
        return false;
      }
      return String == other.String;
    }

    public override bool Equals(object obj) {
      return Equals(obj as IValue);
    }

    public override int GetHashCode() {
      return Type.GetHashCode();
    }

    public override string ToString() {
      return String;
    }
  }
}
