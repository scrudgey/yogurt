using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {


    public class GoalPanic : Goal {
        public GoalPanic(GameObject g, Controller c) : base(g, c) {
            goalThought = "Panic!";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutinePanic(g, c));
        }
    }
}
