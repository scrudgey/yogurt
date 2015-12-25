using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public class VideoCamera : MonoBehaviour {
	// BoxCollider2D view;
	Text rec;
	// Image recBox;
	float blinkTimer;
	// public float yogurtsEaten;
	
	public Commercial commercial = new Commercial();
	
	void Start () {
		// view = transform.Find("Graphic").GetComponent<BoxCollider2D>();
		rec = transform.Find("Graphic/Rec").GetComponent<Text>();
		// recBox = transform.Find("Graphic").GetComponent<Image>();
	}
	
	void Update(){
		blinkTimer += Time.deltaTime;
		if (blinkTimer > 1 && rec.enabled == true){
			rec.enabled = false;
			// recBox.enabled = false;
		}
		if (blinkTimer > 2){
			rec.enabled = true;
			// recBox.enabled = true;
			blinkTimer = 0f;
		}
	}
	
	void OnTriggerEnter2D(Collider2D col){
		if (col.name != "OccurrenceFlag(Clone)")
		return;
		Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
		if (occurrence == null)
		return;
		
		if (occurrence.subjectName.Contains("yogurt") && occurrence.functionName == "Drink"){
			Debug.Log("yogurt eaten");
			commercial.properties["yogurt"].val ++;
			// yogurtsEaten ++;
			SaveCommercial();
            // Debug.Log(EvalCommercial(commercial, "commercial.xml"));
            Debug.Log(EvalVersusAll(commercial));
		}
	}
	
	void SaveCommercial(){
		var serializer = new XmlSerializer(typeof(Commercial));
			string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
			path = Path.Combine(path, "commercial.xml");
			// GameData data = new GameData(this);
			FileStream sceneStream = File.Create(path);
			serializer.Serialize(sceneStream, commercial);
			sceneStream.Close();
	}
    
    bool EvalCommercial(Commercial trial, string templateName){
        var serializer = new XmlSerializer(typeof(Commercial));
        string templatePath = Path.Combine(Application.dataPath, "Resources");
        templatePath = Path.Combine(templatePath, "data");
        templatePath = Path.Combine(templatePath, "commercials");
        templatePath = Path.Combine(templatePath,  templateName);
        if (File.Exists(templatePath)){
				var commercialStream = new FileStream(templatePath, FileMode.Open);
				Commercial templateCommercial = serializer.Deserialize(commercialStream) as Commercial;
				commercialStream.Close();
                return trial.Evaluate(templateCommercial);
        } else {
            Debug.Log("couldn't find " + templatePath);
            return false;
        }
    }
    
    List<Commercial> EvalVersusAll(Commercial trial){
        List<Commercial> passList = new List<Commercial>();
        XmlSerializer serializer = new XmlSerializer(typeof(Commercial));
        Regex reg =  new Regex(@"xml$");
        
        string templateFolder = Path.Combine(Application.dataPath, "Resources");
        templateFolder = Path.Combine(templateFolder, "data");
        templateFolder = Path.Combine(templateFolder, "commercials");
        
        DirectoryInfo info = new DirectoryInfo(templateFolder);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo f in fileInfo){
            if (reg.Matches(f.ToString()).Count == 0)
                continue;
            Debug.Log(f);
            var commercialStream = new FileStream(f.ToString(), FileMode.Open);
            Commercial templateCommercial = serializer.Deserialize(commercialStream) as Commercial;
            commercialStream.Close();
            if (trial.Evaluate(templateCommercial)){
                passList.Add(templateCommercial);
            }
        }
        return passList;
    }
}
