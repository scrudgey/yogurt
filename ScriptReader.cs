using UnityEngine;
// using System.Collections;

public class ScriptReader : MonoBehaviour {

    public bool videoCamera;
    public bool costar;
    public OccurrenceData watchForOccurrence;
    public ScriptDirector director;
    private VideoCamera videoComponent;
    private Speech speech;
	void Start () {
	//    director = GameObject.FindObjectOfType<ScriptDirector>();
       if (videoCamera){
           videoComponent = GetComponent<VideoCamera>(); 
       }
       if (costar){
           speech = GetComponent<Speech>();
       }
       
	}
    public void CoStarLine(string line){
        // say my line.
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
    
    public void OccurrenceCallback(){
        Debug.Log("occurrence callback from "+gameObject.name);
        director.ReaderCallback();
    }
    
}
