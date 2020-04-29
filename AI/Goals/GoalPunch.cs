using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {

    public class GoalPunch : Goal {
        public Ref<GameObject> target;
        public new string goalThought {
            get { return "I've got to do something about that " + target.val.name + "."; }
        }
        public GoalPunch(GameObject g, Controller c, Ref<GameObject> r, Personality.CombatProfficiency profficiency) : base(g, c) {
            Goal punchGoal = new Goal(gameObject, control);
            Routine routinePunch = new RoutinePunchAt(gameObject, control, r, profficiency);
            Routine wanderRoutine = new RoutineWander(g, c);
            switch (profficiency) {
                case Personality.CombatProfficiency.expert:
                    routinePunch.timeLimit = 4f;
                    wanderRoutine.timeLimit = 0.25f;
                    break;
                default:
                case Personality.CombatProfficiency.normal:
                    routinePunch.timeLimit = 2f;
                    wanderRoutine.timeLimit = 0.5f;
                    break;
                case Personality.CombatProfficiency.poor:
                    routinePunch.timeLimit = 1f;
                    wanderRoutine.timeLimit = 1.2f;
                    break;
            }

            routines.Add(routinePunch);
            routines.Add(wanderRoutine);
        }
    }
}
