using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;
using System.Linq;

using UnityEngine.SceneManagement;

public class CutsceneCredits : Cutscene {
    abstract class Module {
        protected enum State { none, dwellStart, translate, dwellEnd }
        protected enum CreditsState { none, fadeIn, fadeOut }
        public bool complete;
        protected State state;
        protected CreditsState creditsState;
        protected Camera camera;
        protected float timer;
        public float dwellTime = 2f;
        public float padTime = 2f;
        public float endTime = 6f;
        public Vector3 initialPosition;
        public Vector3 finalPosition;
        public string titleString;
        public string contentString;
        public float camSpeed = 0.005f;
        public Module(Camera camera) {
            this.camera = camera;
        }
        public virtual void Start() {
            this.initialPosition = GameObject.Find("endCutsceneInitial").transform.position;
            this.finalPosition = GameObject.Find("endCutsceneFinal").transform.position;

            camera.transform.position = initialPosition;
            state = State.dwellStart;
            UINew.Instance.RefreshUI(active: false);
        }
        public abstract void DisplayText();
        public abstract void RemoveText();
        public virtual void Update() {
            timer += Time.deltaTime;
            switch (state) {
                case State.dwellStart:
                    // fade in credits
                    if (timer > padTime && creditsState != CreditsState.fadeIn) {
                        creditsState = CreditsState.fadeIn;
                        DisplayText();
                    }
                    if (timer > dwellTime) {
                        timer = 0f;
                        state = State.translate;
                    }
                    // slew
                    break;
                case State.translate:
                    // translate to end position
                    // lerp
                    camera.transform.position = Vector3.MoveTowards(camera.transform.position, finalPosition, camSpeed);
                    if (Vector2.Distance(camera.transform.position, finalPosition) < 0.05f) {
                        timer = 0f;
                        state = State.dwellEnd;
                    }
                    break;
                default:
                case State.dwellEnd:
                    // slew
                    if (timer > dwellTime && creditsState != CreditsState.fadeOut) {
                        creditsState = CreditsState.fadeOut;
                        RemoveText();
                    }
                    // end
                    if (timer > endTime) {
                        End();
                    }
                    break;
            }
        }
        public virtual void End() {
            complete = true;
        }
    }

    class LeftCreditModule : Module {
        public LeftCreditModule(Camera camera) : base(camera) {
        }
        public override void DisplayText() {
            UINew.Instance.FadeInCredits(titleString, contentString, left: true);
        }
        public override void RemoveText() {
            UINew.Instance.FadeOutCredits(left: true);
        }
    }
    class RightCreditModule : Module {
        public RightCreditModule(Camera camera) : base(camera) {
        }
        public override void DisplayText() {
            UINew.Instance.FadeInCredits(titleString, contentString, left: false);
        }
        public override void RemoveText() {
            UINew.Instance.FadeOutCredits(left: false);
        }
    }
    class MoreCreditModule : Module {
        public string leftMoreText;
        public string rightMoreText;
        public MoreCreditModule(Camera camera, string left, string right) : base(camera) {
            leftMoreText = left;
            rightMoreText = right;
        }
        public override void DisplayText() {
            UINew.Instance.FadeInCredits(titleString, contentString, left: true);
            UINew.Instance.FadeInMoreCredits(leftMoreText, rightMoreText);
        }
        public override void RemoveText() {
            UINew.Instance.FadeOutCredits(left: true);
            UINew.Instance.FadeOutMoreCredits();
        }
    }
    class NeighborhoodModule : LeftCreditModule {
        SpriteRenderer condoRenderer;
        List<Sprite> sprites;
        public NeighborhoodModule(Camera camera) : base(camera) {
            GameObject.Find("signage").SetActive(false);
            GameObject condos = GameObject.Find("condos");
            condoRenderer = condos.GetComponent<SpriteRenderer>();
            sprites = condos.GetComponent<AnimateFrames>().frames;

            condoRenderer.enabled = true;
        }
        public override void Update() {
            base.Update();
            if (state == State.dwellEnd) {
                // slew
                if (timer > dwellTime + 1.25) {
                    condoRenderer.sprite = sprites[1];
                }
            }

        }
    }

