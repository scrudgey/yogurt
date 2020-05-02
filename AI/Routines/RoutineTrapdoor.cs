using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineTrapdoor : Routine {
        public Speech vampireSpeech;
        public ConditionBoolSwitch sw;
        public RoutineTrapdoor(GameObject g, Controller c, Speech vampireSpeech, ConditionBoolSwitch sw) : base(g, c) {
            this.vampireSpeech = vampireSpeech;
            this.sw = sw;
        }
        protected override status DoUpdate() {
            if (!control.Authenticate())
                return status.neutral;
            if (!sw.conditionMet) {
                vampireSpeech.SpeakWith();
                sw.conditionMet = true;
                return status.success;
            } else
                return status.neutral;
        }
    }

}