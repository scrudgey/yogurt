using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityReactToCamera : Priority {
        public ConditionBoolSwitch boolSwitchCondition;
        public LambdaRef<GameObject> video = new LambdaRef<GameObject>(null, () => {
            VideoCamera v = GameObject.FindObjectOfType<VideoCamera>();
            if (v != null) {
                return v.gameObject;
            } else return null;
        });
        public LambdaRef<GameObject> yogurtRef = new LambdaRef<GameObject>(null, () => {
            foreach (LiquidContainer container in GameObject.FindObjectsOfType<LiquidContainer>()) {
                if (container.liquid != null && container.amount > 0 && container.liquid.name == "yogurt") {
                    return container.gameObject;
                }
            }
            return null;
        });
        public Personality.CameraPreference camPref;
        public bool onCamera;
        public PriorityReactToCamera(GameObject g, Controllable c, Personality.CameraPreference camPref) : base(g, c) {
            priorityName = "reactToCamera";
            this.camPref = camPref;

            // TODO: split this into subclasses.
            if (camPref == Personality.CameraPreference.actor) {
                MessageSpeech message = new MessageSpeech("Bob yogurt is so good, we bet a passer-by will really like it!");
                Goal goalWalkTo = new GoalWalkToObject(g, c, video, localOffset: new Vector2(1f, 0f));
                Goal lookGoal = new GoalLookAtObject(g, c, video);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                boolSwitchCondition = sayLine.boolSwitch;

                lookGoal.requirements.Add(goalWalkTo);
                sayLine.requirements.Add(lookGoal);
                goal = sayLine;
            } else if (camPref == Personality.CameraPreference.avoidant) {
                Goal goalWalkTo = new GoalWalkToObject(g, c, video, invert: true);
                goal = goalWalkTo;
            } else if (camPref == Personality.CameraPreference.ambivalent) {
                MessageSpeech message = new MessageSpeech("Get that camera away from me!");
                Goal lookGoal = new GoalLookAtObject(g, c, video);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                sayLine.requirements.Add(lookGoal);
                goal = sayLine;
            } else if (camPref == Personality.CameraPreference.excited) {
                MessageSpeech message = new MessageSpeech("Hi Mom!");
                Goal goalWalkTo = new GoalWalkToObject(g, c, video, localOffset: new Vector2(1f, 0f));
                Goal lookGoal = new GoalLookAtObject(g, c, video);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                lookGoal.requirements.Add(goalWalkTo);
                sayLine.requirements.Add(lookGoal);
                goal = sayLine;
            } else if (camPref == Personality.CameraPreference.eater) {
                MessageSpeech message = new MessageSpeech("I will gladly try Bob yogurt!");
                Goal goalWalkTo = new GoalWalkToObject(g, c, video, localOffset: new Vector2(1f, 0f));
                Goal lookGoal = new GoalLookAtObject(g, c, video);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                GoalGetItem getYogurt = new GoalGetItem(g, c, yogurtRef);
                GoalUseItem goalUseItem = new GoalUseItem(g, c);
                boolSwitchCondition = goalUseItem.boolSwitch;

                getYogurt.ignoreRequirementsIfConditionMet = true;
                sayLine.ignoreRequirementsIfConditionMet = true;

                lookGoal.requirements.Add(goalWalkTo);
                sayLine.requirements.Add(lookGoal);
                getYogurt.requirements.Add(sayLine);
                goalUseItem.requirements.Add(getYogurt);
                goal = goalUseItem;
            }
        }
        public override float Urgency(Personality personality) {
            if (personality.camPref == Personality.CameraPreference.none) {
                return -1;
            }

            if (GameManager.Instance.data != null && !GameManager.Instance.data.recordingCommercial)
                return -1;

            if (camPref == Personality.CameraPreference.actor || camPref == Personality.CameraPreference.eater) {
                if (boolSwitchCondition.conditionMet) {
                    return Priority.urgencyMinor;
                } else {
                    return Priority.urgencyPressing;
                }
            } else if (camPref == Personality.CameraPreference.avoidant) {
                return Priority.urgencySmall;
            } else if (camPref == Personality.CameraPreference.ambivalent || camPref == Personality.CameraPreference.excited) {
                if (onCamera)
                    return Priority.urgencyPressing;
                else
                    return -1;
            }
            return Priority.urgencyMinor;
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageOnCamera) {
                MessageOnCamera message = (MessageOnCamera)incoming;
                onCamera = message.value;
            }
        }
    }
}