    public Camera camera;
    Module module;
    public override void Configure() {
        if (configured)
            return;
        configured = true;
        CameraControl camControl = GameObject.FindObjectOfType<CameraControl>();
        camera = camControl.GetComponent<Camera>();
        camControl.enabled = false;
        UINew.Instance.fader.fadeInTime = 1f;
        UINew.Instance.fader.fadeOutTime = 1f;

        // switch module based on scene
        switch (SceneManager.GetActiveScene().name) {
            default:
            case "neighborhood":
                module = new NeighborhoodModule(camera) {
                    titleString = "- Yogurt Commercial 3 -",
                    contentString = "",
                    camSpeed = 0.0085f
                };
                camera.orthographicSize = 0.85f;
                break;
            case "mountain":
                module = new RightCreditModule(camera) {
                    titleString = "- Developer -",
                    contentString = "Ryan Foltz"
                };
                camera.orthographicSize = 0.6f;
                break;
            case "forest":
                module = new RightCreditModule(camera) {
                    titleString = "- Music and Special Effects -",
                    contentString = "Nathan Wiswall"
                };
                // camera.orthographicSize = 0.6f;
                break;
            case "chamber":
                module = new LeftCreditModule(camera) {
                    titleString = "- Graphics, Design, World, Puzzles -",
                    contentString = "Ryan Foltz"
                };
                // camera.orthographicSize = 0.6f;
                break;
            case "lower_hell":
                module = new LeftCreditModule(camera) {
                    titleString = "- Playtesters -",
                    contentString = playtesters,
                };
                // camera.orthographicSize = 0.6f;
                break;
            case "moon_town":
                module = new MoreCreditModule(camera, soundEffectsCreditLeft, soundEffectsCreditRight) {
                    titleString = "Sound Effects",
                    contentString = "from freesound.org",
                    dwellTime = 5f,
                    endTime = 7f
                };
                camera.orthographicSize = 0.75f;
                Text studio = GameObject.Find("scene/ground/Canvas/Text").GetComponent<Text>();
                studio.text = "";
                break;
        }
        module.Start();
    }

    public override void Update() {
        module.Update();
        if (module.complete) {
            NextScene();
        }
    }
    void NextScene() {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName) {
            case "neighborhood":
                GameManager.Instance.LeaveScene("mountain", 1);
                break;
            case "mountain":
                GameManager.Instance.LeaveScene("forest", 1);
                break;
            case "forest":
                GameManager.Instance.LeaveScene("chamber", 1);
                break;
            case "chamber":
                GameManager.Instance.LeaveScene("lower_hell", 1);
                break;
            case "lower_hell":
                GameManager.Instance.LeaveScene("moon_town", 1);
                break;
            default:
            case "moon_town":
                EndCutscene();
                break;
        }
    }
    void EndCutscene() {
        // GameManager.Instance.data.creditSequence = false;
        GameManager.Instance.data.state = GameState.postCredits;
        UINew.Instance.fader.fadeInTime = 0.5f;
        UINew.Instance.fader.fadeOutTime = 0.5f;
        complete = true;
        // CleanUp();
        GameManager.Instance.LeaveScene("devils_throneroom", 100);
    }
    public override void EscapePressed() {
    }
    // public override void CleanUp() {
    // }
    static string soundEffectsCreditLeft = @"kevinkace - Crate Break

sarana - metalbarrelkick

mlteenie - Beer barrels (aluminium)

MC_Minnaar - Pusing old wooden crate

Srehpog - crate_smash

anational - box-dropping-with-stones

SoundsLikeJoe - CardboardBoxes

mickdow - Paper Flutter .wav

Juhani Junkala - Retro SFX

Donalfonso - Crispbread Knäckebrot

InspectorJ (www.jshaw.co.uk) -
Smashing, Wooden Fence
Impact, Ice, Small, A.wav by InspectorJ 
Jew's Harp, Single, A (H1).wav

aropson - generic impact 1.wav

GowlerMusic - Censor Beep";

    static string soundEffectsCreditRight = @"temawas - Impact Bell Doppler.wav

Mafon2 - Filling the termos with water

C_J - Hollow Impact.wav

Nox_Sound - Ambiance_River_Flow

aerror - stonehit1.wav

IESP - Toilet Flushing

AnthonyChan0 - Tire gauge pin ringing and clicks

sirTmoney - Slurping Noodles

 Kinoton - Numbers 0-9, German, Female Voice

dslrguide - Scraping Stone

ryusa - Mouseover.wav

cabled_mess - Ping_Minimal UI Sounds

suntemple - SFX UI Button Click

visualasylum - Office Phone Ring

InSintesi - Jet Engine 1.wav

Tribal Battle Chants/Shouts Copyright 2012 Iwan Gabovitch [http://qubodup.net]";

    static string playtesters = @"
Kenneth Osborne

Jeff DiMaria

James Dixon

Tim Winter

Dr. Bill Freeman

Bob Greel

Josiah Whitish
";
}