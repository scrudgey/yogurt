using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineLookForTarget : Routine {
        private Awareness awareness;
        private float checkInterval = 2f;
        private RoutineWander wander;
        private float timer;
        public GameObject target;
        protected static List<Vector3> directions = new List<Vector3>{
                Vector3.left, Vector3.right, Vector3.up, Vector3.down
        };
        private Stack<Vector3> directionBag = new Stack<Vector3>();
        public RoutineLookForTarget(GameObject g, Controller c) : base(g, c) {
            routineThought = "Where's that idiot?";
            awareness = gameObject.GetComponent<Awareness>();
        }
        protected override status DoUpdate() {
            timer += Time.deltaTime;
            if (timer > checkInterval) {
                timer = 0;
                ChangeDirection();
            }
            foreach (GameObject obj in awareness.fieldOfView) {
                if (obj.GetComponent<Controllable>() != null) {
                    PersonalAssessment pa = awareness.FormPersonalAssessment(obj);
                    if (pa != null && pa.status == PersonalAssessment.friendStatus.friend)
                        continue;
                    target = obj;
                    return status.success;
                }
            }
            return status.neutral;
        }
        public void ChangeDirection() {
            if (directionBag.Count == 0) {
                Vector3[] dirArray = directions.ToArray();
                Toolbox.ShuffleArray<Vector3>(dirArray);
                foreach (Vector3 d in dirArray) {
                    directionBag.Push(d);
                }
            }
            Vector3 direction = directionBag.Pop();
            control.SetDirection(direction);
        }
    }
}