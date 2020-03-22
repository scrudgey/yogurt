using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;

public class CutsceneAntiMayor : Cutscene {
    Regex ampersandHook = new Regex(@"\&\r?$");
    Regex numberHook = new Regex(@"^([\d.]+)");
    Regex lineHook = new Regex(@"^(.*):(.+)");
    Regex endHook = new Regex(@"END");
    private float timer;
    private float globalTimer;
    private float stopTextFade = 1.5f;
    private float startDialogue = 4.5f;
    private bool inDialogue;
    Text settingText;
    // Text escPrompt;
    // GameObject longShot;
    Speech am;
    // Speech larry;
    // Speech curly;
    HeadAnimation amControl;
    Humanoid amHum;
    GameObject amObj;
    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    public override void Configure() {
        configured = true;
        // longShot = GameObject.Find("long_shot");
        GameObject canvas = GameObject.Find("Canvas");
        settingText = canvas.transform.Find("text").GetComponent<Text>();
        Color blank = new Color(255, 255, 255, 0);
        settingText.color = blank;
        amObj = GameObject.Find("AntiMayor");
        amHum = amObj.GetComponent<Humanoid>();
        amControl = amObj.GetComponentInChildren<HeadAnimation>();
        amHum.SetDirection(Vector2.right);
        amControl.DirectionChange(Vector2.right);
        am = amObj.GetComponent<Speech>();
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
                // longShot.SetActive(false);
                settingText.gameObject.SetActive(false);
                timer = 0;
                globalTimer = 0;
                if (GameManager.Instance.data.activeCommercial == null) {
                    LoadScript("am1");
                } else {
                    if (!LoadScript(GameManager.Instance.data.activeCommercial.cutscene))
                        LoadScript("am1");
                }
            }
        } else {
            // dialogue scene
            if (am.speaking) {
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
            if (match.Groups[1].Value == "AM") {
                am.Say(message);
            }
        }
        if (line == "<LEFT>") {
            Vector3 scale = new Vector3(-1, 1, 1);
            amObj.transform.localScale = scale;
            amControl.DirectionChange(Vector2.left);
            amHum.SetDirection(Vector2.left);
        } else if (line == "<RIGHT>") {
            Vector3 scale = new Vector3(1, 1, 1);
            amObj.transform.localScale = scale;
            amControl.DirectionChange(Vector2.right);
            amHum.SetDirection(Vector2.right);
        } else if (line == "<DOWN>") {
            Vector3 scale = new Vector3(1, 1, 1);
            amObj.transform.localScale = scale;
            amControl.DirectionChange(Vector2.down);
            amHum.SetDirection(Vector2.down);
        } else if (line == "<UP>") {
            Vector3 scale = new Vector3(1, 1, 1);
            amObj.transform.localScale = scale;
            amControl.DirectionChange(Vector2.up);
            amHum.SetDirection(Vector2.up);
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