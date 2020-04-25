using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineUseTelephone : Routine {
        public Ref<GameObject> telRef;
        private Telephone telephone;
        private ConditionBoolSwitch condition;
        public RoutineUseTelephone(GameObject g, Controller c, Ref<GameObject> telRef, ConditionBoolSwitch condition) : base(g, c) {
            this.telRef = telRef;
            this.condition = condition;
            telephone = telRef.val.GetComponent<Telephone>();
        }
        protected override status DoUpdate() {
            if (telephone && !condition.conditionMet) {
                telephone.FireButtonCallback();
                condition.conditionMet = true;
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
}