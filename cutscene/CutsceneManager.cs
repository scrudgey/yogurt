using UnityEngine;
using UnityEngine.UI;
using Easings;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class Cutscene {
    public virtual void Update(){}
    public virtual void Configure(){}
    public virtual void EscapePressed(){}
    public virtual void CleanUp(){}
    public bool complete;
    public bool configured;
}

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
    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    public override void Configure(){
        configured = true;
        longShot = GameObject.Find("long_shot");
        GameObject canvas = GameObject.Find("Canvas");
        settingText = canvas.transform.Find("text").GetComponent<Text>();
        escPrompt = canvas.transform.Find("escPrompt").GetComponent<Text>();
        escPrompt.gameObject.SetActive(false);
        Color blank = new Color(255, 255, 255, 0);
        settingText.color = blank;
        GameObject moeObj = GameObject.Find("moe");
        GameObject larryObj = GameObject.Find("larry");
        GameObject curlyObj = GameObject.Find("curly");
        moeObj.GetComponent<Humanoid>().SetDirection(Vector2.down);
        larryObj.GetComponent<Humanoid>().SetDirection(Vector2.right);
        curlyObj.GetComponent<Humanoid>().SetDirection(Vector2.left);
        moe = moeObj.GetComponent<Speech>();
        larry = larryObj.GetComponent<Speech>();
        curly = curlyObj.GetComponent<Speech>();
    }
    public override void Update(){
        timer += Time.deltaTime;
        globalTimer += Time.deltaTime;
        if (!inDialogue){
            if (timer < stopTextFade){
                Color col = settingText.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTextFade);
                settingText.color = col;
            }
            if (timer >= startDialogue){
                inDialogue = true;
                longShot.SetActive(false);
                settingText.gameObject.SetActive(false);
                timer = 0;
                globalTimer = 0;
                if (GameManager.Instance.activeCommercial == null){
                    LoadScript("eggplant1");
                } else {
                    if (!LoadScript(GameManager.Instance.activeCommercial.cutscene))
                        LoadScript("test");
                }
            }
        } else {
            if (globalTimer > escPromptDelay && globalTimer - escPromptDelay < stopTextFade){
                escPrompt.gameObject.SetActive(true);
                Color col = escPrompt.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(globalTimer - escPromptDelay, 0, 1, stopTextFade);
                escPrompt.color = col;
            }
            // dialogue scene
            if (moe.speaking || larry.speaking || curly.speaking){
                timer = 0;
                return;
            }
            if (timer > scriptTimeSpace){
                timer = 0;
                ProcessLine();
            }
        }
    }
    void ProcessLine(){
        bool amp = false;
        string line = lines[index];
        if (ampersandHook.IsMatch(line)){
            amp = true;
            line = line.Substring(0, line.Length-1);
        }
        if (lineHook.IsMatch(line)){
            Match match = lineHook.Match(line);
            if (match.Groups[1].Value == "MOE"){
                moe.Say(match.Groups[2].Value);
            } else if (match.Groups[1].Value == "LARRY"){
                larry.Say(match.Groups[2].Value);
            } else if (match.Groups[1].Value == "CURLY"){
                curly.Say(match.Groups[2].Value);
            }
        }
        if (endHook.IsMatch(line)){
            complete = true;
            GameManager.Instance.NewDayCutscene();
        }
        if (index + 1 < lines.Count-1){
            if (numberHook.IsMatch(lines[index + 1])){
                Match match = numberHook.Match(lines[index + 1]);
                scriptTimeSpace = float.Parse(match.Groups[1].Value);
                index += 1;
            }
        }
        index += 1;
        if (amp)
            ProcessLine();
    }
    bool LoadScript(string filename){
		TextAsset textData = Resources.Load("data/boardroom/"+filename) as TextAsset;
        if (textData == null){
            return false;
        } else {
            lines = new List<string>(textData.text.Split('\n'));
            return true;
        }
    }
    public override void EscapePressed(){
        complete = true;
        GameManager.Instance.NewDayCutscene();
    }
}
public class CutsceneMayor : Cutscene {
    private GameObject spawnPoint;
    private GameObject mayor;
    private Humanoid mayorControl;
    private DecisionMaker mayorAI;
    private Speech mayorSpeech;
    private bool inPosition;
    private bool walkingAway;
    public override void Configure(){
        configured = true;
        spawnPoint = GameObject.Find("mayorSpawnpoint");
        mayor = GameObject.Instantiate(Resources.Load("prefabs/Mayor"), spawnPoint.transform.position, Quaternion.identity) as GameObject;
        mayorControl = mayor.GetComponent<Humanoid>();
        mayorAI = mayor.GetComponent<DecisionMaker>();
        mayorSpeech = mayor.GetComponent<Speech>();

        mayorAI.enabled = false;
        Controller.Instance.suspendInput = true;

        UINew.Instance.SetActiveUI();
    }
    public override void Update(){
        if (!inPosition){
            mayorControl.rightFlag = true;
        }
        if (!inPosition && Vector3.Distance(mayor.transform.position, GameManager.Instance.playerObject.transform.position) < 0.25){
            inPosition = true;
            mayorControl.ResetInput();
            DialogueMenu menu = mayorSpeech.SpeakWith();
            menu.menuClosed += MenuWasClosed;
            menu.LoadDialogueTree("mayor");
        }
        if (walkingAway){
            mayorControl.leftFlag = true;
            if (mayor.transform.position.x < spawnPoint.transform.position.x){
                Object.Destroy(mayor);
                Controller.Instance.suspendInput = false;
                complete = true;
                UINew.Instance.SetActiveUI(active:true);
            }
        }
    }
    public override void CleanUp(){
        Controller.Instance.suspendInput = false;
    }
    public void MenuWasClosed(){
        walkingAway = true;
    }
}
public class CutsceneNewDay : Cutscene {
    private float timer;
    Text tomText;
    Text dayText;
    GameObject canvas;
    private float stopTomFade = 1.5f;
    private float startDayFade = 1.5f;
    private float stopDayFade = 2.5f;
    private float stopTime = 12f;
    public override void Configure(){
        configured = true;
        canvas = GameObject.Find("Canvas");
        tomText = canvas.transform.Find("tomText").GetComponent<Text>();
        dayText = canvas.transform.Find("dayText").GetComponent<Text>();

        dayText.text = "Day "+GameManager.Instance.data.days.ToString();

        Color blank = new Color(255, 255, 255, 0);
        tomText.color = blank;
        dayText.color = blank;
    }
    public override void Update(){
        timer += Time.deltaTime;
        if (timer < stopTomFade){
            Color col = tomText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTomFade);
            tomText.color = col;
        }
        if (timer > startDayFade && timer < stopDayFade){
            Color col = dayText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer - startDayFade, 0, 1, stopDayFade - startDayFade);
            dayText.color = col;
        }
        if (timer >= stopTime || Input.anyKey){
            complete = true;
            GameManager.Instance.NewDay();
        }
    }
}

