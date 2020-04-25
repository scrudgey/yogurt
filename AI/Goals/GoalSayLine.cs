using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalSayLine : Goal {
        public ConditionBoolSwitch boolSwitch;
        public GoalSayLine(GameObject g, Controller c, MessageSpeech message) : base(g, c) {
            boolSwitch = new ConditionBoolSwitch(g);
            RoutineSayLine routine = new RoutineSayLine(g, c, message, boolSwitch);
            successCondition = boolSwitch;
            routines.Add(routine);
        }
    }
}
