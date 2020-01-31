using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityReactToCamera : Priority {
        public ConditionBoolSwitch sayLineCondition;
        public WorldRef<VideoCamera> video = new WorldRef<VideoCamera>(null);
        public float videoCheckTimer;
        public Personality.CameraPreference camPref;
        public bool onCamera;
        public PriorityReactToCamera(GameObject g, Controllable c, Personality.CameraPreference camPref) : base(g, c) {
            priorityName = "reactToCamera";
            this.camPref = camPref;

            if (camPref == Personality.CameraPreference.actor) {
                MessageSpeech message = new MessageSpeech("Bob yogurt is so good, we bet a passer-by will really like it!");
                Goal goalWalkTo = new GoalWalkToObject(g, c, video.gameObject, localOffset: new Vector2(1f, 0f));
                Goal lookGoal = new GoalLookAtObject(g, c, video.gameObject);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                sayLineCondition = sayLine.boolSwitch;

                lookGoal.requirements.Add(goalWalkTo);
                sayLine.requirements.Add(lookGoal);
                goal = sayLine;
            } else if (camPref == Personality.CameraPreference.avoidant) {
                Goal goalWalkTo = new GoalWalkToObject(g, c, video.gameObject, invert: true);
                goal = goalWalkTo;
            } else if (camPref == Personality.CameraPreference.ambivalent) {
                MessageSpeech message = new MessageSpeech("Get that camera away from me!");
                Goal lookGoal = new GoalLookAtObject(g, c, video.gameObject);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                sayLine.requirements.Add(lookGoal);
                goal = sayLine;
            } else if (camPref == Personality.CameraPreference.excited) {
                MessageSpeech message = new MessageSpeech("Hi Mom!");
                Goal goalWalkTo = new GoalWalkToObject(g, c, video.gameObject, localOffset: new Vector2(1f, 0f));
                Goal lookGoal = new GoalLookAtObject(g, c, video.gameObject);
                GoalSayLine sayLine = new GoalSayLine(g, c, message);
                lookGoal.requirements.Add(goalWalkTo);
                sayLine.requirements.Add(lookGoal);
                goal = sayLine;
            }
        }
        public override float Urgency(Personality personality) {
            if (personality.camPref == Personality.CameraPreference.none) {
                return -1;
            }

            if (!GameManager.Instance.data.recordingCommercial)
                return -1;

            if (camPref == Personality.CameraPreference.actor) {
                if (sayLineCondition.conditionMet) {
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
        public override void Update() {
            base.Update();
            if (video.val == null) {
                videoCheckTimer += Time.deltaTime;
                if (videoCheckTimer > 1f) {
                    video.val = GameObject.FindObjectOfType<VideoCamera>();
                }
            }
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageOnCamera) {
                MessageOnCamera message = (MessageOnCamera)incoming;
                onCamera = message.value;
            }
        }
    }
}