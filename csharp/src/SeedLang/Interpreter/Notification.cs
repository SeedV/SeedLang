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

using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  using Func = System.Func<uint, Value>;

  internal abstract class Notification {
    protected readonly Range _range;

    internal Notification(Range range) {
      _range = range;
    }

    internal abstract void Notify(VisualizerCenter visualizerCenter, Func getRKValue);
  }

  internal class AssignmentNotification : Notification {
    private readonly string _name;
    private readonly VariableType _type;
    private readonly uint _valueId;

    internal AssignmentNotification(string name, VariableType type, uint valueId, Range range) :
        base(range) {
      _name = name;
      _type = type;
      _valueId = valueId;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var ae = new AssignmentEvent(_name, _type, new ValueWrapper(getRKValue(_valueId)), _range);
      visualizerCenter.AssignmentPublisher.Notify(ae);
    }
  }

  internal class BinaryNotification : Notification {
    private readonly uint _leftId;
    private readonly BinaryOperator _op;
    private readonly uint _rightId;
    private readonly uint _resultId;

    internal BinaryNotification(uint leftId, BinaryOperator op, uint rightId, uint resultId,
                                Range range) : base(range) {
      _leftId = leftId;
      _op = op;
      _rightId = rightId;
      _resultId = resultId;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var be = new BinaryEvent(new ValueWrapper(getRKValue(_leftId)), _op,
                               new ValueWrapper(getRKValue(_rightId)),
                               new ValueWrapper(getRKValue(_resultId)), _range);
      visualizerCenter.BinaryPublisher.Notify(be);
    }
  }
}
