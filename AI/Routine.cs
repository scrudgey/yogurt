using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public enum status { neutral, success, failure }

    public class Routine {
        public string routineThought = "I have no idea what I'm doing!";
        protected GameObject gameObject;
        protected Controller control;
        public float timeLimit = -1;
        protected float runTime = 0;
        Transform cachedTransform;
        public Transform transform {
            get {
                if (cachedTransform == null) {
                    cachedTransform = gameObject.GetComponent<Transform>();
                }
                return cachedTransform;
            }
        }
        public Routine(GameObject g, Controller c) {
            gameObject = g;
            control = c;
        }
        protected virtual status DoUpdate() {
            return status.neutral;
        }
        public virtual void Configure() {

        }
        // update is the routine called each frame by goal.
        // this bit first checks for a timeout, then calls a routine
        // that is specific to the child class.
        public status Update() {
            // Debug.Log("")
            runTime += Time.deltaTime;
            if (timeLimit > 0 && runTime > timeLimit) {
                runTime = 0;
                return status.failure;
            } else
                return DoUpdate();
        }
        public virtual void ExitPriority() { }
    }
}