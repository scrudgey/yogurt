using UnityEngine;
using UnityEngine.UI;
using Easings;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class Cutscene {
    public virtual void Update(){}
    public virtual void Configure(){}
    public bool complete;
}

public class CutsceneBoardroom : Cutscene {
    private float timer;
    private float stopTextFade = 1.5f;
    private float startDialogue = 4.5f;
    private bool inDialogue;
    Text settingText;
    GameObject longShot;
    Speech moe;
    Speech larry;
    Speech curly;
    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    public override void Configure(){
        longShot = GameObject.Find("long_shot");
        GameObject canvas = GameObject.Find("Canvas");
        settingText = canvas.transform.Find("text").GetComponent<Text>();
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
                LoadScript("test");
            }
        } else {
            // dialogue scene
            if (moe.speaking || larry.speaking || curly.speaking){
                timer = 0;
                return;
            }
            if (timer > scriptTimeSpace){
                timer = 0;
                if (lines[index].Substring(0, 4) == "MOE:"){
                    moe.Say(lines[index].Substring(4));
                }
                if (lines[index].Substring(0, 6) == "LARRY:"){
                    larry.Say(lines[index].Substring(6));
                }
                if (lines[index].Substring(0, 6) == "CURLY:"){
                    curly.Say(lines[index].Substring(6));
                }
                if (lines[index].Substring(0, 3) == "END"){
                    complete = true;
                }
                index += 1;
            }
        }
    }
    void LoadScript(string filename){
        Regex node_hook = new Regex(@"^(\d)>(.+)", RegexOptions.Multiline);
		Regex response_hook = new Regex(@"^(\d)\)(.+)");
		TextAsset textData = Resources.Load("data/boardroom/"+filename) as TextAsset;
        lines = new List<string>(textData.text.Split('\n'));
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
            cutscene.Configure();
            break;
            default:
            break;
        }
    }
    public void LevelWasLoaded(Scene scene, LoadSceneMode mode){
        if (cutscene == null)
            return;
        if (cutscene.complete){
            cutscene = null;
            return;
        }
        if (cutscene is CutsceneNewDay)
            cutscene.Configure();
    }

    void Update(){
        if (cutscene == null)
            return;
        if (cutscene.complete){
            cutscene = null;
        } else {
            cutscene.Update();
        }
    }
}
