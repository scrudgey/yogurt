using UnityEngine;
using System.Collections.Generic;

public class ScriptDirector : Interactive {

    public TextAsset script;
    public int index;
    private string[] lines;
    private List<ScriptReader> readers;
    // private string currentLine;
    private float timeToNextLine;
    private bool tomLineNext;
    private VideoCamera video;
    private AudioSource audioSource;
    public AudioClip successSound;
    private bool _live;
    public bool live {
        get {
            return _live;
        }
        set {
            _live = value;
            CheckLiveStatus();
        }
    }
    
	void Start () {
        live = false;
        script = Resources.Load("data/scripts/script1") as TextAsset;
        video = GetComponent<VideoCamera>();
        readers = new List<ScriptReader>(GameObject.FindObjectsOfType<ScriptReader>());
        foreach (ScriptReader reader in readers){
            reader.director = this;
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
        
        Interaction disableAct = new Interaction(this, "Stop", "Disable");
        disableAct.validationFunction = true;
        interactions.Add(disableAct);
	}
    
    public void Enable(){
        live = true;
    }
    public bool Enable_Validation(){
        return live == false;
    }
    
    public void Disable(){
        live = false;
    }
    public bool Disable_Validation(){
        return live == true;
    }
    void CheckLiveStatus(){
        if (live){
            UINew.Instance.status.gameObject.SetActive(true);
            ParseLine();
        } else {
            // UINew.Instance.status.gameObject.SetActive(false);
        }
    }

    void ParseLine(){
        string line = lines[index];
        if (!live)
            return;
        if (line.Substring(0, 8) == "COSTAR: "){
            UINew.Instance.SetStatus("-WAIT-");
            UINew.Instance.SetStatusStyle(TextFX.FXstyle.blink);
            string content = line.Substring(7, line.Length-7);
            foreach (ScriptReader reader in readers){
                reader.CoStarLine(content, this);
                reader.WatchForSpeech(content);
                // currentLine = content;
            }
        }
        if (line.Substring(0, 5) == "TOM: "){
            UINew.Instance.SetStatus("PROMPT: SAY LINE");
            UINew.Instance.SetStatusStyle(TextFX.FXstyle.normal);
            string content = line.Substring(4, line.Length-4);
            // currentLine = content;
            tomLineNext = true;
            foreach (ScriptReader reader in readers){
                // reader.TomLine(line);
                reader.WatchForSpeech(content);
            }
        }
        if (line == "[yogurt++]"){
            UINew.Instance.SetStatus("PROMPT: EAT YOGURT");
            UINew.Instance.SetStatusStyle(TextFX.FXstyle.normal);
            // watch for tom to eat yogurt
            foreach (ScriptReader reader in readers){
                reader.TomAct("yogurt");
                tomLineNext = true;
            }
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
    
    public void ReaderCallback(){
        TriggerNextLine();
    }
    
    // public void SpeechCallback(string spoken){
    //     if (spoken == currentLine){
    //         currentLine = "";
    //         TriggerNextLine();
    //     }
    // }

}
