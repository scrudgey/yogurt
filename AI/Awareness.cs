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
	public Vector3 lastSeenTime;
	public Knowledge(GameObject o){
		obj = o;
		Qualities q = o.GetComponent<Qualities>();
		if (q)
			quality = q.quality;
		lastSeenPosition = o.transform.position;
	}
}

public class PersonalAssessment{
	public enum friendStatus{neutral, friend, enemy}
	public friendStatus status;
	public float harmDealt;
}

public class Awareness : MonoBehaviour, IMessagable {
	private GameObject sightCone;
	private Vector3 sightConeScale;
	private Controllable control;
	private float speciousPresent; 
	private List<GameObject> fieldOfView = new List<GameObject>();
	private bool viewed;
	public Dictionary<GameObject, Knowledge> objects = new Dictionary<GameObject, Knowledge>();
	public Dictionary<GameObject, PersonalAssessment> people = new Dictionary<GameObject, PersonalAssessment>();
	public EntityController controller;
	public DecisionMaker decisionMaker;

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
			if (fieldOfView.Contains(gameObject))
				return;
			
			if (other.tag == "Physical")
				fieldOfView.Add(other.gameObject);

			if (other.gameObject.GetComponent<Controllable>()){
				fieldOfView.Add(other.gameObject);
			}
		}
	}
	// process the list of objects in the field of view.
	void Perceive(){
		viewed = false;
		speciousPresent = 1f;
		foreach (GameObject g in fieldOfView){
			// Qualities q = Toolbox.Instance.GetQuality(g);
			Qualities q = Toolbox.Instance.GetOrCreateComponent<Qualities>(g);
			Quality quality = q.quality;
			if (quality.flaming && controller.priority.name != "extinguish"){
				controller.priority.ExtinguishObject(g);
				controller.CheckPriority();
			}
			PersonalAssessment assessment = FormPersonalAssessment(g);
			if (assessment != null){
				// check to see if person is enemy
				if (assessment.status == PersonalAssessment.friendStatus.enemy){
					Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("You are mine enemy!"));
				}
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
			// SendMessage("Say", "That's a sweet hat!", SendMessageOptions.DontRequireReceiver);
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Sweet hat!"));
		}
	}

	PersonalAssessment FormPersonalAssessment(GameObject g){
		if (!g.GetComponent<Controllable>()){
			Debug.Log(g.name+" is not controllable");
			return null;
		}

		PersonalAssessment storedAssessment;
		if (people.TryGetValue(g, out storedAssessment)){
			return storedAssessment;
		}
		PersonalAssessment assessment = new PersonalAssessment();
		people.Add(g, assessment);
		return assessment;
	}

	void AttackedByPerson(GameObject g){
		PersonalAssessment assessment = FormPersonalAssessment(g);
		if (assessment != null){
			assessment.status = PersonalAssessment.friendStatus.enemy;
		}
	}

	public void ReceiveMessage(Message incoming){
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			foreach (GameObject responsible in message.responsibleParty){
				AttackedByPerson(responsible);
			}
		}
	}
}
