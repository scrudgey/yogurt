using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
public class VideoCamera : MonoBehaviour, IMessagable {
    public static Dictionary<string, string> KeyDescriptions = new Dictionary<string, string>{
        {"yogurt", "yogurts eaten"},
        {"vomit", "vomit events"},
        {"yogurt_vomit", "yogurt emesis event"},
        {"yogurt_vomit_eat", "eating yogurt vomit"},
        {"yogurt_floor", "yogurt eaten off the floor"},
	};
	public Commercial commercial = new Commercial();
    public OccurrenceData watchForOccurrence = null;
    public bool live;
    private ScriptDirector director;

	
	void Start () {
        live = false;
	}
	void SaveCommercial(){
		var serializer = new XmlSerializer(typeof(Commercial));
			string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
			path = Path.Combine(path, "commercial.xml");
			FileStream sceneStream = File.Create(path);
			serializer.Serialize(sceneStream, commercial);
			sceneStream.Close();
	}
    // TODO: there could be an issue here with the same occurrence triggering
    // multiple collisions. I will have to handle that eventually.
	void OnTriggerEnter2D(Collider2D col){        
		if (col.name != "OccurrenceFlag(Clone)" || !live)
		return;
		Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
		if (occurrence == null)
		return;
        ProcessOccurrence(occurrence);
	}
    void ProcessOccurrence(Occurrence oc){
        foreach (OccurrenceData data in oc.data){
            switch (data.myType){
                case occurrenceType.eat:
                    ProcessEat(data as OccurrenceEat);
                    break;
                case occurrenceType.vomit:
                    ProcessVomit(data as OccurrenceVomit);
                    break;
                case occurrenceType.speech:
                    ProcessSpeech(data as OccurrenceSpeech);
                    break;
                default:
                    break;
            }
            // handle qualities
            commercial.data.AddData(data);
            //check vs. watchForOccurrence
            if (watchForOccurrence != null){
                 if (watchForOccurrence.Matches(data)){
                    director.OccurrenceHappened();
                    watchForOccurrence = null;
                 }
            }
        }
    }
    void ProcessEat(OccurrenceEat data){
        if (data.liquid.name == "yogurt"){
            IncrementCommercialValue("yogurt", 1f);
            if (data.liquid.vomit)
                IncrementCommercialValue("yogurt_vomit_eat", 1f);
            if (data.food == "Puddle(Clone)")
                IncrementCommercialValue("yogurt_floor", 1f);
        }
    }
    void ProcessVomit(OccurrenceVomit data){
        IncrementCommercialValue("vomit", 1f);
        if (data.liquid.name == "yogurt"){
            IncrementCommercialValue("yogurt_vomit", 1f);
        }
    }
    void ProcessSpeech(OccurrenceSpeech data){
        //add the speech to the transcript
        commercial.transcript.Add(data.line);
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
            UINew.Instance.PopupCounter(poptext, initvalue, finalvalue, this);
        } else {
            // UI check if commercial is complete
            CheckForFinishState();
        }
        commercial.properties[valname].val = finalvalue;
    }
    public void CheckForFinishState(){
        // UI check if commercial is complete
        UINew.Instance.UpdateRecordButtons(commercial);
    }
    public void ReceiveMessage(Message incoming){
        if (incoming is MessageScript){
            MessageScript message = (MessageScript)incoming;
            director = (ScriptDirector)message.messenger;
            if (message.watchForSpeech != ""){
                OccurrenceSpeech data = new OccurrenceSpeech();
                data.line = message.watchForSpeech;
                watchForOccurrence = data;
            }
            if (message.tomAct != MessageScript.TomAction.none){
                OccurrenceEat data = new OccurrenceEat();
                Liquid newLiquid = new Liquid();
                newLiquid.name = "Yogurt";
                data.liquid = newLiquid;
                
                watchForOccurrence = data;
            }
        }
    }
}
