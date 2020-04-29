using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutinePunchAt : Routine {
        Ref<GameObject> target;
        float timer;
        Personality.CombatProfficiency proficiency;
        public RoutinePunchAt(GameObject g, Controller c, Ref<GameObject> t, Personality.CombatProfficiency proficiency) : base(g, c) {
            routineThought = "Fisticuffs!";
            target = t;
            this.proficiency = proficiency;
        }
        protected override status DoUpdate() {
            timer -= Time.deltaTime;
            if (target.val != null) {
                control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f));
                if (timer < 0) {
                    if (proficiency == Personality.CombatProfficiency.normal) {
                        timer = 0.75f + UnityEngine.Random.Range(0, 0.25f);
                    } else if (proficiency == Personality.CombatProfficiency.expert) {
                        timer = 0.5f + UnityEngine.Random.Range(0, 0.2f);
                    } else if (proficiency == Personality.CombatProfficiency.poor) {
                        timer = 0.9f + UnityEngine.Random.Range(0, 0.25f);
                    }
                    control.ShootPressed();
                } else {
                    control.ResetInput();
                }
                return status.neutral;
            } else {
                return status.failure;
            }
        }
    }

}