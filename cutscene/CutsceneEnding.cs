using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;
using System.Linq;

public class CutsceneEnding : Cutscene {
    public abstract class Module {
        public GameObject gameObject;
        public bool complete;
        public bool fade;
        abstract public void Update();
        abstract public void Start();
        abstract public void End();
        abstract public void CleanUp();
    }
    class SpeechModule : Module {
        protected Speech moeSpeech;
        protected Speech curlySpeech;
        protected Speech larrySpeech;
        protected Speech satanSpeech;
        public float scriptTimeSpace = 1f;

        Regex ampersandHook = new Regex(@"\&\r?$");
        Regex numberHook = new Regex(@"^([\d.]+)");
        Regex lineHook = new Regex(@"^(.*):(.+)");
        Regex endHook = new Regex(@"END");
        static private Regex name_hook = new Regex(@"\$name");

        // private float startDialogue = 4.5f;
        float timer;
        int index = 0;
        List<string> lines = new List<string>();

        bool fadeIn = false;

        public SpeechModule(GameObject gameObject, string script, Speech moeSpeech, Speech curlySpeech, Speech larrySpeech, Speech satanSpeech) {
            LoadScript(script);
            this.gameObject = gameObject;
            this.moeSpeech = moeSpeech;
            this.curlySpeech = curlySpeech;
            this.larrySpeech = larrySpeech;
            this.satanSpeech = satanSpeech;
        }
        public override void Start() {
            gameObject.SetActive(true);
            fadeIn = true;
            UINew.Instance.FadeIn(() => {
                fadeIn = false;
            });
        }
        public override void End() {
            gameObject.SetActive(false);
        }
        public override void CleanUp() {

        }
        public override void Update() {
            if (fadeIn)
                return;
            timer += Time.deltaTime;
            if (Speaking()) {
                timer = 0;
                return;
            }
            // moeControl.SetDirection(Vector2.down);
            if (timer > this.scriptTimeSpace) {
                timer = 0;
                ProcessLine();
            }
        }
        public void MoeSpeak(MessageSpeech message) { moeSpeech.HandleSpeech(message); }
        public void CurlySpeak(MessageSpeech message) { curlySpeech.HandleSpeech(message); }
        public void LarrySpeak(MessageSpeech message) { larrySpeech.HandleSpeech(message); }
        public void SatanSpeak(MessageSpeech message) { satanSpeech.HandleSpeech(message); }
        public bool Speaking() {
            return new List<Speech> { moeSpeech, larrySpeech, curlySpeech, satanSpeech }
                .Where(x => x != null)
                .Select(speech => speech.speaking)
                .Contains(true);
        }
        MessageSpeech LineToSpeechMessage(string line) {
            Match match = lineHook.Match(line);

            string phrase = match.Groups[2].Value;
            phrase = name_hook.Replace(phrase, GameManager.Instance.saveGameName);

            return new MessageSpeech(phrase);
        }
        void ProcessLine() {

            bool amp = false;
            string line = lines[index];
            if (ampersandHook.IsMatch(line)) {
                amp = true;
                line = line.Substring(0, line.Length - 1);
            }
            if (lineHook.IsMatch(line)) {
                Match match = lineHook.Match(line);
                MessageSpeech message = LineToSpeechMessage(line);
                if (match.Groups[1].Value == "MOE") {
                    MoeSpeak(message);
                } else if (match.Groups[1].Value == "LARRY") {
                    LarrySpeak(message);
                } else if (match.Groups[1].Value == "CURLY") {
                    CurlySpeak(message);
                } else if (match.Groups[1].Value == "SATAN") {
                    SatanSpeak(message);
                }
            } else {
                // if (line == "<PHONEEND>") {
                //     // soundEffect.GetComponent<AudioSource>().Play();
                //     // set Moe sprite
                //     moeSprite.sprite = phoneDown;
                // }
            }
            if (endHook.IsMatch(line)) {
                complete = true;
            }
            if (index + 1 < lines.Count - 1) {
                if (numberHook.IsMatch(lines[index + 1])) {
                    Match match = numberHook.Match(lines[index + 1]);
                    scriptTimeSpace = float.Parse(match.Groups[1].Value);
                    index += 1;
                }
            }
            index += 1;
            if (amp)
                ProcessLine();
        }
        bool LoadScript(string filename) {
            TextAsset textData = Resources.Load("data/office/" + filename) as TextAsset;
            if (textData == null) {
                return false;
            } else {
                lines = new List<string>(textData.text.Split('\n'));
                return true;
            }
        }
    }

