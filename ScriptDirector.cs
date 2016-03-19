using UnityEngine;
using System.Collections.Generic;
// using System.Text.RegularExpressions;
// using System.IO;

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
        ParseLine();
	}

    void ParseLine(){
        string line = lines[index];
        Debug.Log("next line: "+line);
        if (line.Substring(0, 8) == "COSTAR: "){
            string content = line.Substring(7, line.Length-7);
            foreach (ScriptReader reader in readers){
                reader.CoStarLine(content);
                currentLine = content;
            }
        }
        if (line.Substring(0, 5) == "TOM: "){
            string content = line.Substring(4, line.Length-4);
            foreach (ScriptReader reader in readers){
                currentLine = content;
                tomLineNext = true;
            }
        }
        
        if (line == "[yogurt++]"){
            // watch for tom to eat yogurt
            foreach (ScriptReader reader in readers){
                reader.TomAct("yogurt");
                tomLineNext = true;
            }
        }
    }

    void NextLine(){
        index += 1;
        while (lines[index].Length < 2){
            index += 1;
        }
        ParseLine();
    }
    
    void TriggerNextLine(){
        if (timeToNextLine <= 0){
            timeToNextLine = 0.5f;
            if (tomLineNext){
                Debug.Log("success tom");
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
        // NextLine();
        TriggerNextLine();
    }
    
    public void SpeechCallback(string spoken){
        if (spoken == currentLine){
            currentLine = "";
            TriggerNextLine();
            // NextLine();
        }
    }

}
