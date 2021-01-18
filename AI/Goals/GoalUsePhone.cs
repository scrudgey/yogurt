using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalUsePhone : Goal {
        public bool phoneCalled;
        private RoutineUseTelephone telRoutine;
        public bool findingFail;
        public GoalUsePhone(GameObject g, Controller c) : base(g, c) {
            successCondition = new ConditionBoolSwitch(g);
            Telephone phoneObject = GameObject.FindObjectOfType<Telephone>();
            if (phoneObject) {
                Ref<GameObject> phoneRef = new Ref<GameObject>(phoneObject.gameObject);
                telRoutine = new RoutineUseTelephone(g, c, phoneRef, (ConditionBoolSwitch)successCondition);
                routines.Add(telRoutine);
            } else {
                findingFail = true;
            }
        }
        public override void Update() {
            base.Update();
            if (successCondition.Evaluate() == status.success && !phoneCalled) {
                phoneCalled = true;
            }
        }
    }
}
