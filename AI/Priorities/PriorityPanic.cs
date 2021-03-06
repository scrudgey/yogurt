using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityPanic : Priority {
        public PriorityPanic(GameObject g, Controller c) : base(g, c) {
            priorityName = "panic";
            goal = new GoalPanic(g, c);

            // c.lostControlDelegate += LeaveControl;
            // c.gainedControlDelegate += AssumeControl;
        }
        public override void Update() {
            if (urgency > 0) {
                urgency -= Time.deltaTime / 2f;
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
                foreach (EventData data in message.data.describable.GetChildren()) {
                    urgency += data.quality[Rating.disturbing] / 2f;
                }
            }
            if (incoming is MessageDamage) {
                MessageDamage message = (MessageDamage)incoming;
                // if (!message.impersonal)
                urgency -= Priority.urgencyMinor;
            }
        }

        public override void EnterPriority() {
            base.EnterPriority();
            AssumeControl();
        }
        public override void ExitPriority() {
            base.ExitPriority();
            LeaveControl();
        }
        public void LeaveControl() {
            MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.panic, false);
            Toolbox.Instance.SendMessage(gameObject, GameManager.Instance, anim);
            // Debug.Log("sending stop panic notice");
        }
        public void AssumeControl() {
            MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.panic, true);
            Toolbox.Instance.SendMessage(gameObject, GameManager.Instance, anim);
            // Debug.Log("sending panic notice");
        }
    }
}