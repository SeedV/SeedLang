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
using System.Collections.Generic;
using SeedLang.Common;

namespace SeedLang.Runtime {
  // The value type used in the SeedLang core components.
  //
  // Design consideration:
  // 1) It's designed as a value type (struct) to avoid object creating frequently. It's wrapped by
  //    a ValueWrapper and sent to the visualizer center when visualizers need be notified.
  // 2) "in" keyword is used when passing Value as a parameter of functions to avoid copying.
  // 3) "ref readonly" keywords are used when returning Value from a function to avoid copying.
  internal readonly struct Value : IEquatable<Value> {
    internal enum ValueType {
      None,
      Boolean,
      Number,
      Object,
    }

    public bool IsNone => _type == ValueType.None;
    public bool IsBoolean => _type == ValueType.Boolean;
    public bool IsNumber => _type == ValueType.Number;
    public bool IsString => _type == ValueType.Object && _object.IsString;
    public bool IsList => _type == ValueType.Object && _object.IsList;
    public bool IsFunction => _type == ValueType.Object && _object.IsFunction;
    public bool IsRange => _type == ValueType.Object && _object.IsRange;
    public bool IsTuple => _type == ValueType.Object && _object.IsTuple;

    public int Length {
      get {
        if (_type == ValueType.Object) {
          return _object.Length;
        } else {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotCountable);
        }
      }
    }

    private readonly ValueType _type;
    private readonly double _number;
    private readonly HeapObject _object;

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

    internal Value(object obj) {
      _type = ValueType.Object;
      _number = 0;
      _object = new HeapObject(obj);
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
        case ValueType.Object:
          return _type == other._type && _object.Equals(other._object);
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
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
        case ValueType.Object:
          return (_type, _object).GetHashCode();
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    public override string ToString() {
      switch (_type) {
        case ValueType.None:
        case ValueType.Boolean:
        case ValueType.Number:
          return AsString();
        case ValueType.Object:
          return _object.ToString();
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal Value this[double index] {
      get {
        if (_type == ValueType.Object) {
          return _object[index];
        } else {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotSubscriptable);
        }
      }
      set {
        if (_type == ValueType.Object) {
          _object[index] = value;
        } else {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotSubscriptable);
        }
      }
    }

    internal bool AsBoolean() {
      switch (_type) {
        case ValueType.None:
          return false;
        case ValueType.Boolean:
        case ValueType.Number:
          return ValueHelper.NumberToBoolean(_number);
        case ValueType.Object:
          return _object.AsBoolean();
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal double AsNumber() {
      switch (_type) {
        case ValueType.None:
          return 0;
        case ValueType.Boolean:
        case ValueType.Number:
          return _number;
        case ValueType.Object:
          return _object.AsNumber();
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal string AsString() {
      switch (_type) {
        case ValueType.None:
          return "None";
        case ValueType.Boolean:
          return ValueHelper.BooleanToString(ValueHelper.NumberToBoolean(_number));
        case ValueType.Number:
          return ValueHelper.NumberToString(_number);
        case ValueType.Object:
          return _object.AsString();
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal List<Value> AsList() {
      if (_type == ValueType.Object) {
        return _object.AsList();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }

    internal HeapObject.IFunction AsFunction() {
      if (_type == ValueType.Object) {
        return _object.AsFunction();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorNotCallable);
      }
    }

    internal IReadOnlyList<Value> AsTuple() {
      if (_type == ValueType.Object) {
        return _object.AsTuple();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }
  }
}
