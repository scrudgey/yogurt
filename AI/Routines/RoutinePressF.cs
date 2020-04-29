using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutinePressF : Routine {
        public ConditionBoolSwitch boolSwitch;
        public int count;
        public float interval;
        public float timer;
        public int numberTimesPressed;
        public RoutinePressF(GameObject g, Controller c, ConditionBoolSwitch boolSwitch, int count = 1, float interval = 1f) : base(g, c) {
            this.boolSwitch = boolSwitch;
            this.count = count;
            this.interval = interval;
            this.timer = interval;
        }
        protected override status DoUpdate() {
            timer += Time.deltaTime;
            if (timer > interval) {
                timer = 0;
                if (numberTimesPressed < count) {
                    numberTimesPressed += 1;
                    control.ShootPressed();
                    return status.neutral;
                } else {
                    boolSwitch.conditionMet = true;
                    return status.success;
                }
            }
            return status.neutral;
        }
    }
}