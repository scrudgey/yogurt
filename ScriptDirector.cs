using UnityEngine;
using System.Collections.Generic;

public class ScriptDirector : MonoBehaviour {

    public TextAsset script;
    public int index;
    private string[] lines;
    private List<ScriptReader> readers;
    private string currentLine;
    private float timeToNextLine;
    private bool tomLineNext;
    
	void Start () {
        script = Resources.Load("data/scripts/script1") as TextAsset;
        readers = new List<ScriptReader>(GameObject.FindObjectsOfType<ScriptReader>());
        foreach (ScriptReader reader in readers){
            reader.director = this;
        }
        lines = script.text.Split('\n');
        index = 0;
        Debug.Log(lines[index]);
        UINew.Instance.status.gameObject.SetActive(true);
        // UINew.Instance.status.text = "- WAIT -";
        UINew.Instance.SetStatus("-WAIT-");
        ParseLine();
	}

    void ParseLine(){
        string line = lines[index];
        Debug.Log("next line: "+line);
        if (line.Substring(0, 8) == "COSTAR: "){
            UINew.Instance.SetStatus("-WAIT-");
            string content = line.Substring(7, line.Length-7);
            foreach (ScriptReader reader in readers){
                reader.CoStarLine(content);
                currentLine = content;
            }
        }
        if (line.Substring(0, 5) == "TOM: "){
            UINew.Instance.SetStatus("PROMPT: SAY LINE");
            string content = line.Substring(4, line.Length-4);
            currentLine = content;
            tomLineNext = true;
        }
        if (line == "[yogurt++]"){
            UINew.Instance.SetStatus("PROMPT: EAT YOGURT");
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
                Debug.Log("success tom");
                // Toolbox.Instance.BounceText("Success!", GameManager.Instance.playerObject);
                UINew.Instance.SetTempStatus("Success!", 1f);
            }
        }
    }
    
    void Update(){
        if (timeToNextLine > 0){
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
    
    public void SpeechCallback(string spoken){
        if (spoken == currentLine){
            currentLine = "";
            TriggerNextLine();
        }
    }

}
