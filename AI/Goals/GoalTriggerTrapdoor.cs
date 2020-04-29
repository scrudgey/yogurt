using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalTriggerTrapdoor : Goal {
        public ConditionBoolSwitch successSwitch;
        public GoalTriggerTrapdoor(GameObject g, Controller c) : base(g, c) {
            goalThought = "I'm sprining my trap.";
            Speech speech = g.GetComponent<Speech>();

            successSwitch = new ConditionBoolSwitch(g);
            routines.Add(new RoutineTrapdoor(g, c, speech, successSwitch));

            // routines.Add(new Routine)

            // successSwitch.conditionMet = true;
            successCondition = successSwitch;
        }
    }
}
