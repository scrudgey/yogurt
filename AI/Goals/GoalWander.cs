using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalWander : Goal {
        public GoalWander(GameObject g, Controller c) : base(g, c) {
            goalThought = "I'm doing nothing in particular.";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutineWander(g, c));
        }
    }
}
