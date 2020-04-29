using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {

    public class GoalDance : Goal {
        public GoalDance(GameObject g, Controller c, Personality p) : base(g, c) {
            goalThought = "Dance!";
            successCondition = new ConditionFail(g);

            routines.Add(new RoutineDance(g, c, p));
        }
    }
}
