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
  // The class to manage the register stack and register information.
  internal class Registers {
    // The class to store variable information of registers.
    internal class RegisterInfo {
      private enum Type {
        Local,
        Reference,
        Temporary,
      }

      // The flag to indicate a local variable is stored in the register.
      public bool IsLocal => _type == Type.Local;
      // The flag to indicate a reference of a global variable, or an element of a container is
      // stored in the register.
      public bool IsReference => _type == Type.Reference;
      // The flag to indicate a temporary variable is stored in the register.
      public bool IsTemporary => _type == Type.Temporary;

      // The name of the local or referenced variable.
      public string Name { get; }
      // The referenced variable type.
      public VariableType RefVariableType { get; }
      // The keys of the container elements, e.g. for a[0][1], 0 and 1 are stored in the keys.
      public IReadOnlyList<Value> Keys { get; }

      private readonly Type _type;

      // Constructs a temporary register information.
      internal RegisterInfo() {
        _type = Type.Temporary;
      }

      // Constructs a local register information.
      internal RegisterInfo(string name) {
        _type = Type.Local;
        Name = name;
        Keys = new List<Value>();
      }

      // Constructs a reference register information.
      internal RegisterInfo(string name, VariableType refVariableType, IReadOnlyList<Value> keys) {
        _type = Type.Reference;
        Name = name;
        RefVariableType = refVariableType;
        Keys = keys;
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

    // Base register index of current function.
    public uint Base { get; set; }

    // The stack size. Each function can allocate maximun 250 registers in the stack. So the stack
    // can hold maximun 100 recursive function calls.
    private const int _stackSize = 25 * 1024;

    // The stack of registers.
    private readonly VMValue[] _stack = new VMValue[_stackSize];

    // The list of register information.
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

    internal ValueSpan GetArguments(uint funcRegisterId, int count) {
      return new ValueSpan(_stack, (int)(Base + funcRegisterId + 1), count);
    }

    internal void SetReturnValue(in VMValue value) {
      if (Base > 0) {
        _stack[Base - 1] = value;
      }
    }

    // Gets register information for a given register id. Returns temporary register information if
    // the index is out of the range, because there isn't mechanism to track temporary variables in
    // the registers.
    internal RegisterInfo GetRegisterInfo(uint registerId) {
      int index = (int)(Base + registerId);
      return index < _registerInfos.Count ? _registerInfos[index] : new RegisterInfo();
    }

    internal void SetLocalRegisterInfoAt(uint registerId, string name) {
      SetRegisterInfoAt(registerId, new RegisterInfo(name));
    }

    internal void SetRefRegisterInfoAt(uint registerId, string name, VariableType refVariableType,
                                       IReadOnlyList<Value> keys) {
      SetRegisterInfoAt(registerId, new RegisterInfo(name, refVariableType, keys));
    }

    internal void DeleteRegisterInfoFrom(uint startId) {
      var index = (int)(Base + startId);
      if (index < _registerInfos.Count) {
        _registerInfos.RemoveRange(index, _registerInfos.Count - index);
      }
    }

    internal IReadOnlyList<string> NamesOfLocalVariableFrom(uint startId) {
      var index = (int)(Base + startId);
      var locals = new List<string>();
      for (int i = index; i < _registerInfos.Count; i++) {
        if (_registerInfos[i].IsLocal) {
          locals.Add(_registerInfos[i].Name);
        }
      }
      return locals;
    }

    private void SetRegisterInfoAt(uint registerId, RegisterInfo info) {
      int index = (int)(Base + registerId);
      if (index < _registerInfos.Count) {
        _registerInfos[index] = info;
      } else {
        while (index > _registerInfos.Count) {
          _registerInfos.Add(new RegisterInfo());
        }
        _registerInfos.Add(info);
      }
    }
  }
}
