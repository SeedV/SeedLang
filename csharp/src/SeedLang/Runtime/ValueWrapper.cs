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

namespace SeedLang.Runtime {
  // A wrapper class that implements the IValue interface and delgate the calls to the wrapped
  // SeedLang value object. It's used in the notification events for visualizers.
  internal class ValueWrapper : IValue {
    private readonly Value _value;

    internal ValueWrapper(Value value) {
      _value = value;
    }

    public bool IsNil => _value.IsNil;
    public bool IsBoolean => _value.IsBoolean;
    public bool IsNumber => _value.IsNumber;
    public bool IsString => _value.IsString;
    public bool IsList => _value.IsList;
    public bool IsFunction => _value.IsFunction;

    public int Length => _value.Length;

    public bool Boolean => _value.AsBoolean();
    public double Number => _value.AsNumber();
    public string String => _value.AsString();

    public override string ToString() {
      return _value.ToString();
    }
  }
}
