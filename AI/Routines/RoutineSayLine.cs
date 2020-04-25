using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineSayLine : Routine {
        public MessageSpeech message;
        private ConditionBoolSwitch condition;
        public RoutineSayLine(GameObject g, Controller c, MessageSpeech message, ConditionBoolSwitch condition) : base(g, c) {
            this.message = message;
            this.condition = condition;
        }
        protected override status DoUpdate() {
            if (message == null)
                return status.neutral;
            if (!condition.conditionMet) {
                Toolbox.Instance.SendMessage(gameObject, control.controllable, message);
                condition.conditionMet = true;
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
}