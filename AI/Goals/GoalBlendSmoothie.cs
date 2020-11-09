using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalBlendSmoothie : Goal {
        Ref<GameObject> cauldron;
        // Blender blender;
        Inventory inventory;
        public RoutineUseBlender routine;
        public ConditionBoolSwitch condition;
        public Ref<int> smoothieOrder;
        public GoalBlendSmoothie(GameObject g, Controller c, Ref<GameObject> cauldron, Ref<int> smoothieOrder) : base(g, c) {
            this.cauldron = cauldron;
            this.smoothieOrder = smoothieOrder;

            inventory = g.GetComponent<Inventory>();
            condition = new ConditionBoolSwitch(g);
            successCondition = condition;

            if (cauldron.val != null) {
                Blender blender = cauldron.val.GetComponent<Blender>();
                routine = new RoutineUseBlender(g, c, blender, inventory, condition);
                routines.Add(routine);
            }
        }
        public void Reset() {
            routine.Reset();
            smoothieOrder.val = -1;
            // Debug.Log(smoothieOrder.val);
        }

    }
}
