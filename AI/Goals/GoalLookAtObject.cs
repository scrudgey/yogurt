using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalLookAtObject : Goal {
        public Ref<GameObject> target;
        public GoalLookAtObject(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            this.target = target;
            successCondition = new ConditionLookingAtObject(g, c, target);
            routines.Add(new RoutineLookAtObject(g, c, target));
        }
    }
}
