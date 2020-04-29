using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityReadScript : Priority {
        string nextLine;
        public PriorityReadScript(GameObject g, Controller c) : base(g, c) {
            priorityName = "readscript";
            VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
            Goal goalWalkTo = new GoalWalkToPoint(g, c, new Ref<Vector2>(new Vector2(0.186f, 0.812f)));
            if (video != null) {
                Goal lookGoal = new GoalLookAtObject(g, c, new Ref<GameObject>(video.gameObject));
                lookGoal.requirements.Add(goalWalkTo);
                goal = lookGoal;
            }
        }
        public override float Urgency(Personality personality) {
            if (personality.camPref == Personality.CameraPreference.actor || personality.camPref == Personality.CameraPreference.gravy) {
                if (nextLine != null) {
                    return Priority.urgencyPressing;
                } else {
                    return Priority.urgencyMinor;
                }
            } else {
                return -1;
            }
        }
        public override void DoAct() {
            if (goal != null) {
                goal.Update();
            }
            if (nextLine != null) {
                Toolbox.Instance.SendMessage(gameObject, control.controllable, new MessageSpeech(nextLine));
                nextLine = null;
            }
        }
    }
}