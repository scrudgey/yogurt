using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using Easings;
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
		stasher.validationFunction = true;
		interactions.Add(stasher);
    }
    public void EnableBubble(){
        doneBubble.SetActive(true);
        Transform bubbleImage = doneBubble.transform.Find("bubbleFrame1/Image");
        StartCoroutine(EaseIn(bubbleImage));
    }
    IEnumerator EaseIn(Transform target) {
        float t = 0;
        while (t < 1){
            t += Time.deltaTime;
            Vector3 newScale = target.localScale;
            newScale.x = (float)PennerDoubleAnimation.ElasticEaseOut(t, 0.5, 0.5, 1);
            newScale.y = (float)PennerDoubleAnimation.ElasticEaseOut(t, 0.5, 0.5, 1);
            target.localScale = newScale;
            yield return new WaitForEndOfFrame();
        }
    }
    public void DisableBubble(){
        doneBubble.SetActive(false);
    }
    public void FinishButtonClick(){
		live = false;
        GameManager.Instance.EvaluateCommercial(commercial);
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
        foreach (OccurrenceData occurrence in oc.data){
            foreach(EventData data in occurrence.events){
                commercial.IncrementValue(data);
            }
            commercial.eventData.AddRange(occurrence.events);
            foreach(EventData data in occurrence.events){
                if (data.transcriptLine != null){
                    commercial.transcript.Add(data.transcriptLine);
                }
            }
            // }
            // if (watchForOccurrence != null){
                //  if (watchForOccurrence.Matches(data)){
                //     director.OccurrenceHappened();
                //     watchForOccurrence = null;
                //  }
            // }
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
