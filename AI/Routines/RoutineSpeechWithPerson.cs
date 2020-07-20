using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineSpeechWithPerson : Routine {
        public Ref<GameObject> target;
        private ConditionBoolSwitch condition;
        private Speech speech;
        public RoutineSpeechWithPerson(GameObject g, Controller c, Ref<GameObject> target, ConditionBoolSwitch condition) : base(g, c) {
            this.target = target;
            this.condition = condition;
            this.speech = g.GetComponent<Speech>();
        }
        protected override status DoUpdate() {
            if (target.val == null)
                return status.neutral;
            if (!condition.conditionMet) {
                condition.conditionMet = true;
                speech.SpeakWith();
                Inventory myInv = gameObject.GetComponent<Inventory>();
                Inventory theirInv = target.val.GetComponent<Inventory>();
                if (myInv.holding != null && theirInv != null) {
                    theirInv.GetItem(myInv.holding);
                } else if (myInv != null) {
                    myInv.DropItem();
                }
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
}