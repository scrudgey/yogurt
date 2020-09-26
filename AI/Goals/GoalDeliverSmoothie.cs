using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {

    public class GoalDeliverSmoothie : Goal {
        GoalBlendSmoothie smoothieGoal;
        Ref<GameObject> customer;
        public GoalDeliverSmoothie(GameObject g, Controller c, Ref<GameObject> customer, GoalBlendSmoothie smoothieGoal) : base(g, c) {
            // this.target = target;
            // successCondition = new ConditionBoolSwitch(g);
            // boolSwitch = new ConditionBoolSwitch(g);
            // successCondition = boolSwitch;
            // RoutineSpeechWithPerson talkRoutine = new RoutineSpeechWithPerson(g, c, target, (ConditionBoolSwitch)successCondition);
            // routines.Add(talkRoutine);
            this.customer = customer;
            this.smoothieGoal = smoothieGoal;
            routines.Add(new RoutineTransferItem(g, c, customer, smoothieGoal));
            successCondition = new ConditionFail(g);
        }

    }
}