    class NewsModule : SpeechModule {
        Controller moeController;
        public NewsModule(GameObject gameObject, string script, Speech moeSpeech, Speech curlySpeech, Speech larrySpeech, Speech satanSpeech) : base(gameObject, script, moeSpeech, curlySpeech, larrySpeech, satanSpeech) {
            moeController = new Controller(moeSpeech.gameObject);
        }
        public override void CleanUp() {
            moeController.Dispose();
        }
        public override void Update() {
            base.Update();
            moeController.SetDirection(Vector2.down);
        }
    }
    class HQModule : SpeechModule {
        Controller moeController;
        Controller larryController;
        Controller curlyController;
        Controller satanController;
        public HQModule(GameObject gameObject, string script, Speech moeSpeech, Speech curlySpeech, Speech larrySpeech, Speech satanSpeech) : base(gameObject, script, moeSpeech, curlySpeech, larrySpeech, satanSpeech) {
            moeController = new Controller(moeSpeech.gameObject);
            larryController = new Controller(larrySpeech.gameObject);
            curlyController = new Controller(curlySpeech.gameObject);
            satanController = new Controller(satanSpeech.gameObject);
        }
        public override void CleanUp() {
            moeController.Dispose();
            larryController.Dispose();
            curlyController.Dispose();
            satanController.Dispose();
        }
        public override void Update() {
            base.Update();
            moeController.SetDirection(Vector2.right);
            larryController.SetDirection(Vector2.right);
            curlyController.SetDirection(Vector2.left);
            satanController.SetDirection(Vector2.down);
        }
    }

    class StartModule : Module {
        private float stopTextFade = 1.5f;
        private float startDialogue = 4.5f;
        Text settingText;
        GameObject canvas;
        float timer;
        public StartModule(GameObject longShot, GameObject canvas, Text settingText) {
            this.canvas = canvas;
            this.gameObject = longShot;
            this.settingText = settingText;
        }
        public override void Start() {
            canvas.SetActive(true);
            gameObject.SetActive(true);
        }
        public override void End() {
            canvas.SetActive(false);
            gameObject.SetActive(false);
        }
        public override void CleanUp() {

        }
        public override void Update() {
            timer += Time.deltaTime;
            // do fade in of text
            if (timer < stopTextFade) {
                Color col = settingText.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTextFade);
                settingText.color = col;
            }
            if (timer >= startDialogue) {
                this.complete = true;
                settingText.gameObject.SetActive(false);
            }
        }
    }


    enum State { start, viewing, street, news, hq }
    State state;
    public EndingCutsceneController controller;
    StartModule startModule;
    SpeechModule viewingModule;
    SpeechModule streetModule;
    SpeechModule newsModule;
    SpeechModule hqModule;
    Module getCurrentModule() {
        switch (state) {
            default:
            case State.start:
                return startModule;
            case State.viewing:
                return viewingModule;
            case State.street:
                return streetModule;
            case State.news:
                return newsModule;
            case State.hq:
                return hqModule;
        }
    }

    public override void Configure() {
        if (configured)
            return;
        configured = true;
        controller = GameObject.FindObjectOfType<EndingCutsceneController>();
        CameraControl camControl = GameObject.FindObjectOfType<CameraControl>();
        MusicController.Instance.EnqueueMusic(new Music(new Track(TrackName.ending, loop: false)));

        startModule = new StartModule(controller.objLongShot, controller.objCanvas, controller.settingText);
        viewingModule = new SpeechModule(controller.objViewingRoom, "endingViewing", controller.ViewingMoeSpeech, controller.ViewingCurlySpeech, controller.ViewingLarrySpeech, controller.ViewingSatanSpeech);
        streetModule = new SpeechModule(controller.objStreet, "endingStreet", controller.StreetMoeSpeech, controller.StreetCurlySpeech, null, controller.StreetSatanSpeech);
        newsModule = new NewsModule(controller.objNews, "endingNews", controller.NewsMoeSpeech, null, null, controller.ViewingSatanSpeech);
        hqModule = new HQModule(controller.objHQ, "endingHQ", controller.HQMoeSpeech, controller.HQCurlySpeech, controller.HQLarrySpeech, controller.HQCEOSpeech);
        state = State.start;
        getCurrentModule().Start();
    }

    public override void Update() {
        Module module = getCurrentModule();
        module.Update();
        if (module.complete && !module.fade) {
            module.fade = true;
            UINew.Instance.FadeOut(() => {
                doNextModule(module);
            });
        }
    }
    public void doNextModule(Module module) {
        module.End();
        NextModule();
    }
    void NextModule() {
        bool startNextmodule = true;
        // switch to next module
        switch (state) {
            case State.start:
                state = State.viewing;
                break;
            case State.viewing:
                state = State.street;
                break;
            case State.street:
                state = State.news;
                break;
            case State.news:
                state = State.hq;
                break;
            case State.hq:
            default:
                EndCutscene();
                startNextmodule = false;
                break;
        }
        if (startNextmodule) {
            getCurrentModule().Start();
        }
    }
    void EndCutscene() {
        complete = true;
        CleanUp();
        // GameManager.Instance.data.creditSequence = true;
        GameManager.Instance.data.state = GameState.endCredits;

        GameManager.Instance.DoLeaveScene("neighborhood", 1);
    }

    public override void EscapePressed() {
        // EndCutscene();
    }
    public override void CleanUp() {
        foreach (Module module in new List<Module> { startModule, viewingModule, streetModule, newsModule, hqModule }) {
            module.CleanUp();
        }
    }
}