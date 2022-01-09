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
      String,
      List,
      Function,
    }

    private readonly ValueType _type;
    private readonly double _number;
    private readonly HeapObject _object;

    private Value(bool value) {
      _type = ValueType.Boolean;
      _number = ValueHelper.BooleanToNumber(value);
      _object = null;
    }

    private Value(double value) {
      _type = ValueType.Number;
      _number = value;
      _object = null;
    }

    private Value(string value) : this(ValueType.String, HeapObject.String(value)) { }

    private Value(ValueType type, HeapObject obj) {
      _type = type;
      _number = 0;
      _object = obj;
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
        case ValueType.List:
        case ValueType.Function:
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
        case ValueType.String:
        case ValueType.List:
        case ValueType.Function:
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
        case ValueType.String:
        case ValueType.List:
        case ValueType.Function:
          return _object.ToString();
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal static Value None() {
      return new Value();
    }

    internal static Value Boolean(bool value) {
      return new Value(value);
    }

    internal static Value Number(double value) {
      return new Value(value);
    }

    internal static Value String(string value) {
      return new Value(value);
    }

    internal static Value List(List<Value> values) {
      return new Value(ValueType.List, HeapObject.List(values));
    }

    internal static Value Function(IFunction value) {
      return new Value(ValueType.Function, HeapObject.Function(value));
    }

    internal Value this[double index] {
      get {
        switch (_type) {
          case ValueType.None:
          case ValueType.Boolean:
          case ValueType.Number:
          case ValueType.Function:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
          case ValueType.String:
          case ValueType.List:
            return _object[index];
          default:
            throw new NotImplementedException($"Unsupported value type: {_type}.");
        }
      }
      set {
        switch (_type) {
          case ValueType.None:
          case ValueType.Boolean:
          case ValueType.Number:
          case ValueType.Function:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
          case ValueType.String:
          case ValueType.List:
            _object[index] = value;
            break;
          default:
            throw new NotImplementedException($"Unsupported value type: {_type}.");
        }
      }
    }

    internal bool IsNone() { return _type == ValueType.None; }
    internal bool IsBoolean() { return _type == ValueType.Boolean; }
    internal bool IsNumber() { return _type == ValueType.Number; }
    internal bool IsString() { return _type == ValueType.String; }
    internal bool IsList() { return _type == ValueType.List; }
    internal bool IsFunction() { return _type == ValueType.Function; }

    internal bool AsBoolean() {
      switch (_type) {
        case ValueType.None:
          return false;
        case ValueType.Boolean:
        case ValueType.Number:
          return ValueHelper.NumberToBoolean(_number);
        case ValueType.String:
          return ValueHelper.StringToBoolean(_object.AsString());
        case ValueType.List:
          return Count() != 0;
        case ValueType.Function:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal double AsNumber() {
      switch (_type) {
        case ValueType.None:
          return 0;
        case ValueType.Boolean:
        case ValueType.Number:
          return _number;
        case ValueType.String:
          return ValueHelper.StringToNumber(_object.AsString());
        case ValueType.List:
        case ValueType.Function:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
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
        case ValueType.String:
        case ValueType.List:
        case ValueType.Function:
          return _object.AsString();
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal IFunction AsFunction() {
      switch (_type) {
        case ValueType.None:
        case ValueType.Boolean:
        case ValueType.Number:
        case ValueType.String:
        case ValueType.List:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                Message.RuntimeErrorNotCallable);
        case ValueType.Function:
          return _object.AsFunction();
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }

    }

    internal int Count() {
      switch (_type) {
        case ValueType.None:
        case ValueType.Boolean:
        case ValueType.Number:
        case ValueType.Function:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotCountable);
        case ValueType.String:
        case ValueType.List:
          return _object.Count();
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }

    internal Value Call(Value[] arguments) {
      switch (_type) {
        case ValueType.None:
        case ValueType.Boolean:
        case ValueType.Number:
        case ValueType.String:
        case ValueType.List:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotCallable);
        case ValueType.Function:
          return _object.Call(arguments);
        default:
          throw new NotImplementedException($"Unsupported value type: {_type}.");
      }
    }
  }
}
