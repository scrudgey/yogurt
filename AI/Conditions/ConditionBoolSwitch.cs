using UnityEngine;

namespace AI {
    public class ConditionBoolSwitch : Condition {
        public bool conditionMet;
        public ConditionBoolSwitch(GameObject g) : base(g) { }
        public override status Evaluate() {
            if (conditionMet) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
}