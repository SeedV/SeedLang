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

using SeedLang.Runtime;

namespace SeedLang.Ast {
  // The base class of all the value classes that are used during AST tree execution.
  internal abstract class BaseValue : IValue {
    public abstract ValueType Type { get; }

    public abstract double ToNumber();

    public static BaseValue operator +(BaseValue lhs, BaseValue rhs) {
      return new NumberValue(lhs.ToNumber() + rhs.ToNumber());
    }

    public static BaseValue operator -(BaseValue lhs, BaseValue rhs) {
      return new NumberValue(lhs.ToNumber() - rhs.ToNumber());
    }

    public static BaseValue operator *(BaseValue lhs, BaseValue rhs) {
      return new NumberValue(lhs.ToNumber() * rhs.ToNumber());
    }

    public static BaseValue operator /(BaseValue lhs, BaseValue rhs) {
      return new NumberValue(lhs.ToNumber() / rhs.ToNumber());
    }
  }
}
