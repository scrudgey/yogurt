using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalObserveObject : Goal {
        public Ref<GameObject> target;
        public GoalObserveObject(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            goalThought = "I'm looking for something.";
            this.target = target;
            routines.Add(new Routine(g, c));
            successCondition = new ConditionLookingAtObject(g, c, target);
        }
    }
}
