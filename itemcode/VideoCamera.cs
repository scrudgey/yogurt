using UnityEngine;
using System.Xml.Serialization;
using System.IO;
public class VideoCamera : Interactive, IMessagable {
	public Commercial commercial = new Commercial();
    public OccurrenceData watchForOccurrence = null;
    public bool live;
    private ScriptDirector director;
    public GameObject doneBubble;
	void Awake () {
        live = false;
        doneBubble = transform.Find("doneBubble").gameObject;
        doneBubble.SetActive(false);
	}
    void Start(){
        Interaction stasher = new Interaction(this, "Finish", "FinishButtonClick");
		// stasher.displayVerb = "Stash in";
		stasher.validationFunction = true;
		interactions.Add(stasher);
    }
    public void EnableBubble(){
        doneBubble.SetActive(true);
    }
    public void DisableBubble(){
        doneBubble.SetActive(false);
    }
    public void FinishButtonClick(){
		// VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
		live = false;
		// if (video){
        GameManager.Instance.EvaluateCommercial(commercial);
		// }
	}
    public bool FinishButtonClick_Validation(){
        if (commercial == null)
            return false;
        if (GameManager.Instance.activeCommercial == null)
            return false;
        return commercial.Evaluate(GameManager.Instance.activeCommercial);
    }
    public string FinishButtonClick_desc(){
        return "Finish commercial";
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
            commercial.data.AddData(data);
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
