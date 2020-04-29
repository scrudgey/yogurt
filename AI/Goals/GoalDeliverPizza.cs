using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {

    public class GoalDeliverPizza : Goal {
        public Ref<GameObject> target;
        public ConditionBoolSwitch boolSwitch;
        public GoalDeliverPizza(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            this.target = target;
            successCondition = new ConditionBoolSwitch(g);
            boolSwitch = new ConditionBoolSwitch(g);
            successCondition = boolSwitch;
            RoutineSpeechWithPerson talkRoutine = new RoutineSpeechWithPerson(g, c, target, (ConditionBoolSwitch)successCondition);
            routines.Add(talkRoutine);
        }
    }
}
