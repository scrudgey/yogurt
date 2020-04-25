using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {

    public class GoalInflateBalloons : Goal {
        float utteranceTimer = UnityEngine.Random.Range(10f, 20f);
        List<string> phrases;
        public GoalInflateBalloons(GameObject g, Controller c) : base(g, c) {
            goalThought = "I'm inflating balloons.";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutineWanderAndPressF(g, c));
            LoadPhrases();
        }
        public void LoadPhrases() {
            phrases = new List<string>();
            TextAsset textData = Resources.Load("data/dialogue/clown_phrases") as TextAsset;
            foreach (string line in textData.text.Split('\n')) {
                phrases.Add(line);
            }
        }
        public override void Update() {
            base.Update();
            utteranceTimer -= Time.deltaTime;
            if (utteranceTimer <= 0f) {
                EventData ed = new EventData(positive: 1);
                utteranceTimer = UnityEngine.Random.Range(10f, 20f);
                string phrase = phrases[UnityEngine.Random.Range(0, phrases.Count)];
                MessageSpeech message = new MessageSpeech(phrase, data: ed);
                Toolbox.Instance.SendMessage(gameObject, gameObject.transform, message);
            }
        }
    }
}
