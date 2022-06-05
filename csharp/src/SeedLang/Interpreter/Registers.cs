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
using System.Diagnostics;
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  internal class Registers {
    internal class Span {
      public int Count { get; }
      private readonly VMValue[] _values;
      private readonly int _start;

      internal Span(VMValue[] values, int start, int count) {
        _values = values;
        _start = start;
        Count = count;
      }

      internal ref readonly VMValue this[int index] {
        get {
          Debug.Assert(_start + index < Count);
          return ref _values[_start + index];
        }
      }
    }

    // The class to store variable information of registers.
    internal class RegisterInfo {
      private enum Type {
        Temporary,
        Local,
        Reference,
      }

      public bool IsTemporary => _type == Type.Temporary;
      public bool IsLocal => _type == Type.Local;
      public bool IsReference => _type == Type.Reference;

      public string Name { get; }
      public VariableType RefVariableType { get; }
      public IReadOnlyList<Value> Keys { get; }

      private readonly Type _type;

      internal RegisterInfo() {
        _type = Type.Temporary;
      }

      internal RegisterInfo(string name) {
        Name = name;
        _type = Type.Local;
      }

      internal RegisterInfo(string name, VariableType refVariableType, IReadOnlyList<Value> keys) {
        Name = name;
        RefVariableType = refVariableType;
        Keys = keys;
        _type = Type.Reference;
      }
    }

    public IReadOnlyList<IVM.VariableInfo> Locals {
      get {
        var locals = new List<IVM.VariableInfo>();
        for (int i = (int)Base; i < _registerInfos.Count; i++) {
          if (_registerInfos[i].IsLocal) {
            locals.Add(new IVM.VariableInfo(_registerInfos[i].Name, new Value(_stack[i])));
          }
        }
        return locals;
      }
    }

    public uint Base { get; set; }

    // The stack size. Each function can allocate maximun 250 registers in the stack. So the stack
    // can hold maximun 100 recursive function calls.
    private const int _stackSize = 25 * 1024;
    private readonly VMValue[] _stack = new VMValue[_stackSize];
    private List<RegisterInfo> _registerInfos;

    internal ref readonly VMValue GetValueAt(uint registerId) {
      return ref _stack[Base + registerId];
    }

    internal void SetValueAt(uint registerId, in VMValue value) {
      _stack[Base + registerId] = value;
    }

    internal void Reset() {
      Base = 0;
      _registerInfos = new List<RegisterInfo>();
    }

    internal Span GetArguments(uint funcRegisterId, int count) {
      return new Span(_stack, (int)(Base + funcRegisterId + 1), count);
    }

    internal void SetReturnValue(in VMValue value) {
      if (Base > 0) {
        _stack[Base - 1] = value;
      }
    }

    internal RegisterInfo GetRegisterInfo(uint registerId) {
      int index = (int)(Base + registerId);
      Debug.Assert(index < _registerInfos.Count);
      return _registerInfos[index];
    }

    internal void SetTempRegisterAt(uint registerId) {
      SetRegisterInfoAt(registerId, new RegisterInfo());
    }

    internal void SetLocalRegisterInfoAt(uint registerId, string name) {
      SetRegisterInfoAt(registerId, new RegisterInfo(name));
    }

    internal void SetRefRegisterInfoAt(uint registerId, string name, VariableType refVariableType,
                                       IReadOnlyList<Value> keys) {
      SetRegisterInfoAt(registerId, new RegisterInfo(name, refVariableType, keys));
    }

    internal void DeleteRegisterInfoFrom(uint startId, Action<RegisterInfo> notifyAction) {
      var index = (int)(Base + startId);
      for (int i = _registerInfos.Count - 1; i >= index; i--) {
        if (_registerInfos[i].IsLocal) {
          notifyAction(_registerInfos[i]);
        }
      }
      _registerInfos.RemoveRange(index, _registerInfos.Count - index);
    }

    private void SetRegisterInfoAt(uint registerId, RegisterInfo info) {
      int index = (int)(Base + registerId);
      if (index < _registerInfos.Count) {
        _registerInfos[index] = info;
      } else {
        for (int i = _registerInfos.Count; i < index; i++) {
          _registerInfos.Add(new RegisterInfo());
        }
        _registerInfos.Add(info);
      }
    }
  }
}
