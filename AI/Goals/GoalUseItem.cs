using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalUseItem : Goal {
        public Inventory inventory;
        public ConditionBoolSwitch boolSwitch;
        public GoalUseItem(GameObject g, Controller c) : base(g, c) {
            inventory = g.GetComponent<Inventory>();
            boolSwitch = new ConditionBoolSwitch(g);
            successCondition = boolSwitch;
            RoutinePressF routinePressF = new RoutinePressF(g, c, boolSwitch, count: 2, interval: 5f);
            routines.Add(routinePressF);
        }
    }
}
