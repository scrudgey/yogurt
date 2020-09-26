using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {

    public class GoalInflateBalloons : Goal {
        float utteranceTimer = UnityEngine.Random.Range(10f, 20f);
        List<string> phrases;
        float lowTimeRange;
        float highTimeRange;
        public GoalInflateBalloons(GameObject g, Controller c, string phrasePath, float low = 10f, float high = 20f) : base(g, c) {
            goalThought = "I'm inflating balloons.";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutineWanderAndPressF(g, c));
            LoadPhrases(phrasePath);
            this.lowTimeRange = low;
            this.highTimeRange = high;
            utteranceTimer = UnityEngine.Random.Range(low, high);
        }
        public void LoadPhrases(string path) {
            phrases = new List<string>();
            TextAsset textData = Resources.Load(path) as TextAsset;
            foreach (string line in textData.text.Split('\n')) {
                phrases.Add(line);
            }
        }
        public override void Update() {
            base.Update();
            utteranceTimer -= Time.deltaTime;
            if (utteranceTimer <= 0f) {
                EventData ed = new EventData(positive: 1);
                utteranceTimer = UnityEngine.Random.Range(lowTimeRange, highTimeRange);
                string phrase = phrases[UnityEngine.Random.Range(0, phrases.Count)];
                MessageSpeech message = new MessageSpeech(phrase, data: ed);
                Toolbox.Instance.SendMessage(gameObject, gameObject.transform, message);
            }
        }
    }
}
