using UnityEngine;
// using System.Collections;

public class ScriptReader : MonoBehaviour {

    public bool videoCamera;
    public bool costar;
    public OccurrenceData watchForOccurrence;
    public ScriptDirector director;
    private VideoCamera videoComponent;
    private Speech speech;
    private Controllable controllable;
	void Start () {
        controllable = GetComponent<Controllable>();
        if (videoCamera){
            videoComponent = GetComponent<VideoCamera>(); 
        }
        if (costar){
            speech = GetComponent<Speech>();
        }
       
	}
    public void CoStarLine(string line, ScriptDirector director){
        if (costar){
            // if i am a costar, look at the camera.
            Vector3 dif = director.transform.position - transform.position;
            Vector2 direction = (Vector2)dif;
            controllable.direction = direction;
            controllable.SetDirection(direction);
        }
        // say my line.
        speech = GetComponent<Speech>();
        if (!costar)
            return;
        speech.Say(line);
    }


    public void TomAct(string eventKey){
        // set the watch-for occurrence to be Tom doing something.
        if (!videoCamera)
            return;
        if (eventKey == "yogurt"){
            OccurrenceEat data = new OccurrenceEat();
            Liquid newLiquid = new Liquid();
            newLiquid.name = "Yogurt";
            data.liquid = newLiquid;
            
            videoComponent.watchForOccurrence = data;
        }
    }
    
    public void WatchForSpeech(string line){
        if (!videoCamera)
            return;
        OccurrenceSpeech data = new OccurrenceSpeech();
        data.line = line;
        videoComponent.watchForOccurrence = data;
    }
    
    public void Cut(){
        if (videoComponent){
            GameManager.Instance.EvaluateCommercial(videoComponent.commercial);
        }
    }
    
    public void OccurrenceCallback(){
        director.ReaderCallback();
    }
    
}
