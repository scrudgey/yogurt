using UnityEngine;

namespace AI {
    // this success condition is going to be pretty update intensive, getting a component on each frame??
    // find a better way to do this
    public class ConditionHoldingObjectOfType : Condition {
        string type;
        Inventory inv;
        public ConditionHoldingObjectOfType(GameObject g, string t) : base(g) {
            conditionThought = "I need a " + t;
            type = t;
            inv = gameObject.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (inv) {
                if (inv.holding) {
                    if (inv.holding.GetComponent(type)) {
                        return status.success;
                    } else {
                        return status.neutral;
                    }
                } else {
                    return status.neutral;
                }
            } else {
                return status.failure;
            }
        }
    }
}