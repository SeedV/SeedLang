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
using System.Text;
using SeedLang.Common;

namespace SeedLang.Runtime {
  using List = List<Value>;

  // A class to hold heap allocated objects.
  internal class HeapObject : IEquatable<HeapObject> {
    public bool IsString => _object is string;
    public bool IsList => _object is List;
    public bool IsFunction => _object is IFunction;
    public bool IsIterator => _object is Iterator;
    public bool IsRange => _object is NumberRange;

    public int Length {
      get {
        switch (_object) {
          case string str:
            return str.Length;
          case List list:
            return list.Count;
          case NumberRange range:
            return range.Length;
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotCountable);
        }
      }
    }

    private static readonly HashSet<HeapObject> _visitedObjects = new HashSet<HeapObject>();

    private readonly object _object;

    public HeapObject(object obj) {
      _object = obj;
    }

    public static bool operator ==(HeapObject lhs, HeapObject rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(HeapObject lhs, HeapObject rhs) {
      return !(lhs == rhs);
    }

    public override bool Equals(object obj) {
      return Equals(obj as HeapObject);
    }

    public bool Equals(HeapObject other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      if (GetType() != other.GetType()) {
        return false;
      }
      // Compares contents for string types, and compares references for other types. This behavior
      // is consistent with GetHashCode implementation.
      return _object == other._object;
    }

    public override int GetHashCode() {
      return _object.GetHashCode();
    }

    public override string ToString() {
      return AsString();
    }

    internal bool AsBoolean() {
      switch (_object) {
        case string str:
          return ValueHelper.StringToBoolean(str);
        case List list:
          return list.Count != 0;
        case NumberRange range:
          return range.Length != 0;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal double AsNumber() {
      switch (_object) {
        case string str:
          return ValueHelper.StringToNumber(str);
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal string AsString() {
      switch (_object) {
        case string str:
          return str;
        case List list:
          return ToString(list);
        case IFunction func:
          return func.ToString();
        case NumberRange range:
          return range.ToString();
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal List AsList() {
      switch (_object) {
        case List list:
          return list;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal IFunction AsFunction() {
      switch (_object) {
        case IFunction func:
          return func;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotCallable);
      }
    }

    internal Iterator AsIterator() {
      switch (_object) {
        case Iterator iter:
          return iter;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotCallable);
      }
    }

    internal Value this[double index] {
      get {
        switch (_object) {
          case string str:
            return new Value(str[ToIntIndex(index, str.Length)].ToString());
          case List list:
            return list[ToIntIndex(index, list.Count)];
          case NumberRange range:
            return range[ToIntIndex(index, range.Length)];
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
        }
      }
      set {
        switch (_object) {
          case string _:
            throw new NotImplementedException();
          case List list:
            list[ToIntIndex(index, list.Count)] = value;
            break;
          case NumberRange _:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSupportAssignment);
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
        }
      }
    }

    internal Value Call(Value[] args, int offset, int length) {
      switch (_object) {
        case IFunction func:
          return func.Call(args, offset, length);
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorNotCallable);
      }
    }

    private static int ToIntIndex(double index, int length) {
      var intIndex = (int)index;
      if (intIndex != index) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidIndex);
      } else if (intIndex < 0 || intIndex >= length) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorOutOfRange);
      }
      return intIndex;
    }

    private string ToString(IReadOnlyList<Value> values) {
      var sb = new StringBuilder();
      sb.Append('[');
      if (_visitedObjects.Contains(this)) {
        sb.Append("...");
      } else {
        _visitedObjects.Add(this);
        bool first = true;
        foreach (Value value in values) {
          if (first) {
            first = false;
          } else {
            sb.Append(", ");
          }
          sb.Append(value);
        }
        _visitedObjects.Remove(this);
      }
      sb.Append(']');
      return sb.ToString();
    }
  }
}
