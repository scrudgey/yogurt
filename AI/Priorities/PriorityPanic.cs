using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityPanic : Priority {
        public PriorityPanic(GameObject g, Controllable c) : base(g, c) {
            priorityName = "panic";
            goal = new GoalPanic(g, c);
        }
        public override void Update() {
            if (urgency > 0) {
                urgency -= Time.deltaTime;
            }
        }
        public override float Urgency(Personality personality) {
            if (awareness.imOnFire) {
                return 2 * Priority.urgencyMaximum;
            }
            if (personality.bravery == Personality.Bravery.brave) {
                return urgency / 100f;
            }
            // return urgency / 100f;
            if (personality.bravery == Personality.Bravery.cowardly)
                return urgency * 2f;
            return urgency;
        }
        public override void ReceiveMessage(Message incoming) {
            if (awareness.imOnFire)
                if (awareness.decisionMaker.personality.bravery == Personality.Bravery.cowardly) {
                    if (incoming is MessageDamage) {
                        urgency += Priority.urgencyMinor / 2f;
                    }
                    if (incoming is MessageThreaten) {
                        urgency += Priority.urgencyMinor / 2f;
                    }
                }
            if (incoming is MessageOccurrence) {
                MessageOccurrence message = (MessageOccurrence)incoming;
                foreach (EventData data in message.data.events) {
                    urgency += data.ratings[Rating.disturbing] / 2f;
                }
            }
        }
    }
}