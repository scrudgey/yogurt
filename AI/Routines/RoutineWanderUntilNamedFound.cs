using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineWanderUntilNamedFound : Routine {
        // TODO: routine's update mimics goal's requirements structure. replace this with a goal
        private string target;
        private Awareness awareness;
        private float checkInterval;
        private RoutineWander wander;
        private RoutineGetNamedFromEnvironment getIt;
        private int mode;
        public RoutineWanderUntilNamedFound(GameObject g, Controller c, string t) : base(g, c) {
            target = t;
            routineThought = "I'm looking around for a " + t + ".";
            mode = 0;
            awareness = gameObject.GetComponent<Awareness>();
            wander = new RoutineWander(gameObject, control);
        }
        protected override status DoUpdate() {
            if (mode == 0) {   // wander part
                if (checkInterval > 1.5f) {
                    checkInterval = 0f;
                    List<GameObject> objs = awareness.FindObjectWithName(target);
                    if (objs.Count > 0) {
                        mode = 1;
                        getIt = new RoutineGetNamedFromEnvironment(gameObject, control, target);
                    }
                }
                checkInterval += Time.deltaTime;
                return wander.Update();
            } else {
                return getIt.Update();
            }
        }
    }
}