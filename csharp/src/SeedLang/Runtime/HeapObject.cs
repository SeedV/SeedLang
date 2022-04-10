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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SeedLang.Common;

namespace SeedLang.Runtime {
  using List = List<Value>;
  using Tuple = ImmutableArray<Value>;
  using Dict = Dictionary<Value, Value>;

  // A class to hold heap allocated objects.
  internal partial class HeapObject : IEquatable<HeapObject> {
    public bool IsString => _object is string;
    public bool IsList => _object is List;
    public bool IsFunction => _object is IFunction;
    public bool IsRange => _object is Range;
    public bool IsTuple => _object is Tuple;
    public bool IsDict => _object is Dict;

    public int Length {
      get {
        switch (_object) {
          case string str:
            return str.Length;
          case List list:
            return list.Count;
          case Range range:
            return range.Length;
          case Tuple tuple:
            return tuple.Length;
          case Dict dict:
            return dict.Count;
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotCountable);
        }
      }
    }

    private static readonly HashSet<HeapObject> _visitedObjects = new HashSet<HeapObject>();

    private readonly object _object;

    public HeapObject(object obj) {
      switch (obj) {
        case Dict dict:
          foreach (var item in dict) {
            CheckKey(item.Key);
          }
          break;
        default:
          break;
      }
      _object = obj;
      Debug.Assert(IsString || IsList || IsFunction || IsRange || IsTuple || IsDict,
                   $"Unsupported object type: {_object.GetType()}");
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
      if (_object.GetType() != other._object.GetType()) {
        return false;
      }
      // Compares contents for String, Tuple, List and Dict types.
      switch (_object) {
        case string str:
          return str == other._object as string;
        case Tuple tuple:
          return tuple.SequenceEqual((Tuple)other._object);
        case List list:
          return list.SequenceEqual(other._object as List);
        case Dict dict:
          return dict.SequenceEqual(other._object as Dict);
        default:
          return _object == other._object;
      }
    }

    // Computes hash code based on contents for String (uses csharp default implementation) and
    // tuple types. Uses csharp default implementation for List and Dict types, because they are
    // unhashable types, and this method is not used for them.
    public override int GetHashCode() {
      switch (_object) {
        case Tuple _:
          var equ = (IStructuralEquatable)_object;
          return equ.GetHashCode(StructuralComparisons.StructuralEqualityComparer);
        default:
          return _object.GetHashCode();
      }
    }

    public override string ToString() {
      return IsString ? $"'{AsString()}'" : AsString();
    }

    internal bool AsBoolean() {
      switch (_object) {
        case string str:
          return ValueHelper.StringToBoolean(str);
        case List list:
          return list.Count != 0;
        case Range range:
          return range.Length != 0;
        case Tuple tuple:
          return tuple.Length != 0;
        case Dict dict:
          return dict.Count != 0;
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
          return ListToString(list);
        case IFunction func:
          return func.ToString();
        case Range range:
          return range.ToString();
        case Tuple tuple:
          return TupleToString(tuple);
        case Dict dict:
          return DictToString(dict);
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

    internal Tuple AsTuple() {
      switch (_object) {
        case Tuple tuple:
          return tuple;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal Dict AsDict() {
      switch (_object) {
        case Dict dict:
          return dict;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal Value this[Value key] {
      get {
        switch (_object) {
          case string str:
            return new Value(str[ToIntIndex(key.AsNumber(), str.Length)].ToString());
          case List list:
            return list[ToIntIndex(key.AsNumber(), list.Count)];
          case Range range:
            return range[ToIntIndex(key.AsNumber(), range.Length)];
          case Tuple tuple:
            return tuple[ToIntIndex(key.AsNumber(), tuple.Length)];
          case Dict dict:
            CheckKey(key);
            if (dict.ContainsKey(key)) {
              return dict[key];
            }
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNoKey);
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
            list[ToIntIndex(key.AsNumber(), list.Count)] = value;
            break;
          case Dict dict:
            CheckKey(key);
            dict[key] = value;
            break;
          case Range _:
          case Tuple _:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSupportAssignment);
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
        }
      }
    }

    private static int ToIntIndex(double index, int length) {
      var intIndex = (int)index;
      if (intIndex != index) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidIndex);
      } else if (intIndex < -length || intIndex >= length) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorOutOfRange);
      }
      return intIndex < 0 ? length + intIndex : intIndex;
    }

    private static void CheckKey(Value key) {
      if (!key.IsNil && !key.IsBoolean && !key.IsNumber && !key.IsString && !key.IsRange &&
          !key.IsTuple) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorUnhashableType);
      }
    }

    private static string TupleToString(ImmutableArray<Value> tuple) {
      var sb = new StringBuilder();
      sb.Append('(');
      sb.Append(string.Join(", ", tuple));
      sb.Append(')');
      return sb.ToString();
    }

    private string ListToString(IReadOnlyList<Value> list) {
      var sb = new StringBuilder();
      sb.Append('[');
      if (_visitedObjects.Contains(this)) {
        sb.Append("...");
      } else {
        _visitedObjects.Add(this);
        sb.Append(string.Join(", ", list));
        _visitedObjects.Remove(this);
      }
      sb.Append(']');
      return sb.ToString();
    }

    private string DictToString(IReadOnlyDictionary<Value, Value> dict) {
      var sb = new StringBuilder();
      sb.Append('{');
      if (_visitedObjects.Contains(this)) {
        sb.Append("...");
      } else {
        _visitedObjects.Add(this);
        bool first = true;
        foreach (var item in dict) {
          if (first) {
            first = false;
          } else {
            sb.Append(", ");
          }
          sb.Append(item.Key);
          sb.Append(": ");
          sb.Append(item.Value);
        }
        _visitedObjects.Remove(this);
      }
      sb.Append('}');
      return sb.ToString();
    }
  }
}
