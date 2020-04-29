using UnityEngine;

namespace AI {
    public class ConditionFail : Condition {
        public ConditionFail(GameObject g) : base(g) { }
        public override status Evaluate() {
            return status.failure;
        }
    }
}