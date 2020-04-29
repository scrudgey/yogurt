using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalTalkToPerson : Goal {
        public Ref<GameObject> target;
        public GoalTalkToPerson(GameObject g, Controller c, Awareness awareness, Ref<GameObject> target) : base(g, c) {
            this.target = target;
            // this.target = awareness.socializationTarget;
            successCondition = new ConditionBoolSwitch(g);
            RoutineTalkToPerson talkRoutine = new RoutineTalkToPerson(g, c, target, (ConditionBoolSwitch)successCondition, awareness);
            routines.Add(talkRoutine);
        }
    }
}
