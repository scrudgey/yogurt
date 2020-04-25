using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalGetItem : Goal {
        public bool findingFail;
        public GoalGetItem(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            // TODO: fill this in
            successCondition = new ConditionHoldingSpecificObject(g, target);
            routines.Add(new RoutineRetrieveRefFromInv(g, c, target));
            routines.Add(new RoutineGetRefFromEnvironment(g, c, target));
        }
        public GoalGetItem(GameObject g, Controller c, string target) : base(g, c) {
            goalThought = "I need a " + target + ".";
            successCondition = new ConditionHoldingObjectWithName(g, target);
            routines.Add(new RoutineRetrieveNamedFromInv(g, c, target));
            routines.Add(new RoutineGetNamedFromEnvironment(g, c, target));
            routines.Add(new RoutineWanderUntilNamedFound(g, c, target));
        }
        public GoalGetItem(GameObject g, Controller c, GameObject target) : base(g, c) {
            goalThought = "I need a " + target + ".";
            successCondition = new ConditionHoldingObjectWithName(g, target.name);
            routines.Add(new RoutineRetrieveNamedFromInv(g, c, target.name));
            routines.Add(new RoutineGetNamedFromEnvironment(g, c, target.name));
            routines.Add(new RoutineWanderUntilNamedFound(g, c, target.name));
        }
        public override void Update() {
            base.Update();
            if (index == 2 && !findingFail) {
                findingFail = true;
            }
        }
    }
}