public class CutsceneManager : Singleton<CutsceneManager> {
    public enum CutsceneType {newDay, mayorTalk, boardRoom}
    public Cutscene cutscene;
    void Start (){
        SceneManager.sceneLoaded += LevelWasLoaded;
    }
    public void InitializeCutscene(CutsceneType scene){
        switch (scene)
        {
            case CutsceneType.newDay:
            cutscene = new CutsceneNewDay();
            break;
            case CutsceneType.mayorTalk:
            cutscene = new CutsceneMayor();
            cutscene.Configure();
            break;
            case CutsceneType.boardRoom:
            cutscene = new CutsceneBoardroom();
            break;
            default:
            break;
        }
    }
    public void LevelWasLoaded(Scene scene, LoadSceneMode mode){
        if (cutscene == null)
            return;
        if (cutscene.complete){
            cutscene.CleanUp();
            cutscene = null;
            return;
        }
        if (cutscene.configured == false){
            if (cutscene is CutsceneNewDay || cutscene is CutsceneBoardroom){
                cutscene.Configure();
            }
        } else if (cutscene is CutsceneNewDay || cutscene is CutsceneBoardroom) {
            cutscene.CleanUp();
            cutscene = null;
        }
        
    }
    void Update(){
        if (cutscene == null)
            return;
        if (cutscene.complete){
            cutscene.CleanUp();
            cutscene = null;
        } else {
            if (cutscene.configured)
                cutscene.Update();
        }
    }
    public void EscapePressed(){
        // Debug.Log("escape pressed");    
        if (cutscene != null)
            cutscene.EscapePressed();
    }
}
