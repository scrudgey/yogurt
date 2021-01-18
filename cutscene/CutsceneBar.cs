using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;

public class CutsceneBar : Cutscene {
    enum State { normal, bartenderWalkLeft, bartenderWait, bartenderWaitRight }
    State state;
    Regex ampersandHook = new Regex(@"\&\r?$");
    Regex numberHook = new Regex(@"^([\d.]+)");
    Regex lineHook = new Regex(@"^(.*):(.+)");
    Regex endHook = new Regex(@"END");
    private float timer;
    private float globalTimer;
    private float startDialogue = 4.5f;
    GameObject moeObj;
    GameObject larryObj;
    GameObject curlyObj;
    GameObject bartenderObj;
    // GameObject ceoObj;
    Controller bartenderControl;
    readonly float LeftMostPoint = 0.372f;

    Speech moeSpeech;
    Speech larrySpeech;
    Speech curlySpeech;
    Speech bartenderSpeech;
    Controller moeControl;
    SpriteRenderer moeSprite;

    DespondentBar larryDespondent;
    DespondentBar curlyDespondent;

    AdvancedAnimation moeAnimation;
    Animation moeAnimate;

    Sprite phoneUp;
    Sprite phoneDown;

    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    public SoundEffect soundEffect;
    public override void Configure() {
        if (configured)
            return;
        Debug.Log("config cutscene");
        configured = true;
        moeObj = GameObject.Find("moe");
        larryObj = GameObject.Find("larry");
        curlyObj = GameObject.Find("curly");
        bartenderObj = GameObject.Find("bartender");

        CameraControl camControl = GameObject.FindObjectOfType<CameraControl>();

        soundEffect = GameObject.Find("sounder").GetComponent<SoundEffect>();

        moeControl = new Controller(moeObj);
        moeSpeech = moeObj.GetComponent<Speech>();
        larrySpeech = larryObj.GetComponent<Speech>();
        curlySpeech = curlyObj.GetComponent<Speech>();

        larryDespondent = larryObj.GetComponent<DespondentBar>();
        curlyDespondent = curlyObj.GetComponent<DespondentBar>();

        bartenderControl = new Controller(bartenderObj);
        bartenderSpeech = bartenderObj.GetComponent<Speech>();

        moeAnimate = moeObj.GetComponent<Animation>();
        moeAnimation = moeObj.GetComponent<AdvancedAnimation>();

        moeSprite = moeObj.GetComponent<SpriteRenderer>();


        phoneUp = Resources.Load<Sprite>("sprites/MoePhone") as Sprite;
        phoneDown = moeSprite.sprite;
        // Debug.Log(phoneDown);
        // Debug.Log(phoneUp);

        LoadScript("bar");
    }

    public override void Update() {
        timer += Time.deltaTime;
        globalTimer += Time.deltaTime;
        switch (state) {
            default:
            case State.normal:
                DialogueUpdate();
                break;
            case State.bartenderWait:
            case State.bartenderWalkLeft:
            case State.bartenderWaitRight:
                BartenderUpdate();
                break;
        }
    }
    void BartenderUpdate() {
        switch (state) {
            case State.bartenderWait:
                if (timer > 3f) {
                    state = State.bartenderWaitRight;
                    timer = 0f;
                }
                break;
            case State.bartenderWalkLeft:
                if (bartenderObj.transform.position.x > LeftMostPoint) {
                    bartenderControl.leftFlag = true;
                } else {
                    bartenderControl.ResetInput();
                    bartenderControl.SetDirection(Vector2.down);
                    state = State.bartenderWait;
                    timer = 0f;
                    bartenderSpeech.Say(new MessageSpeech("Last call, fellas."));
                }
                break;
            case State.bartenderWaitRight:
                bartenderControl.rightFlag = true;
                if (timer > 2f) {
                    bartenderControl.ResetInput();
                    bartenderControl.SetDirection(Vector2.down);
                    state = State.normal;
                    timer = 0f;
                }
                break;
        }
    }
    void DialogueUpdate() {
        moeControl.SetDirection(Vector2.down);
        if (moeSpeech.speaking || larrySpeech.speaking || curlySpeech.speaking) {
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
            }
        } else {
            if (line == "<LARRYPOUND>") {
                larryDespondent.PoundFist();
            } else if (line == "<CURLYPOUND>") {
                curlyDespondent.PoundFist();
            } else if (line == "<BART>") {
                state = State.bartenderWalkLeft;
                timer = 0f;
            } else if (line == "<PHONECALL>") {
                phoneDown = moeSprite.sprite;
                soundEffect.GetComponent<AudioSource>().Play();
                // set Moe sprite
                moeSprite.sprite = phoneUp;
                moeAnimate.Stop();
                moeAnimate.enabled = false;
                moeAnimation.enabled = false;
            } else if (line == "<PHONEEND>") {
                // soundEffect.GetComponent<AudioSource>().Play();
                // set Moe sprite
                moeSprite.sprite = phoneDown;
            }
        }
        if (endHook.IsMatch(line)) {
            // state = State.exitStageRight;
            EndCutscene();
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
        // moeControl.Dispose();
        // larryControl.Dispose();
        // curlyControl.Dispose();
        // ceoControl.Dispose();
        bartenderControl.Dispose();
    }
}