using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;

public class CutsceneAirport : Cutscene {
    enum State { walkdown, walkright, dialogue, exitStageRight, slewEnd }
    State state;
    Regex ampersandHook = new Regex(@"\&\r?$");
    Regex numberHook = new Regex(@"^([\d.]+)");
    Regex lineHook = new Regex(@"^(.*):(.+)");
    Regex endHook = new Regex(@"END");
    private float timer;
    private float globalTimer;
    private float startDialogue = 4.5f;
    // private bool inDialogue;
    GameObject moeObj;
    GameObject larryObj;
    GameObject curlyObj;
    GameObject ceoObj;

    Speech moeSpeech;
    Speech larrySpeech;
    Speech curlySpeech;
    Speech ceoSpeech;

    // TODO: clean these up
    Controller moeControl;
    Controller larryControl;
    Controller curlyControl;
    Controller ceoControl;
    HeadAnimation ceoHeadAnimation;
    Vector3 flipx = new Vector3(-1f, 1f, 1f);

    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    const float LeftmostPoint = -1.185f;
    const float RightmostPoint = 0.365f;
    public bool panicMode;
    public Coroutine moeCoroutine;
    public Coroutine larryCoroutine;
    public Coroutine curlyCoroutine;
    public SoundEffect soundEffect;
    public override void Configure() {
        if (configured)
            return;
        Debug.Log("config cutscene");
        configured = true;
        // GameObject canvas = GameObject.Find("Canvas");
        // Color blank = new Color(255, 255, 255, 0);
        moeObj = GameObject.Find("moe");
        larryObj = GameObject.Find("larry");
        curlyObj = GameObject.Find("curly");
        ceoObj = GameObject.Find("CEO");

        CameraControl camControl = GameObject.FindObjectOfType<CameraControl>();
        camControl.focus = ceoObj;

        soundEffect = GameObject.Find("sounder").GetComponent<SoundEffect>();

        moeControl = new Controller(moeObj);
        larryControl = new Controller(larryObj);
        curlyControl = new Controller(curlyObj);
        ceoControl = new Controller(ceoObj);

        ceoControl.SetDirection(Vector2.down);
        ceoHeadAnimation = ceoObj.GetComponentInChildren<HeadAnimation>();
        ceoHeadAnimation.DirectionChange(Vector2.down);

        moeSpeech = moeObj.GetComponent<Speech>();
        larrySpeech = larryObj.GetComponent<Speech>();
        curlySpeech = curlyObj.GetComponent<Speech>();
        ceoSpeech = ceoObj.GetComponent<Speech>();
        LoadScript("airport");

        state = State.walkdown;


        moeControl.SetDirection(Vector2.right);
        larryControl.SetDirection(Vector2.right);
        curlyControl.SetDirection(Vector2.right);
        foreach (GameObject obj in new List<GameObject> { moeObj, larryObj, curlyObj }) {
            HeadAnimation head = obj.GetComponentInChildren<HeadAnimation>();
            head.DirectionChange(Vector2.right);
        }

        GameManager.Instance.data.ceoFled = true;

        moeObj.SetActive(false);
        larryObj.SetActive(false);
        curlyObj.SetActive(false);
    }

