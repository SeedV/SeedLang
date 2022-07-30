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
using System.Collections.Immutable;
using SeedLang.Common;
using SeedLang.Runtime.HeapObjects;

namespace SeedLang.Runtime {
  // The value type used in the SeedVM that can carry primary value types (Nil, Boolean, Number) and
  // reference value types (String, Function, Dict, List, Tuple, Range, Slice etc.)
  //
  // 1) It's designed as a value type (struct) to avoid object creating frequently. It's wrapped by
  //    a Value class defined in SeedLang.Visualization and sent to the visualizer center when
  //    visualizers need be notified.
  // 2) "in" keyword is used when passing Value as a parameter of functions to avoid copying.
  // 3) "ref readonly" keywords are used when returning Value from a function to avoid copying.
  internal readonly struct VMValue : IEquatable<VMValue> {
    internal enum ValueType {
      Nil,
      Boolean,
      Number,
      Object,
    }

    public bool IsNil => _type == ValueType.Nil;
    public bool IsBoolean => _type == ValueType.Boolean;
    public bool IsNumber => _type == ValueType.Number;
    public bool IsString => _type == ValueType.Object && _object.IsString;
    public bool IsFunction => _type == ValueType.Object && _object.IsFunction;
    public bool IsDict => _type == ValueType.Object && _object.IsDict;
    public bool IsList => _type == ValueType.Object && _object.IsList;
    public bool IsTuple => _type == ValueType.Object && _object.IsTuple;
    public bool IsRange => _type == ValueType.Object && _object.IsRange;
    public bool IsSlice => _type == ValueType.Object && _object.IsSlice;
    public bool IsModule => _type == ValueType.Object && _object.IsModule;

    public bool IsIterable => IsString || IsDict || IsList || IsTuple || IsRange;

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

    internal VMValue(bool value) {
      _type = ValueType.Boolean;
      _number = ValueHelper.BooleanToNumber(value);
      _object = null;
    }

    internal VMValue(double value) {
      _type = ValueType.Number;
      _number = value;
      _object = null;
    }

    internal VMValue(object obj) {
      _type = ValueType.Object;
      _number = 0;
      _object = new HeapObject(obj);
    }

    public static bool operator ==(in VMValue lhs, in VMValue rhs) {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(in VMValue lhs, in VMValue rhs) {
      return !(lhs == rhs);
    }

    public bool Equals(VMValue other) {
      return _type switch {
        ValueType.Nil => other._type == ValueType.Nil,
        var type when type == ValueType.Boolean || type == ValueType.Number =>
           (other._type == ValueType.Boolean || other._type == ValueType.Number) &&
           _number == other._number,
        ValueType.Object => _type == other._type && _object.Equals(other._object),
        _ => throw new NotImplementedException($"Unsupported value type: {_type}."),
      };
    }

    public override bool Equals(object obj) {
      return obj is VMValue other && Equals(other);
    }

    public override int GetHashCode() {
      return _type switch {
        ValueType.Nil => _type.GetHashCode(),
        var type when type == ValueType.Boolean || type == ValueType.Number =>
            new { _type, _number }.GetHashCode(),
        ValueType.Object => new { _type, _object }.GetHashCode(),
        _ => throw new NotImplementedException($"Unsupported value type: {_type}."),
      };
    }

    public override string ToString() {
      return _type switch {
        var type when type == ValueType.Nil || type == ValueType.Boolean ||
                      type == ValueType.Number => AsString(),
        ValueType.Object => _object.ToString(),
        _ => throw new NotImplementedException($"Unsupported value type: {_type}."),
      };
    }

    internal VMValue this[VMValue key] {
      get {
        if (_type == ValueType.Object) {
          return _object[key];
        } else {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotSubscriptable);
        }
      }
      set {
        if (_type == ValueType.Object) {
          _object[key] = value;
        } else {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotSubscriptable);
        }
      }
    }

    internal bool AsBoolean() {
      return _type switch {
        ValueType.Nil => false,
        var type when type == ValueType.Boolean || type == ValueType.Number =>
            ValueHelper.NumberToBoolean(_number),
        ValueType.Object => _object.AsBoolean(),
        _ => throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                           Message.RuntimeErrorInvalidCast),
      };
    }

    internal double AsNumber() {
      return _type switch {
        var type when type == ValueType.Boolean || type == ValueType.Number => _number,
        ValueType.Object => _object.AsNumber(),
        _ => throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                           Message.RuntimeErrorInvalidCast),
      };
    }

    internal string AsString() {
      return _type switch {
        ValueType.Nil => "None",
        ValueType.Boolean => ValueHelper.BooleanToString(ValueHelper.NumberToBoolean(_number)),
        ValueType.Number => ValueHelper.NumberToString(_number),
        ValueType.Object => _object.AsString(),
        _ => throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                           Message.RuntimeErrorInvalidCast),
      };
    }

    internal IFunction AsFunction() {
      if (_type == ValueType.Object) {
        return _object.AsFunction();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorNotCallable);
      }
    }

    internal Dictionary<VMValue, VMValue> AsDict() {
      if (_type == ValueType.Object) {
        return _object.AsDict();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }

    internal List<VMValue> AsList() {
      if (_type == ValueType.Object) {
        return _object.AsList();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }

    internal ImmutableArray<VMValue> AsTuple() {
      if (_type == ValueType.Object) {
        return _object.AsTuple();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }

    internal Slice AsSlice() {
      if (_type == ValueType.Object) {
        return _object.AsSlice();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }

    internal Module AsModule() {
      if (_type == ValueType.Object) {
        return _object.AsModule();
      } else {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidCast);
      }
    }
  }
}
