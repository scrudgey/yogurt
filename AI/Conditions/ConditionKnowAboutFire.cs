using UnityEngine;

namespace AI {
    public class ConditionKnowAboutFire : Condition {
        Awareness awareness;
        public ConditionKnowAboutFire(GameObject g) : base(g) {
            awareness = g.GetComponent<Awareness>();
        }
        public override status Evaluate() {
            if (awareness) {
                if (awareness.nearestFire.val != null) {
                    return status.success;
                } else {
                    return status.failure;
                }
            } else {
                return status.failure;
            }
        }
    }
}