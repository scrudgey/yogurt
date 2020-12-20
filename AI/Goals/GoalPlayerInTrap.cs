using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalPlayerInTrap : Goal {
        public GoalPlayerInTrap(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            goalThought = "I'm looking for something.";
            routines.Add(new Routine(g, c));
            TrapDoor trapdoor = GameObject.Find("trapdoor").GetComponent<TrapDoor>();
            successCondition = new ConditionPlayerInTrap(g, target, trapdoor);
        }
    }
}
