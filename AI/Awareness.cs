using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Knowledge{
	public GameObject obj;
	public Transform transform;
	public Vector3 lastSeenPosition;
	public float lastSeenTime;

	public Flammable flammable;
	public MeleeWeapon meleeWeapon;

	public Knowledge(GameObject o){
		obj = o;
		transform = o.transform;

		lastSeenPosition = transform.position;
		lastSeenTime = Time.time;
		foreach(Component component in o.GetComponents<Component>()){
			if (component is Flammable){
				flammable = (Flammable)component;
			}
			if (component is MeleeWeapon){
				meleeWeapon = (MeleeWeapon)component;
			}
		}
	}
	public void UpdateInfo(){
		if (obj){
			lastSeenPosition = transform.position;
			lastSeenTime = Time.time;
		}
	}
}

[System.Serializable]
public class PersonalAssessment{
	public enum friendStatus{neutral, friend, enemy}
	public friendStatus status;
	public float harmDealt;
	public Knowledge knowledge;
	public PersonalAssessment(Knowledge k){
		knowledge = k;
	}
}

public class Awareness : MonoBehaviour, IMessagable {
	private GameObject sightCone;
	Transform cachedTransform;
	public new Transform transform
		{
			get
			{
				if( cachedTransform == null )
				{
					cachedTransform = gameObject.GetComponent<Transform>();
				}
				return cachedTransform;
			}
	}
	public Transform sightConeTransform;
	private Vector3 sightConeScale;
	private Controllable control;
	private float speciousPresent; 
	private List<GameObject> fieldOfView = new List<GameObject>();
	private bool viewed;
	public SerializableDictionary<GameObject, Knowledge> knowledgebase = new SerializableDictionary<GameObject, Knowledge>();
	public SerializableDictionary<GameObject, PersonalAssessment> people = new SerializableDictionary<GameObject, PersonalAssessment>();
	void Start () {
		control = gameObject.GetComponent<Controllable>();
		sightCone = Instantiate( Resources.Load("sightcone1"), gameObject.transform.position, Quaternion.identity ) as GameObject;
		sightConeScale = sightCone.transform.localScale;
		sightConeTransform = sightCone.transform;
		sightConeTransform.parent = transform;
	}
	public List<GameObject> FindObjectWithName(string targetName){
		List<GameObject> returnArray = new List<GameObject>();
		List<GameObject> removeArray = new List<GameObject>();
		foreach (Knowledge k in knowledgebase.Values){
			if (k.obj){
				if (k.obj.name == targetName)
					returnArray.Add(k.obj);
			} else {
				removeArray.Add(k.obj);
			}
		}
		foreach(GameObject g in removeArray){
			knowledgebase.Remove(g);
		}
		return returnArray;
	}
	
	void Update () {
		// update sight cone rotation and scale -- point it in the right direction.
		if (transform.localScale.x < 0 && sightConeTransform.localScale.x > 0){
			Vector3 tempscale = sightConeScale;
			tempscale.x = -1 * sightConeScale.x;
			sightConeTransform.localScale = tempscale;
		} 
		if (transform.localScale.x > 0 && sightConeTransform.localScale.x < 0){
			sightConeTransform.localScale = sightConeScale;
		}
		float rot_z = Mathf.Atan2(control.direction.y, control.direction.x) * Mathf.Rad2Deg;
		sightConeTransform.rotation = Quaternion.Euler(0f, 0f, rot_z );
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
			if (fieldOfView.Contains(other.gameObject))
				return;
			// might be able to have better logic for how to add things to the field of view here. I need 
			// "high level" objects of import.
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
			Knowledge knowledge = null;
			if (knowledgebase.TryGetValue(g, out knowledge)){
				knowledge.UpdateInfo();
			} else {
				knowledge = new Knowledge(g);
				knowledgebase.Add(g, knowledge);
			}
			FormPersonalAssessment(g);
			// React(g, knowledge);
		}
	}

	public PersonalAssessment FormPersonalAssessment(GameObject g){
		if (!knowledgebase.ContainsKey(g))
			knowledgebase.Add(g, new Knowledge(g));
		PersonalAssessment storedAssessment;
		if (people.TryGetValue(g, out storedAssessment)){
			return storedAssessment;
		}
		if (!g.GetComponent<Controllable>()){
			return null;
		}
		PersonalAssessment assessment = new PersonalAssessment(knowledgebase[g]);
		people.Add(g, assessment);
		return assessment;
	}
	public GameObject nearestEnemy(){
		GameObject threat = null;
		foreach (PersonalAssessment assessment in people.Values){
			if (assessment.status == PersonalAssessment.friendStatus.enemy)
				return assessment.knowledge.obj;
		}
		return threat;
	}
	public GameObject nearestFire(){
		foreach (Knowledge knowledge in knowledgebase.Values){
			if (knowledge.flammable != null){
				if (knowledge.flammable.onFire)
					return knowledge.obj;
			}
		}
		return null;
	}
	void AttackedByPerson(GameObject g, MessageDamage message){
		PersonalAssessment assessment = FormPersonalAssessment(g);
		if (assessment != null){
			assessment.status = PersonalAssessment.friendStatus.enemy;
			assessment.harmDealt = message.amount;
		}
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			foreach (GameObject responsible in message.responsibleParty){
				AttackedByPerson(responsible, message);
			}
		}
	}

	public void Insulted(GameObject insulter, DialogueMenu menu){
		PersonalAssessment assessment = FormPersonalAssessment(insulter);
		assessment.status = PersonalAssessment.friendStatus.enemy;
		menu.Say(gameObject, "How dare you!");
	}
}
