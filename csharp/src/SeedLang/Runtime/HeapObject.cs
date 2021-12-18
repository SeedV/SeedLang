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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeedLang.Common;

namespace SeedLang.Runtime {
  internal class HeapObject : IEquatable<HeapObject> {
    private static readonly HashSet<HeapObject> _visitedObjects = new HashSet<HeapObject>();
    private readonly object _object;
    private string _unsupportedObjectTypeMessage => $"Unsupported heap object type: {_object}";

    private HeapObject(string str) {
      _object = str;
    }

    private HeapObject(List<Value> list) {
      _object = list;
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
      switch (_object) {
        case string _:
          return _object == other._object;
        case List<Value> list:
          return list.SequenceEqual(other._object as List<Value>);
        default:
          throw new NotImplementedException(_unsupportedObjectTypeMessage);
      }
    }

    public override int GetHashCode() {
      return _object.GetHashCode();
    }

    public override string ToString() {
      return AsString();
    }

    internal static HeapObject String(string str) {
      return new HeapObject(str);
    }

    internal static HeapObject List(List<Value> values) {
      return new HeapObject(values);
    }

    internal string AsString() {
      switch (_object) {
        case string str:
          return str;
        case List<Value> list:
          return ToString(list);
        default:
          throw new NotImplementedException(_unsupportedObjectTypeMessage);
      }
    }

    internal int Count() {
      switch (_object) {
        case string str:
          return str.Length;
        case List<Value> list:
          return list.Count;
        default:
          throw new NotImplementedException(_unsupportedObjectTypeMessage);
      }
    }

    internal Value this[double index] {
      get {
        switch (_object) {
          case string str:
            return Value.String(str[ToIntIndex(index, str.Length)].ToString());
          case List<Value> list:
            return list[ToIntIndex(index, list.Count)];
          default:
            throw new NotImplementedException(_unsupportedObjectTypeMessage);
        }
      }
      set {
        switch (_object) {
          case string _:
            throw new NotImplementedException("");
          case List<Value> list:
            list[ToIntIndex(index, list.Count)] = value;
            break;
          default:
            throw new NotImplementedException(_unsupportedObjectTypeMessage);
        }
      }
    }

    private static int ToIntIndex(double index, int length) {
      var intIndex = (int)index;
      if (intIndex != index) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorInvalidListIndex);
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
