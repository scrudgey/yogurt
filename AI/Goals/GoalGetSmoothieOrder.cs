using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalGetSmoothieOrder : Goal {
        public bool findingFail;
        public float utteranceTimer;
        public Ref<int> smoothieOrder;
        public static List<string> callOuts = new List<string>{
            "Smoothie! Getcha smoothie heah!",
            "I scream, you scream! Let's have a smoothie!",
            "Fuck! Oh shit! Shit ass smoothie dick cock motherfucker!!!",
            ""
        };
        public GoalGetSmoothieOrder(GameObject g, Controller c, Ref<int> smoothieOrder) : base(g, c) {
            this.smoothieOrder = smoothieOrder;
            ConditionLambda smoothieCondition = new ConditionLambda(g, () => {
                return this.smoothieOrder.val != -1;
            });
            successCondition = smoothieCondition;
        }
        public override void Update() {
            base.Update();
            utteranceTimer -= Time.deltaTime;
            if (utteranceTimer <= 0f) {
                EventData ed = new EventData(positive: 1);
                utteranceTimer = UnityEngine.Random.Range(10f, 20f);
                string phrase = callOuts[UnityEngine.Random.Range(0, callOuts.Count)];
                MessageSpeech message = new MessageSpeech(phrase, data: ed);
                Toolbox.Instance.SendMessage(gameObject, gameObject.transform, message);
            }
        }
    }
}
