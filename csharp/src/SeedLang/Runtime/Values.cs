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

  internal class NullValue : IValue {
    public ValueType Type => ValueType.Null;
    public bool Boolean => false;
    public double Number => 0;
    public string String => "";

    public override string ToString() {
      return String;
    }
  }

  // An immutable boolean value class.
  internal class BooleanValue : IValue {
    public ValueType Type => ValueType.Number;
    public bool Boolean { get; }
    public double Number => ValueHelper.BooleanToNumber(Boolean);
    public string String => ValueHelper.BooleanToString(Boolean);

    internal BooleanValue(bool value) {
      Boolean = value;
    }

    public override string ToString() {
      return String;
    }
  }

  // An immutable number value class.
  internal class NumberValue : IValue {
    public ValueType Type => ValueType.Number;
    public bool Boolean => ValueHelper.NumberToBoolean(Number);
    public double Number { get; }
    public string String => ValueHelper.NumberToString(Number);

    internal NumberValue(double value = 0) {
      Number = value;
    }

    public override string ToString() {
      return String;
    }
  }

  // An immutable string value class.
  internal class StringValue : IValue {
    public ValueType Type => ValueType.String;
    public bool Boolean => ValueHelper.StringToBoolean(String);
    public double Number => ValueHelper.StringToNumber(String);
    public string String { get; }

    internal StringValue(string value = "") {
      String = value;
    }

    public override string ToString() {
      return String;
    }
  }
}
