using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class Goal {
        public List<Routine> routines = new List<Routine>();
        public int index = 0;
        public List<Goal> requirements = new List<Goal>();
        public Condition successCondition;
        public GameObject gameObject;
        public Controller control;
        public float slewTime;
        private bool fulfillingRequirements = true;
        public string goalThought = "I'm just doing my thing.";
        public bool ignoreRequirementsIfConditionMet;

        public Goal(GameObject g, Controller c) {
            gameObject = g;
            control = c;
            slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
        }
        public status Evaluate() {
            if (ignoreRequirementsIfConditionMet && successCondition.Evaluate() == status.success)
                return status.success;
            foreach (Goal requirement in requirements) {
                if (requirement.Evaluate() != status.success) {
                    return status.failure;
                }
            }
            return successCondition.Evaluate();
        }
        public virtual void Update() {

            // if i have any unmet requirements, my update goes to the first unmet one.
            if (!(ignoreRequirementsIfConditionMet && successCondition.Evaluate() == status.success)) {
                foreach (Goal requirement in requirements) {
                    if (requirement.Evaluate() != status.success) {
                        fulfillingRequirements = true;
                        requirement.Update();
                        return;
                    }
                }
            }

            if (fulfillingRequirements) {
                // Debug.Log(control.gameObject.name + " " + this.ToString() + " requirements met"); ;
                control.ResetInput();
            }
            fulfillingRequirements = false;
            if (slewTime > 0) {
                slewTime -= Time.deltaTime;
                return;
            }
            if (routines.Count > 0) {
                try {
                    status routineStatus = routines[index].Update();
                    // Debug.Log(routines[index].GetType().ToString());
                    // Debug.Log(routineStatus);
                    if (routineStatus == status.failure) {
                        control.ResetInput();
                        index++;
                        // get next routine, or fail.
                        if (index < routines.Count) {
                            slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
                        } else {
                            // what do do? reset from the start maybe
                            // index = routines.Count - 1;
                            index = 0;
                        }
                        routines[index].Configure();
                    }
                }
                catch (Exception e) {
                    Debug.LogError(this.ToString() + " fail: " + e.Message);
                    Debug.LogErrorFormat(e.StackTrace);
                    // Debug.Log(e.TargetSite);
                }
            }
        }
    }
}
