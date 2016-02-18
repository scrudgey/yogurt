using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public class VideoCamera : MonoBehaviour {
	Text rec;
	float blinkTimer;
    GameObject cutButton;
    float interfaceTimeout;
	
	public Commercial commercial = new Commercial();
	
	void Start () {
		rec = transform.Find("Graphic/Rec").GetComponent<Text>();
        cutButton = transform.Find("Canvas/CutButton").gameObject;
        cutButton.SetActive(false);
	}
	
	void Update(){
		blinkTimer += Time.deltaTime;
		if (blinkTimer > 1 && rec.enabled == true){
			rec.enabled = false;
		}
		if (blinkTimer > 2){
			rec.enabled = true;
			blinkTimer = 0f;
		}
        if (interfaceTimeout <= 0){
            if (cutButton.activeSelf){
                cutButton.SetActive(false);
            }
        } else {
            cutButton.SetActive(true);
            interfaceTimeout -= Time.deltaTime;
        }
	}
	
	void OnTriggerEnter2D(Collider2D col){        
		if (col.name != "OccurrenceFlag(Clone)")
		return;
		Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
		if (occurrence == null)
		return;
		if (occurrence.subjectName.Contains("yogurt") && occurrence.functionName == "Drink"){
			commercial.properties["yogurt"].val ++;
		}
	}
    
    public void CalledCut(){
        SaveCommercial();
        List<Commercial> success = EvalVersusAll(commercial);
        // Debug.Log(EvalVersusAll(commercial).Count);
        if (success.Count > 0){
            GameObject report = Instantiate(Resources.Load("UI/CommercialReport")) as GameObject;
            report.GetComponent<CommercialReportMenu>().Report(success[0]);
        }
        commercial = new Commercial();
    }
    
    void OnTriggerStay2D(Collider2D col){
        if (col.gameObject == GameManager.Instance.playerObject){
            interfaceTimeout = 0.5f;
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
