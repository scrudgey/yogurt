using UnityEngine;
using System.Collections.Generic;

namespace AI {
    public class Priority {
        public string priorityName = "priority";
        public const float urgencyMinor = 1f;
        public const float urgencySmall = 2.5f;
        public const float urgencyLarge = 5f;
        public const float urgencyPressing = 10f;
        public const float urgencyMaximum = 10f;
        public float minimumUrgency = 0;
        public float urgency;
        public Awareness awareness;
        public Controller control;
        public GameObject gameObject;
        public Goal goal;
        public Priority(GameObject g, Controller c) {
            InitReferences(g, c);
        }
        public void InitReferences(GameObject g, Controller c) {
            gameObject = g;
            control = c;
            awareness = g.GetComponent<Awareness>();
        }
        public virtual void Update() { }
        public virtual void DoAct() {
            if (goal != null) {
                goal.Update();
            }
        }
        public virtual float Urgency(Personality personality) {
            return urgency;
        }
        public virtual void ReceiveMessage(Message m) { }
        // public virtual void ObserveOccurrence(OccurrenceData data){}
        public virtual void EnterPriority() { }
        public virtual void ExitPriority() { }

    }
}