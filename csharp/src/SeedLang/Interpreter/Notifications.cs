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

using System.Collections.Generic;
using System.Text;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  using Func = System.Func<uint, Value>;

  // The base class of all notification classes, which are used to store information for VISNOTIFY
  // instruction. The VM will create the correspding event object based on the information and send
  // to visualizers.
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
    // The constant or register id of the assigned value.
    private readonly uint _valueId;

    public override string ToString() {
      return $"AssignmentNotification: '{_name}': {_type} {_valueId} {_range}";
    }

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

    public override string ToString() {
      return $"BinaryNotification: {_leftId} {_op} {_rightId} {_resultId} {_range}";
    }

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

  internal class ComparisonNotification : Notification {
    private readonly uint _firstId;
    private readonly IReadOnlyList<ComparisonOperator> _ops;
    private readonly IReadOnlyList<uint> _valueIds;
    // The constant or register id of the result value if it is not null.
    private readonly uint? _resultId;
    // The boolean result of this comparison expression if _resultId is null. There is no register
    // to hold the result in this situation. It's implied by the different code execution paths.
    private readonly bool _result;

    internal ComparisonNotification(uint firstId, IReadOnlyList<ComparisonOperator> ops,
                                    IReadOnlyList<uint> valueIds, uint? resultId, bool result,
                                    Range range) : base(range) {
      _firstId = firstId;
      _ops = ops;
      _valueIds = valueIds;
      _resultId = resultId;
      _result = result;
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.Append("ComparisonNotification: ");
      sb.Append($"{_firstId} ");
      for (int i = 0; i < _ops.Count; i++) {
        sb.Append($"{_ops[i]} ");
        sb.Append($"{_valueIds[i]} ");
      }
      sb.Append(_resultId.HasValue ? _resultId : _result);
      sb.Append($" {_range}");
      return sb.ToString();
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var values = new IValue[_valueIds.Count];
      for (int i = 0; i < _valueIds.Count; i++) {
        values[i] = new ValueWrapper(getRKValue(_valueIds[i]));
      }
      var result = _resultId.HasValue ? new ValueWrapper(getRKValue((uint)_resultId)) :
                                        new ValueWrapper(new Value(_result));
      var ce = new ComparisonEvent(new ValueWrapper(getRKValue(_firstId)), _ops, values, result,
                                   _range);
      visualizerCenter.ComparisonPublisher.Notify(ce);
    }
  }

  internal class UnaryNotification : Notification {
    private readonly UnaryOperator _op;
    private readonly uint _valueId;
    private readonly uint _resultId;

    public override string ToString() {
      return $"UnaryNotification: {_op} {_valueId} {_resultId} {_range}";
    }

    internal UnaryNotification(UnaryOperator op, uint valueId, uint resultId, Range range) :
        base(range) {
      _op = op;
      _valueId = valueId;
      _resultId = resultId;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var ue = new UnaryEvent(_op, new ValueWrapper(getRKValue(_valueId)),
                              new ValueWrapper(getRKValue(_resultId)), _range);
      visualizerCenter.UnaryPublisher.Notify(ue);
    }
  }
}
