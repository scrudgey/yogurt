using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalWalkToObject : Goal {
        public Ref<GameObject> target;
        public new string goalThought {
            get { return "I'm going to check out that " + target.val.name + "."; }
        }
        public GoalWalkToObject(GameObject g, Controller c, Ref<GameObject> t, float range = 0.2f, bool invert = false, Vector2 localOffset = new Vector2()) : base(g, c) {
            target = t;
            // TODO: if invert, change success condition
            successCondition = new ConditionCloseToObject(g, target, range, localOffset: localOffset);
            routines.Add(new RoutineWalkToGameobject(g, c, target, invert: invert, localOffset: localOffset));
        }
        public GoalWalkToObject(GameObject g, Controller c, Type objType, float range = 0.2f) : base(g, c) {
            // GameObject targetObject = GameObject.FindObjectOfType<typeof(objType)>();
            UnityEngine.Object obj = GameObject.FindObjectOfType(objType);
            Component targetComponent = (Component)obj;
            successCondition = new ConditionFail(g);
            if (targetComponent != null) {
                GameObject targetObject = targetComponent.gameObject;
                target = new Ref<GameObject>(targetObject);
                routines.Add(new RoutineWalkToGameobject(g, c, target));
                successCondition = new ConditionCloseToObject(g, target, range);
            }
        }
    }
}
