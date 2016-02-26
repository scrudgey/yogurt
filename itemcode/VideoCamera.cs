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
        {"yogurt_vomit", "yogurt emesis"},
        {"yogurt_vomit_eat", "eating yogurt vomit"}
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
	
    // TODO: there could be an issue here with the same occurrence triggering
    // multiple collisions. I will have to handle that eventually.
	void OnTriggerEnter2D(Collider2D col){        
		if (col.name != "OccurrenceFlag(Clone)")
		return;
		Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
		if (occurrence == null)
		return;
        ProcessOccurrence(occurrence);
        // Dest
	}
    
    void ProcessOccurrence(Occurrence oc){
        foreach (OccurrenceData data in oc.data){
            // Debug.Log(data.myType);
            switch (data.myType){
                case occurrenceType.eat:
                    ProcessEat(data as OccurrenceEat);
                    break;
                case occurrenceType.vomit:
                    ProcessVomit(data as OccurrenceVomit);
                    break;
                default:
                    break;
            }
            // handle qualities
            commercial.data.AddData(data);
            // trigger reaction to values increasing?
        }
    }
    
    void ProcessEat(OccurrenceEat data){
        if (data.liquid.name == "Yogurt"){
            IncrementCommercialValue("yogurt", 1f);
            if (data.liquid.vomit)
                IncrementCommercialValue("yogurt_vomit_eat", 1f);
        }
    }
    
    void ProcessVomit(OccurrenceVomit data){
        IncrementCommercialValue("vomit", 1f);
    }
    
    public void IncrementCommercialValue(string valname, float increment){
        
        CommercialProperty property = null;
        commercial.properties.TryGetValue(valname, out property);
        if (property == null){
            commercial.properties[valname] = new CommercialProperty();
        }
        
        float initvalue = commercial.properties[valname].val;
        float finalvalue = initvalue + increment;
        
        string poptext = "default";
        KeyDescriptions.TryGetValue(valname, out poptext);
        if (poptext != "default"){
            Toolbox.Instance.PopupCounter(poptext, initvalue, finalvalue);
        }
        
        commercial.properties[valname].val = finalvalue;
    }
}
