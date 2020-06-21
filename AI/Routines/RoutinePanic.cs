using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutinePanic : Routine {
        public static List<string> panicPhrases = new List<string>() { "Yipe!", "Egad!", "Panic!", "Yikes!", "Yikers!" };
        private float wanderTime = 0;
        // private float switchTime = 0;
        private DirectionEnum dir;
        public RoutinePanic(GameObject g, Controller c) : base(g, c) {
            routineThought = "Panic!!!!";
        }
        public override void Configure() {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        }
        protected override status DoUpdate() {
            if (wanderTime >= 0) {
                wanderTime -= Time.deltaTime;
                switch (dir) {
                    case DirectionEnum.down:
                        control.downFlag = true;
                        break;
                    case DirectionEnum.left:
                        control.leftFlag = true;
                        break;
                    case DirectionEnum.right:
                        control.rightFlag = true;
                        break;
                    case DirectionEnum.up:
                        control.upFlag = true;
                        break;
                }
            } else {
                if (wanderTime < 0f) {
                    wanderTime = UnityEngine.Random.Range(0, 0.25f);
                    dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
                    control.ResetInput();
                    if (UnityEngine.Random.Range(0, 1f) < 0.1f) {
                        MessageSpeech message = new MessageSpeech(panicPhrases[UnityEngine.Random.Range(0, panicPhrases.Count)]);
                        Toolbox.Instance.SendMessage(gameObject, Toolbox.Instance, message);
                    }
                }
            }

            return status.neutral;
        }
    }
}