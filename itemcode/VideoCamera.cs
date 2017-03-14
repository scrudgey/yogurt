using UnityEngine;
using System.Xml.Serialization;
using System.IO;
public class VideoCamera : MonoBehaviour, IMessagable {
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
            data.UpdateCommercialOccurrences(commercial);
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
