using UnityEngine;
// using System.Collections;
using System.Collections.Generic;


public class Perception{
	public enum tag{none,saw_object,was_hurt,saw_person}
	public tag perceptType;
	public GameObject target;
}

public class Knowledge{
	public enum tag{edible,weapon};
	public GameObject obj;
	public Quality quality;
	public Vector3 lastSeenPosition;

	public Knowledge(GameObject o){
		obj = o;
		Qualities q = o.GetComponent<Qualities>();
		if (q)
			quality = q.quality;
		lastSeenPosition = o.transform.position;
	}
}

public class PersonalAssessment{
	public enum tag{neutral, friend, enemy}
	public tag friendStatus;
	public float feelingToward;
	public float harmDealt;
}

public class Awareness : MonoBehaviour {
	private GameObject sightCone;
	private Vector3 sightConeScale;
	private Controllable control;
	private float speciousPresent; 
	private List<GameObject> fieldOfView = new List<GameObject>();
	private bool viewed;
	public Dictionary<GameObject,Knowledge> objects = new Dictionary<GameObject,Knowledge>();
	public Dictionary<GameObject,PersonalAssessment> people = new Dictionary<GameObject, PersonalAssessment>();
	public EntityController controller;

	void Start () {
		control = gameObject.GetComponent<Controllable>();
		sightCone = Instantiate( Resources.Load("sightcone1"), gameObject.transform.position, Quaternion.identity ) as GameObject;
		sightConeScale = sightCone.transform.localScale;
		sightCone.transform.parent = transform;
		controller = gameObject.GetComponent<EntityController>();
	}

	// not going to work right now
	public List<GameObject> FindObjectWithTag(Knowledge.tag  tag ){
		List<GameObject> returnArray = new List<GameObject>();
//		foreach (Knowledge k in objects.Values){
////			if (k.attributes.Contains(tag))
////				returnArray.Add(k.obj);
//		}
		return returnArray;
	}

	public List<GameObject> FindObjectWithName(string targetName){
		List<GameObject> returnArray = new List<GameObject>();
		List<GameObject> removeArray = new List<GameObject>();
		foreach (Knowledge k in objects.Values){
			if (k.obj){
				if (k.obj.name == targetName)
					returnArray.Add(k.obj);
			} else {
				removeArray.Add(k.obj);
			}
		}
		foreach(GameObject g in removeArray){
			objects.Remove(g);
		}
		return returnArray;
	}
	
	void Update () {
		// update sight cone rotation and scale -- point it in the right direction.
		if (transform.localScale.x < 0 && sightCone.transform.localScale.x > 0){
			Vector3 tempscale = sightConeScale;
			tempscale.x = -1 * sightConeScale.x;
			sightCone.transform.localScale = tempscale;
		} 
		if (transform.localScale.x > 0 && sightCone.transform.localScale.x < 0){
			sightCone.transform.localScale = sightConeScale;
		}
		float rot_z = Mathf.Atan2(control.direction.y, control.direction.x) * Mathf.Rad2Deg;
		sightCone.transform.rotation = Quaternion.Euler(0f, 0f, rot_z );

		// work the timer for the discrete perception updates
		speciousPresent -= Time.deltaTime;
		if (speciousPresent <= 0 && fieldOfView.Count > 0 && viewed == true){
			Perceive();
		}
	}

	// if its time to run the perception, add all triggerstay colliders to the list.
	// we don't know when this will be run or how many times, so i need a boolean to track
	// whether it has run this cycle yet or not.
	void OnTriggerStay2D(Collider2D other){
		if (speciousPresent <= 0){
			if (viewed == false){
				fieldOfView = new List<GameObject>();
				viewed = true;
			}
			if (other.tag == "Physical")
				fieldOfView.Add(other.gameObject);
		}
	}
	// process the list of objects in the field of view.
	void Perceive(){
		viewed = false;
		speciousPresent = 1f;
		foreach (GameObject g in fieldOfView){
			Qualities q = Toolbox.Instance.GetQuality(g);
			Quality quality = q.quality;
			if (quality.flaming && controller.priority.name != "extinguish"){
				controller.priority.ExtinguishObject(g);
				controller.CheckPriority();
			}
		}
	}
	void OnTriggerEnter2D(Collider2D col){
		List<GameObject> keys = new List<GameObject>(objects.Keys);
		if (! keys.Contains(col.gameObject) && col.tag == "Physical" ){
			Knowledge newKnowledge = new Knowledge(col.gameObject);
			objects.Add(col.gameObject,newKnowledge);
			React(col.gameObject);

		}
		if (keys.Contains(col.gameObject)){
			objects[col.gameObject].lastSeenPosition = col.gameObject.transform.position;
		}
	}
	void React(GameObject g){
		if (g.name == "pimp_hat"){
			SendMessage("Say","That's a sweet hat!",SendMessageOptions.DontRequireReceiver);
		}
	}
}
