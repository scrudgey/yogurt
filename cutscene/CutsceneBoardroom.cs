using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;

public class CutsceneBoardroom : Cutscene {
    Regex ampersandHook = new Regex(@"\&\r?$");
    Regex numberHook = new Regex(@"^([\d.]+)");
    Regex lineHook = new Regex(@"^(.*):(.+)");
    Regex endHook = new Regex(@"END");
    private float timer;
    private float globalTimer;
    private float stopTextFade = 1.5f;
    private float startDialogue = 4.5f;
    private float escPromptDelay = 14.5f;
    private bool inDialogue;
    Text settingText;
    Text escPrompt;
    GameObject longShot;
    Speech moe;
    Speech larry;
    Speech curly;
    HeadAnimation moeControl;
    GameObject moeObj;
    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    public override void Configure() {
        configured = true;
        longShot = GameObject.Find("long_shot");
        GameObject canvas = GameObject.Find("Canvas");
        settingText = canvas.transform.Find("text").GetComponent<Text>();
        escPrompt = canvas.transform.Find("escPrompt").GetComponent<Text>();
        escPrompt.gameObject.SetActive(false);
        Color blank = new Color(255, 255, 255, 0);
        settingText.color = blank;
        moeObj = GameObject.Find("moe");
        GameObject larryObj = GameObject.Find("larry");
        GameObject curlyObj = GameObject.Find("curly");
        using (Controller controller = new Controller(moeObj)) {
            controller.SetDirection(Vector2.down);
        }
        using (Controller controller = new Controller(larryObj)) {
            controller.SetDirection(Vector2.right);
        }
        using (Controller controller = new Controller(curlyObj)) {
            controller.SetDirection(Vector2.left);
        }
        moeControl = moeObj.GetComponentInChildren<HeadAnimation>();
        moeControl.DirectionChange(Vector2.down);
        moe = moeObj.GetComponent<Speech>();
        larry = larryObj.GetComponent<Speech>();
        curly = curlyObj.GetComponent<Speech>();
    }
    public override void Update() {
        timer += Time.deltaTime;
        globalTimer += Time.deltaTime;
        if (!inDialogue) {
            if (timer < stopTextFade) {
                Color col = settingText.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTextFade);
                settingText.color = col;
            }
            if (timer >= startDialogue) {
                inDialogue = true;
                longShot.SetActive(false);
                settingText.gameObject.SetActive(false);
                timer = 0;
                globalTimer = 0;
                if (GameManager.Instance.data.activeCommercial == null) {
                    LoadScript("auto");
                } else {
                    if (!LoadScript(GameManager.Instance.data.activeCommercial.cutscene))
                        LoadScript("test");
                }
            }
        } else {
            if (globalTimer > escPromptDelay && globalTimer - escPromptDelay < stopTextFade) {
                escPrompt.gameObject.SetActive(true);
                Color col = escPrompt.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(globalTimer - escPromptDelay, 0, 1, stopTextFade);
                escPrompt.color = col;
            }
            // dialogue scene
            if (moe.speaking || larry.speaking || curly.speaking) {
                timer = 0;
                return;
            }
            if (timer > scriptTimeSpace) {
                timer = 0;
                ProcessLine();
            }
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
                moe.Say(message);
            } else if (match.Groups[1].Value == "LARRY") {
                larry.Say(message);
            } else if (match.Groups[1].Value == "CURLY") {
                curly.Say(message);
            }
        }
        if (line == "<LEFT>") {
            Vector3 scale = new Vector3(-1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.left);
        } else if (line == "<RIGHT>") {
            Vector3 scale = new Vector3(1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.right);
        } else if (line == "<DOWN>") {
            Vector3 scale = new Vector3(1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.down);
        } else if (line == "<UP>") {
            Vector3 scale = new Vector3(1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.up);
        }
        if (endHook.IsMatch(line)) {
            complete = true;
            GameManager.Instance.NewDayCutscene();
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
        TextAsset textData = Resources.Load("data/boardroom/" + filename) as TextAsset;
        if (textData == null) {
            return false;
        } else {
            lines = new List<string>(textData.text.Split('\n'));
            return true;
        }
    }
    public override void EscapePressed() {
        complete = true;
        GameManager.Instance.NewDayCutscene();
    }
}