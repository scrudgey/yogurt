using UnityEngine;

namespace AI {
    public class ConditionInFightMode : Condition {
        Controller control;
        Inventory inv;
        public ConditionInFightMode(GameObject g, Controller c) : base(g) {
            control = c;
            inv = g.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (inv != null) {
                if (inv.holding) {
                    MeleeWeapon weapon = inv.holding.GetComponent<MeleeWeapon>();
                    if (weapon) {
                        return status.success;
                    }
                }
            }
            if (control.controllable.fightMode) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
}