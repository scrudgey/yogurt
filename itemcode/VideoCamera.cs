using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
// using System.Text.RegularExpressions;
public class VideoCamera : MonoBehaviour {
    public static Dictionary<string, string> KeyDescriptions = new Dictionary<string, string>{
        {"yogurt", "yogurts eaten"},
        {"vomit", "vomit events"},
	};
	Text rec;
	float blinkTimer;
    GameObject cutButton;
    float interfaceTimeout;
	public Commercial commercial = new Commercial();
	
	void Start () {
		rec = transform.Find("Graphic/Rec").GetComponent<Text>();
        cutButton = transform.Find("Canvas/CutButton").gameObject;
        cutButton.SetActive(false);
        // Toolbox.Instance.PopupCounter("yogurts eaten", 0f, 1f);
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
        ProcessOccurrence(occurrence);
	}
    
    // Handle all the various occurrences
    void ProcessOccurrence(Occurrence oc){
        if (oc.subjectName.Contains("yogurt") && oc.functionName == "Drink"){
            IncrementCommercialValue("yogurt", 1f);
		}
    }
    
    public void IncrementCommercialValue(string valname, float increment){
        
        float initvalue = commercial.properties[valname].val;
        float finalvalue = initvalue + increment;
        
        string poptext = "default";
        KeyDescriptions.TryGetValue(valname, out poptext);
        if (poptext != "default"){
            Toolbox.Instance.PopupCounter(poptext, initvalue, finalvalue);
        }
        
        commercial.properties[valname].val = finalvalue;
    }
    
    public void CalledCut(){
        SaveCommercial();
        GameManager.Instance.EvaluateCommercial(commercial);
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

}
