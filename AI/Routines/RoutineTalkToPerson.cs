using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineTalkToPerson : Routine {
        public Ref<GameObject> target;
        private ConditionBoolSwitch condition;
        public Awareness awareness;
        public RoutineTalkToPerson(GameObject g, Controller c, Ref<GameObject> target, ConditionBoolSwitch condition, Awareness awareness) : base(g, c) {
            this.target = target;
            this.condition = condition;
            this.awareness = awareness;
        }
        protected override status DoUpdate() {
            if (target.val == null)
                return status.neutral;
            if (awareness && !condition.conditionMet) {
                awareness.ReactToPerson(target.val);
                condition.conditionMet = true;
                awareness.socializationTimer = -30;
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
}