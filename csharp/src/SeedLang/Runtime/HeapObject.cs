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
  using IFunction = HeapObjects.IFunction;
  using Dict = Dictionary<VMValue, VMValue>;
  using List = List<VMValue>;
  using Tuple = ImmutableArray<VMValue>;
  using Range = HeapObjects.Range;
  using Slice = HeapObjects.Slice;

  // A class to hold heap allocated objects.
  internal partial class HeapObject : IEquatable<HeapObject> {
    public bool IsString => _object is string;
    public bool IsFunction => _object is IFunction;
    public bool IsDict => _object is Dict;
    public bool IsList => _object is List;
    public bool IsTuple => _object is Tuple;
    public bool IsRange => _object is Range;
    public bool IsSlice => _object is Slice;

    public int Length {
      get {
        switch (_object) {
          case string str:
            return str.Length;
          case Dict dict:
            return dict.Count;
          case List list:
            return list.Count;
          case Tuple tuple:
            return tuple.Length;
          case Range range:
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
      Debug.Assert(IsString || IsFunction || IsDict || IsList || IsTuple || IsRange || IsSlice,
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
        case Dict dict:
          return dict.SequenceEqual(other._object as Dict);
        case List list:
          return list.SequenceEqual(other._object as List);
        case Tuple tuple:
          return tuple.SequenceEqual((Tuple)other._object);
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
        case Dict dict:
          return dict.Count != 0;
        case List list:
          return list.Count != 0;
        case Tuple tuple:
          return tuple.Length != 0;
        case Range range:
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
        case IFunction func:
          return func.ToString();
        case Dict dict:
          return DictToString(dict);
        case List list:
          return ListToString(list);
        case Tuple tuple:
          return TupleToString(tuple);
        case Range range:
          return range.ToString();
        case Slice slice:
          return slice.ToString();
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

    internal Dict AsDict() {
      switch (_object) {
        case Dict dict:
          return dict;
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

    internal Tuple AsTuple() {
      switch (_object) {
        case Tuple tuple:
          return tuple;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal Slice AsSlice() {
      switch (_object) {
        case Slice slice:
          return slice;
        default:
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorInvalidCast);
      }
    }

    internal VMValue this[VMValue key] {
      get {
        switch (_object) {
          case string str:
            if (key.IsNumber) {
              return new VMValue(str[ToIntIndex(key.AsNumber(), str.Length)].ToString());
            }
            return SliceString(str, key.AsSlice());
          case Dict dict:
            CheckKey(key);
            if (dict.ContainsKey(key)) {
              return dict[key];
            }
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNoKey);
          case List list:
            if (key.IsNumber) {
              return list[ToIntIndex(key.AsNumber(), list.Count)];
            }
            return SliceList(list, key.AsSlice());
          case Tuple tuple:
            if (key.IsNumber) {
              return tuple[ToIntIndex(key.AsNumber(), tuple.Length)];
            }
            return SliceTuple(tuple, key.AsSlice());
          case Range range:
            return range[ToIntIndex(key.AsNumber(), range.Length)];
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
        }
      }
      set {
        switch (_object) {
          case Dict dict:
            CheckKey(key);
            dict[key] = value;
            break;
          case List list:
            if (key.IsNumber) {
              list[ToIntIndex(key.AsNumber(), list.Count)] = value;
            } else {
              AssignListSlice(list, key.AsSlice(), value);
            }
            break;
          case string _:
          case Tuple _:
          case Range _:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSupportAssignment);
          default:
            throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                          Message.RuntimeErrorNotSubscriptable);
        }
      }
    }

    private static VMValue SliceString(string str, Slice slice) {
      (int start, int stop, int step) = AdjustSliceInLength(slice, str.Length);
      var sb = new StringBuilder();
      SliceContainer(start, stop, step, i => sb.Append(str[i]));
      return new VMValue(sb.ToString());
    }

    private static VMValue SliceList(List list, Slice slice) {
      (int start, int stop, int step) = AdjustSliceInLength(slice, list.Count);
      var newList = new List<VMValue>();
      SliceContainer(start, stop, step, i => newList.Add(list[i]));
      return new VMValue(newList);
    }

    private static VMValue SliceTuple(Tuple tuple, Slice slice) {
      (int start, int stop, int step) = AdjustSliceInLength(slice, tuple.Length);
      var list = new List<VMValue>();
      SliceContainer(start, stop, step, i => list.Add(tuple[i]));
      return new VMValue(list.ToImmutableArray());
    }

    private static void SliceContainer(int start, int stop, int step, Action<int> copyItemAt) {
      if ((stop - start) * step > 0) {
        Func<int, bool> pred = step > 0 ? i => i < stop : i => i > stop;
        for (int i = start; pred(i); i += step) {
          copyItemAt(i);
        }
      }
    }

    private static void AssignListSlice(List list, Slice slice, VMValue value) {
      (int start, int stop, int step) = AdjustSliceInLength(slice, list.Count);
      if (step == 1) {
        if (stop > start) {
          list.RemoveRange(start, stop - start);
        }
        for (int i = value.Length - 1; i >= 0; i--) {
          list.Insert(start, value[new VMValue(i)]);
        }
      } else if ((stop - start) * step > 0) {
        Func<int, bool> pred = step > 0 ? i => i < stop : i => i > stop;
        int index = 0;
        for (int i = start; pred(i); i += step, index++) {
          list[i] = value[new VMValue(index)];
        }
        if (index != value.Length) {
          throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                        Message.RuntimeErrorSliceAssignmentCount);
        }
      }
    }

    private static (int, int, int) AdjustSliceInLength(Slice slice, int length) {
      int start, stop;
      int step = slice.Step ?? 1;
      if (!(slice.Start is null)) {
        start = slice.Start.Value < 0 ? slice.Start.Value + length : slice.Start.Value;
      } else {
        start = step > 0 ? 0 : length - 1;
      }
      if (!(slice.Stop is null)) {
        stop = slice.Stop.Value < 0 ? slice.Stop.Value + length : slice.Stop.Value;
      } else {
        stop = step > 0 ? length : -1;
      }
      if (start < 0 && stop < 0 || start >= length && stop >= length) {
        stop = start;
      }
      if (start < stop) {
        start = Math.Max(start, 0);
        stop = Math.Min(stop, length);
      } else if (start > stop) {
        start = Math.Min(start, length - 1);
        stop = Math.Max(stop, -1);
      }
      return (start, stop, step);
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

    private static void CheckKey(VMValue key) {
      if (!key.IsNil && !key.IsBoolean && !key.IsNumber && !key.IsString && !key.IsRange &&
          !key.IsTuple) {
        throw new DiagnosticException(SystemReporters.SeedRuntime, Severity.Fatal, "", null,
                                      Message.RuntimeErrorUnhashableType);
      }
    }

    private static string TupleToString(ImmutableArray<VMValue> tuple) {
      var sb = new StringBuilder();
      sb.Append('(');
      sb.Append(string.Join(", ", tuple));
      sb.Append(')');
      return sb.ToString();
    }

    private string ListToString(IReadOnlyList<VMValue> list) {
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

    private string DictToString(IReadOnlyDictionary<VMValue, VMValue> dict) {
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
