using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;
using System.Linq;

using UnityEngine.SceneManagement;

public class CutsceneFirstDeath : Cutscene {
    // public float camSpeed = 0.005f;
    class Module {
        protected Camera camera;
        public bool complete;
        private bool started;
        protected float timer;
        public Module(Camera camera) {
            this.camera = camera;
        }
        public virtual void Start() {
            UINew.Instance.RefreshUI(active: false);
        }
        public virtual void Update() {
            if (!started) {
                started = true;
                Start();
            }
            timer += Time.deltaTime;
        }
        public virtual void End() {
            complete = true;
        }
    }

    class DialogueModule : Module {
        protected enum State { initial, ending }
        protected State state;
        public Vector3 position;
        public Speech hench;
        public float initialDuration;
        public float endDuration;
        public float camSpeed = 0.05f;
        public string dialogue;
        public Vector3 scale = Vector3.one;
        public DialogueModule(Camera camera, string dialogue) : base(camera) { this.dialogue = dialogue; }
        public override void Start() {
            base.Start();
            Vector3 camPosition = camera.transform.position;
            camPosition.z = -1;
            camera.transform.position = camPosition;
            camera.orthographicSize = 0.6f;

            state = State.initial;
            // spawn skeleton?
            GameObject henchObject = GameObject.Instantiate(Resources.Load("hench"), position, Quaternion.identity) as GameObject;
            Vector3 henchPosition = henchObject.transform.position;
            henchPosition.z = 0;
            henchObject.transform.position = henchPosition;
            henchObject.transform.localScale = scale;
            hench = henchObject.GetComponent<Speech>();
            hench.defaultMonologue = dialogue;
        }
        public override void Update() {
            base.Update();
            camera.transform.position = Vector3.Lerp(camera.transform.position, position, camSpeed);
            switch (state) {
                default:
                case State.initial:
                    if (timer > initialDuration) {
                        // Debug.Log("open dialogue menu");
                        // open dialogue menu
                        state = State.ending;
                        timer = 0f;
                        DialogueMenu menu = hench.SpeakWith();
                    }
                    break;
                case State.ending:
                    if (timer > endDuration) {
                        End();
                    }
                    break;
            }
        }
        public override void End() {
            base.End();
            GameObject.Destroy(hench.gameObject);
        }
    }


    Camera camera;
    CameraControl camControl;
    Stack<Module> modules = new Stack<Module>();
    bool fadeIn;
    bool white;
    float timer;
    float whiteoutTime = 3f;
    public override void Configure() {
        if (configured)
            return;
        UINew.Instance.ClearFaderModules();

        UINew.Instance.fader.fadeInTime = 4f;
        UINew.Instance.fader.fadeOutTime = 4f;

        configured = true;
        camControl = GameObject.FindObjectOfType<CameraControl>();
        camera = camControl.GetComponent<Camera>();
        camControl.enabled = false;

        modules.Push(new DialogueModule(camera, "deathCutscene4") {
            position = GameObject.Find("deathCutscene1").transform.position,
            initialDuration = 1f,
            endDuration = 1f,
        });

        modules.Push(new DialogueModule(camera, "deathCutscene3") {
            position = GameObject.Find("deathCutscene3").transform.position,
            initialDuration = 1f,
            endDuration = 1f,
        });

        modules.Push(new DialogueModule(camera, "deathCutscene2") {
            position = GameObject.Find("deathCutscene2").transform.position,
            initialDuration = 1f,
            endDuration = 1f,
            scale = new Vector3(-1, 1, 1)
        });

        modules.Push(new DialogueModule(camera, "deathCutscene1") {
            position = GameObject.Find("deathCutscene1").transform.position,
            initialDuration = 3f,
            endDuration = 2f,
            scale = new Vector3(1, 1, 1)
        });
        fadeIn = true;
        white = true;
        Module module = modules.Pop();
        module.Update();
        modules.Push(module);
        UINew.Instance.WhiteOut();
    }

    public override void Update() {
        if (white) {
            timer += Time.unscaledDeltaTime;
            if (timer > whiteoutTime) {
                white = false;
                UINew.Instance.WhiteIn(() => {
                    fadeIn = false;
                });
            }
            return;
        }
        if (fadeIn)
            return;
        if (modules.Count > 0) {
            Module module = modules.Pop();
            module.Update();
            if (!module.complete)
                modules.Push(module);
        } else {
            EndCutscene();
        }
    }
    void EndCutscene() {
        UINew.Instance.fader.fadeInTime = 0.5f;
        UINew.Instance.fader.fadeOutTime = 0.5f;
        complete = true;
        camControl.enabled = true;
    }
    public override void EscapePressed() {

    }
}