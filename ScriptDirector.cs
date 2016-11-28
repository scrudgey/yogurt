using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class ScriptDirector : Interactive {
    public TextAsset script;
    public int index;
    private string[] lines;
    private List<GameObject> readers;
    private float timeToNextLine;
    private bool tomLineNext;
    public VideoCamera video;
    private AudioSource audioSource;
    public AudioClip successSound;
    public bool live;
    private GameObject regionIndicator;
    
	void Start () {
        live = false;
        script = Resources.Load("data/scripts/script1") as TextAsset;
        video = GetComponent<VideoCamera>();
        readers = new List<GameObject>();
        foreach (VideoCamera cam in GameObject.FindObjectsOfType<VideoCamera>()){
            readers.Add(cam.gameObject);
        }
        foreach (DecisionMaker dm in GameObject.FindObjectsOfType<DecisionMaker>()){
            readers.Add(dm.gameObject);
        }
        lines = script.text.Split('\n');
        index = 0;
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        UINew.Instance.SetStatus("-WAIT-");
        UINew.Instance.SetStatusStyle(TextFX.FXstyle.blink);
        ParseLine();
        
        Interaction enableAct = new Interaction(this, "Start", "Enable");
        enableAct.validationFunction = true;
        interactions.Add(enableAct);
        
        regionIndicator = transform.Find("Graphic").gameObject;
        regionIndicator.SetActive(false);

        // Interaction disableAct = new Interaction(this, "Stop", "Disable");
        // disableAct.validationFunction = true;
        // interactions.Add(disableAct);
	}
    
    public IEnumerator WaitAndStartScript(float waitTime){
         yield return new WaitForSeconds(waitTime);
         ParseLine();
    }

    public string Enable_desc(){
        return "Start recording a new commercial";
    }
    public void Enable(){
        // improve logic to catch null
        if (GameManager.Instance.activeCommercial != null){
            live = true;
            video.live = true;
            regionIndicator.SetActive(true);
            UINew.Instance.EnableRecordButtons(true);
            UINew.Instance.UpdateRecordButtons(video.commercial);
            StartCoroutine(WaitAndStartScript(1f));
        } else {
            live = false;
            regionIndicator.SetActive(false);
            UINew.Instance.EnableRecordButtons(false);
            Instantiate(Resources.Load("UI/ScriptSelector"));
        }
    }
    public bool Enable_Validation(){
        return live == false;
    }
    
    public void Disable(){
        live = false;
        video.live = false;
        regionIndicator.SetActive(false);
    }
    public bool Disable_Validation(){
        return live == true;
    }
    void ParseLine(){
        string line = lines[index];
        if (!live)
            return;
        MessageScript message = new MessageScript();

        if (line.Substring(0, 8) == "COSTAR: "){
            UINew.Instance.SetStatus("-WAIT-");
            UINew.Instance.SetStatusStyle(TextFX.FXstyle.blink);
            string content = line.Substring(8, line.Length-8);
            message.coStarLine = content;
            message.watchForSpeech = "Costar: "+content;
        }
        if (line.Substring(0, 5) == "TOM: "){
            UINew.Instance.SetStatus("PROMPT: SAY LINE");
            UINew.Instance.SetStatusStyle(TextFX.FXstyle.normal);
            string content = line.Substring(4, line.Length-4);
            tomLineNext = true;
            message.watchForSpeech = "Tom: "+content;
        }
        if (line == "[yogurt++]"){
            UINew.Instance.SetStatus("PROMPT: EAT YOGURT");
            UINew.Instance.SetStatusStyle(TextFX.FXstyle.normal);
            message.tomAct = MessageScript.TomAction.yogurt;
        }
        foreach (GameObject reader in readers){
            Toolbox.Instance.SendMessage(reader, this, message);
        }
    }

    void NextLine(){
        index += 1;
        // catch here if index is beyond the length of the script.
        if (index == lines.Length){
            Debug.Log("end of script");
            GameManager.Instance.EvaluateCommercial(video.commercial);
            return;
        }
        while (lines[index].Length < 2){
            index += 1;
        }
        ParseLine();
    }
    
    void TriggerNextLine(){
        if (timeToNextLine <= 0){
            timeToNextLine = 1f;
            if (tomLineNext){
                tomLineNext = false;
                UINew.Instance.SetTempStatus("Success!", 1f, TextFX.FXstyle.bounce);
                audioSource.PlayOneShot(successSound);
            }
        }
    }
    void Update(){
        if (timeToNextLine > 0 && live){
            timeToNextLine -= Time.deltaTime;
            if (timeToNextLine <= 0){
                NextLine();
            }
        }
    }
    public string NextTomLine(){
        string tomLine = "What's my line?";
        for (int i = index; i < lines.Length; i++){
            if (lines[i].Length < 2)
                continue;
            if (lines[i].Substring(0, 5) == "TOM: "){
                tomLine = lines[i].Substring(4, lines[i].Length-4);
                break;
            }
        }
        return tomLine;
    }
    public void OccurrenceHappened(){
        TriggerNextLine();
    }

}
