using UnityEngine;

namespace AI {
    public class ConditionHoldingObjectWithName : Condition {
        string name;
        Inventory inv;
        public ConditionHoldingObjectWithName(GameObject g, string t) : base(g) {
            conditionThought = "I need a " + t;
            name = t;
            inv = gameObject.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (inv) {
                if (inv.holding) {
                    string holdingName = Toolbox.Instance.CloneRemover(inv.holding.name);
                    // Debug.Log($"{holdingName} <-> {name}");
                    if (holdingName.ToLower().Contains(name.ToLower())) {
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