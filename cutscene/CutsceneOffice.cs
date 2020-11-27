using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;

public class CutsceneOffice : Cutscene {
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
    // HeadAnimation ceoHeadAnimation;
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
        configured = true;
        // GameObject canvas = GameObject.Find("Canvas");
        // Color blank = new Color(255, 255, 255, 0);
        moeObj = GameObject.Find("moe");
        larryObj = GameObject.Find("larry");
        curlyObj = GameObject.Find("curly");
        ceoObj = GameObject.Find("CEO");

        soundEffect = GameObject.Find("sounder").GetComponent<SoundEffect>();

        moeControl = new Controller(moeObj);
        larryControl = new Controller(larryObj);
        curlyControl = new Controller(curlyObj);
        ceoControl = new Controller(ceoObj);

        moeControl.SetDirection(Vector2.down);
        larryControl.SetDirection(Vector2.right);
        curlyControl.SetDirection(Vector2.left);
        // this is not working?
        ceoControl.SetDirection(Vector2.down);
        HeadAnimation ceoHead = ceoObj.GetComponentInChildren<HeadAnimation>();
        ceoHead.DirectionChange(Vector2.down);

        // moeControl = moeObj.GetComponentInChildren<HeadAnimation>();

        moeSpeech = moeObj.GetComponent<Speech>();
        larrySpeech = larryObj.GetComponent<Speech>();
        curlySpeech = curlyObj.GetComponent<Speech>();
        ceoSpeech = ceoObj.GetComponent<Speech>();
        LoadScript("test");

        StartPanicMode();
    }
    public void StartPanicMode() {
        panicMode = true;
        MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.panic, true);
        Toolbox.Instance.SendMessage(larryObj, GameManager.Instance, anim);
        Toolbox.Instance.SendMessage(curlyObj, GameManager.Instance, anim);
        Toolbox.Instance.SendMessage(moeObj, GameManager.Instance, anim);

        moeCoroutine = CutsceneManager.Instance.StartCoroutine(DoPanic(moeControl, Vector2.right));
        larryCoroutine = CutsceneManager.Instance.StartCoroutine(DoPanic(larryControl, Vector2.right));
        curlyCoroutine = CutsceneManager.Instance.StartCoroutine(DoPanic(curlyControl, Vector2.left));
    }
    public void StopPanicMode() {
        panicMode = false;
        MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.panic, false);
        Toolbox.Instance.SendMessage(larryObj, GameManager.Instance, anim);
        Toolbox.Instance.SendMessage(curlyObj, GameManager.Instance, anim);
        Toolbox.Instance.SendMessage(moeObj, GameManager.Instance, anim);

        CutsceneManager.Instance.StopCoroutine(moeCoroutine);
        CutsceneManager.Instance.StopCoroutine(larryCoroutine);
        CutsceneManager.Instance.StopCoroutine(curlyCoroutine);

        foreach (Controller controller in new List<Controller> { moeControl, larryControl, curlyControl }) {
            controller.ResetInput();
            Vector2 direction = controller.controllable.transform.position - ceoControl.controllable.transform.position;
            controller.SetDirection(direction);
            HeadAnimation head = controller.controllable.GetComponentInChildren<HeadAnimation>();
            head.DirectionChange(direction);
        }
        moeSpeech.Stop();
        larrySpeech.Stop();
        curlySpeech.Stop();
        soundEffect.Say("*WHAM*");
        soundEffect.GetComponent<AudioSource>().Play();
        MessagePunch punch = new MessagePunch();
        Toolbox.Instance.SendMessage(ceoObj, GameManager.Instance, punch);
    }
    IEnumerator DoPanic(Controller control, Vector2 initDir) {
        Vector2 direction = initDir;
        while (true) {
            if (control.controllable == null)
                continue;
            if (direction == Vector2.right) {
                control.rightFlag = true;
                if (control.controllable.transform.position.x > RightmostPoint) {
                    control.ResetInput();
                    direction = Vector2.left;
                    yield return new WaitForSeconds(1);
                } else {
                    yield return null;
                }
            } else if (direction == Vector2.left) {
                control.leftFlag = true;
                if (control.controllable.transform.position.x < LeftmostPoint) {
                    control.ResetInput();
                    direction = Vector2.right;
                    yield return new WaitForSeconds(1);
                } else {
                    yield return null;
                }
            }
        }
    }
    public override void Update() {
        timer += Time.deltaTime;
        globalTimer += Time.deltaTime;
        // dialogue scene
        if (ceoSpeech.speaking) {
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
            Vector3 scale = new Vector3(-1, 1, 1);
        } else if (line == "<RIGHT>") {
            Vector3 scale = new Vector3(1, 1, 1);
            ceoControl.SetDirection(Vector2.right);
        } else if (line == "<DOWN>") {
            Vector3 scale = new Vector3(1, 1, 1);
            ceoControl.SetDirection(Vector2.down);
        } else if (line == "<UP>") {
            Vector3 scale = new Vector3(1, 1, 1);
            ceoControl.SetDirection(Vector2.up);
        } else if (line == "<STOP PANIC>") {
            StopPanicMode();
        }
        if (endHook.IsMatch(line)) {
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
        StopPanicMode();
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