    public override void Update() {
        timer += Time.deltaTime;
        globalTimer += Time.deltaTime;

        if (ceoObj == null && state != State.slewEnd) {
            state = State.slewEnd;
            timer = 0f;
        }

        if (state == State.walkdown) {
            WalkDownUpdate();
        } else if (state == State.walkright) {
            WalkRightUpdate();
        } else if (state == State.dialogue) {
            DialogueUpdate();
        } else if (state == State.exitStageRight) {
            ExitUpdate();
        } else if (state == State.slewEnd) {
            if (timer > 2f) {
                EndCutscene();
            }
        }

    }
    void WalkDownUpdate() {
        // flipx.x *= -1f;
        if (timer < 1f) {
            ceoHeadAnimation.DirectionChange(Vector2.down);
        } else if (timer > 1f && timer < 2f) {
            ceoHeadAnimation.transform.localScale = flipx;
            ceoHeadAnimation.DirectionChange(Vector2.left);
        } else if (timer > 2f && timer < 3f) {
            ceoHeadAnimation.transform.localScale = Vector3.one;
            ceoHeadAnimation.DirectionChange(Vector2.right);
        } else if (timer > 3f && timer < 4f) {
            ceoHeadAnimation.transform.localScale = flipx;
            ceoHeadAnimation.DirectionChange(Vector2.left);
        } else if (timer > 4f && timer < 5f) {
            ceoHeadAnimation.transform.localScale = Vector3.one;
            ceoHeadAnimation.DirectionChange(Vector2.down);
            ceoControl.downFlag = true;
        } else if (timer > 6f && timer < 7f) {
            ceoControl.downFlag = false;
        } else if (timer > 7f) {
            timer = 0f;
            state = State.walkright;
        }
    }
    void WalkRightUpdate() {
        if (timer < 1f) {
            ceoControl.rightFlag = true;
        } else if (timer > 1f && timer < 2f) {
            ceoHeadAnimation.DirectionChange(Vector2.left);
            ceoHeadAnimation.transform.localScale = flipx;
            ceoControl.rightFlag = false;
        } else if (timer > 2f && timer < 2.5f) {
            ceoHeadAnimation.transform.localScale = Vector3.one;
            ceoHeadAnimation.DirectionChange(Vector2.right);
        } else if (timer > 2.5f && timer < 4f) {
            ceoControl.rightFlag = true;
        } else if (timer > 4f && timer < 5f) {
            ceoControl.rightFlag = false;
            moeObj.SetActive(true);
            larryObj.SetActive(true);
            curlyObj.SetActive(true);

            MessageSpeech message = new MessageSpeech("Sir!");
            moeSpeech.Say(message);
            larrySpeech.Say(message);
            curlySpeech.Say(message);


            moeControl.SetDirection(Vector2.right);
            larryControl.SetDirection(Vector2.right);
            curlyControl.SetDirection(Vector2.right);
            foreach (GameObject obj in new List<GameObject> { moeObj, larryObj, curlyObj }) {
                HeadAnimation head = obj.GetComponentInChildren<HeadAnimation>();
                head.DirectionChange(Vector2.right);
            }

            moeControl.rightFlag = true;
            larryControl.rightFlag = true;
            curlyControl.rightFlag = true;
        } else if (timer > 5f) {

            moeControl.SetDirection(Vector2.right);
            larryControl.SetDirection(Vector2.right);
            curlyControl.SetDirection(Vector2.right);
            foreach (GameObject obj in new List<GameObject> { moeObj, larryObj, curlyObj }) {
                HeadAnimation head = obj.GetComponentInChildren<HeadAnimation>();
                head.DirectionChange(Vector2.right);
            }
            timer = 0f;
            state = State.dialogue;
        }
    }
    void ExitUpdate() {
        ceoControl.rightFlag = true;

    }
    void DialogueUpdate() {
        if (timer > 1f) {
            moeControl.rightFlag = false;
            larryControl.rightFlag = false;
            curlyControl.rightFlag = false;
        }
        if (moeSpeech.speaking || larrySpeech.speaking || curlySpeech.speaking || ceoSpeech.speaking) {
            // if (ceoSpeech.speaking) {
            timer = 0;
            return;
        }
        if (timer > scriptTimeSpace) {
            timer = 0;
            ProcessLine();
        }
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
            MessageSpeech message = new MessageSpeech(match.Groups[2].Value);
            if (match.Groups[1].Value == "MOE") {
                moeSpeech.Say(message);
            } else if (match.Groups[1].Value == "LARRY") {
                larrySpeech.Say(message);
            } else if (match.Groups[1].Value == "CURLY") {
                curlySpeech.Say(message);
            } else if (match.Groups[1].Value == "CEO") {
                ceoSpeech.Say(message);
            }
        }
        if (line == "<LEFT>") {
            // Vector3 scale = new Vector3(-1, 1, 1);
            ceoControl.SetDirection(Vector2.left);
        } else if (line == "<RIGHT>") {
            // Vector3 scale = new Vector3(1, 1, 1);
            ceoControl.SetDirection(Vector2.right);
        } else if (line == "<DOWN>") {
            // Vector3 scale = new Vector3(1, 1, 1);
            ceoControl.SetDirection(Vector2.down);
        } else if (line == "<UP>") {
            // Vector3 scale = new Vector3(1, 1, 1);
            ceoControl.SetDirection(Vector2.up);
        } else if (line == "<CEOMUGS>") {
            ceoHeadAnimation.DirectionChange(Vector2.down);
        } else if (line == "<JETIN>") {
            soundEffect.GetComponent<AudioSource>().Play();
        }
        if (endHook.IsMatch(line)) {
            state = State.exitStageRight;
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
    void EndCutscene() {
        complete = true;
        CleanUp();
        GameManager.Instance.ReturnToPhone();
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
    public override void EscapePressed() {
        EndCutscene();
    }
    public override void CleanUp() {
        moeControl.Dispose();
        larryControl.Dispose();
        curlyControl.Dispose();
        ceoControl.Dispose();
    }
}