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

namespace SeedLang.Runtime {
  internal class HeapObject {
    private readonly object _object;

    private HeapObject(string str) {
      _object = str;
    }

    private HeapObject(List<Value> list) {
      _object = list;
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
          return list.ToString();
        default:
          throw new NotImplementedException($"Unsupported box type: {_object}");
      }
    }

    internal int Count() {
      switch (_object) {
        case string str:
          return str.Length;
        case List<Value> list:
          return list.Count;
        default:
          throw new NotImplementedException($"Unsupported box type: {_object}");
      }
    }

    internal Value this[int index] {
      get {
        switch (_object) {
          case string str:
            return Value.String(str[index].ToString());
          case List<Value> list:
            return list[index];
          default:
            throw new NotImplementedException($"Unsupported box type: {_object}");
        }
      }
      set {
        switch (_object) {
          case string _:
            throw new NotImplementedException("");
          case List<Value> list:
            list[index] = value;
            break;
          default:
            throw new NotImplementedException($"Unsupported box type: {_object}");
        }
      }
    }
  }
}
