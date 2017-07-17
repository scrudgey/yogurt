using UnityEngine;
using System.Collections.Generic;
using AI;

[System.Serializable]
public class Knowledge{
	public GameObject obj;
	public Transform transform;
	public Vector3 lastSeenPosition;
	public float lastSeenTime;
	public Flammable flammable;
	public Knowledge(){
		
	}
	public Knowledge(GameObject o){
		this.obj = o;
		transform = o.transform;
		lastSeenPosition = transform.position;
		lastSeenTime = Time.time;
		foreach(Component component in o.GetComponents<Component>()){
			if (component is Flammable){
				flammable = (Flammable)component;
			}
			// if (component is MeleeWeapon){
			// 	meleeWeapon = (MeleeWeapon)component;
			// }
		}
	}
	public void UpdateInfo(){
		if (obj){
			// Debug.Log(obj.name + " updating");
			lastSeenPosition = transform.position;
			lastSeenTime = Time.time;
		}
	}
}

[System.Serializable]
public class PersonalAssessment{
	public enum friendStatus{neutral, friend, enemy}
	public friendStatus status;
	public Knowledge knowledge;
	public bool unconscious;
	public PersonalAssessment(Knowledge k){
		knowledge = k;
	}
}

public class Awareness : MonoBehaviour, IMessagable {
	public List<GameObject> initialAwareness;
	public GameObject possession;
	public Knowledge possessionDefaultState;
	public GameObject wayWardPossession;
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
	public Controllable.HitState hitState;
	public Ref<GameObject> nearestEnemy = new Ref<GameObject>(null);
	public Ref<GameObject> nearestFire = new Ref<GameObject>(null);
	public SerializableDictionary<GameObject, Knowledge> knowledgebase = new SerializableDictionary<GameObject, Knowledge>();
	public SerializableDictionary<GameObject, PersonalAssessment> people = new SerializableDictionary<GameObject, PersonalAssessment>();
	// public DecisionMaker decisionMaker;
	void Start () {
		control = gameObject.GetComponent<Controllable>();
		sightCone = Instantiate( Resources.Load("sightcone1"), gameObject.transform.position, Quaternion.identity ) as GameObject;
		sightConeScale = sightCone.transform.localScale;
		sightConeTransform = sightCone.transform;
		sightConeTransform.parent = transform;
		if (initialAwareness.Count > 0){
			fieldOfView = initialAwareness;
			if (possession != null)
				initialAwareness.Add(possession);
			Perceive();
		}

	}
	public List<GameObject> FindObjectWithName(string targetName){
		List<GameObject> returnArray = new List<GameObject>();
		List<GameObject> removeArray = new List<GameObject>();
		foreach (Knowledge k in knowledgebase.Values){
			if (k.obj){
				if (k.obj.activeInHierarchy == false)
					continue;
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
		if (speciousPresent <= 0){
			if (fieldOfView.Count > 0 && viewed == true){
				Perceive();
			}
			SetNearestEnemy();
			SetNearestFire();
		}
	}

	void SetNearestEnemy(){
		nearestEnemy.val = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
		foreach (PersonalAssessment assessment in people.Values){
			if (assessment.status != PersonalAssessment.friendStatus.enemy)
				continue;
			if (assessment.unconscious)
				continue;
			// Vector3 directionToTarget = assessment.knowledge.lastSeenPosition - currentPosition;
			Vector3 directionToTarget = assessment.knowledge.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                nearestEnemy.val = assessment.knowledge.obj;
            }
		}
	}
	void SetNearestFire(){
		nearestFire.val = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
		foreach (Knowledge knowledge in knowledgebase.Values){
			if (knowledge.flammable == null)
				continue;
			if (knowledge.flammable.onFire){
				Vector3 directionToTarget = knowledge.lastSeenPosition - currentPosition;
				float dSqrToTarget = directionToTarget.sqrMagnitude;
				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					nearestFire.val = knowledge.obj;
				}
			}
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
	void OnTriggerEnter2D(Collider2D other){
		if (other.tag == "occurrenceFlag"){
			NoticeOccurrence(other.gameObject);
		}
	}
	void NoticeOccurrence(GameObject flag){
		Occurrence occurrence = flag.GetComponent<Occurrence>();
		if (occurrence == null)
			return;
		foreach (OccurrenceData od in occurrence.data){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageOccurrence(od));
			if (od is OccurrenceViolence){
				OccurrenceViolence dat = (OccurrenceViolence)od;
				PersonalAssessment attacker = FormPersonalAssessment(dat.attacker);
				PersonalAssessment victim = FormPersonalAssessment(dat.victim);
				if (attacker == null || victim == null){
					continue;
				}
				// Debug.Log(dat.attacker.name + " attacked "+ dat.victim.name);
				if (gameObject == attacker.knowledge.obj || gameObject == victim.knowledge.obj)
					continue;

				switch (attacker.status)
				{
					case PersonalAssessment.friendStatus.friend:
						victim.status = PersonalAssessment.friendStatus.enemy;
						// Debug.Log(name + " friend attacked friend");
					break;
					case PersonalAssessment.friendStatus.neutral:
						if (victim.status == PersonalAssessment.friendStatus.friend){
							attacker.status = PersonalAssessment.friendStatus.enemy;
							// Debug.Log(name + " neutral attacked friend");
						}
						if (victim.status == PersonalAssessment.friendStatus.neutral){
							attacker.status = PersonalAssessment.friendStatus.enemy;
							// Debug.Log(name + " neutral attacked neutral");
						}
						if (victim.status == PersonalAssessment.friendStatus.enemy){
							attacker.status = PersonalAssessment.friendStatus.friend;
							// Debug.Log(name + " neutral attacked enemy");
						}
					break;
					default:
					break;
				}
			}
		}
	}
	// process the list of objects in the field of view.
	void Perceive(){
		viewed = false;
		speciousPresent = 1f;
		foreach (GameObject g in fieldOfView){

			if (g == null)
				continue;
			Knowledge knowledge = null;
			if (g == possession){
				if (!knowledgebase.ContainsKey(g)){
					possessionDefaultState = new Knowledge(g);
					knowledge = new Knowledge(g);
					knowledgebase.Add(g, knowledge);
				}
			}
			if (knowledgebase.TryGetValue(g, out knowledge)){
				knowledge.UpdateInfo();
			} else {
				knowledge = new Knowledge(g);
				knowledgebase.Add(g, knowledge);
			}
			PersonalAssessment assessment = FormPersonalAssessment(g);
			Humanoid human = g.GetComponent<Humanoid>();
			if (human){
				assessment.unconscious = human.hitState >= Controllable.HitState.stun;
			}
		}
	}
	public bool PossessionsAreOkay(){
		if (possession == null)
			return true;
		Knowledge knowledge = null;
		if (!knowledgebase.TryGetValue(possession, out knowledge)){
			return true;
		}
		if (possessionDefaultState == null)
			return true;

		if (Time.time - knowledge.lastSeenTime > 2){
			// Debug.Log("possession not okay: timeout");
			return false;
		}
		if (Vector2.Distance(knowledge.lastSeenPosition, possessionDefaultState.lastSeenPosition) > 0.1){
			// Debug.Log("possession not okay: location");			
			return false;
		} else {
			return true;
		}
	}

	public PersonalAssessment FormPersonalAssessment(GameObject g, bool debug=false){
		if (g == null)
			return null;
		if (debug)
			Debug.Log("assess "+g.name+":");
		Controllable rootControllable = g.GetComponentInParent<Controllable>();
		if (rootControllable == null){
			if (debug)
				Debug.Log("no root controllable. quitting...");
			return null;
		}
		GameObject rootObject = rootControllable.gameObject;
		if (debug)
			Debug.Log("root object: "+rootObject.name);

		if (!knowledgebase.ContainsKey(rootObject))
			knowledgebase.Add(rootObject, new Knowledge(rootObject));
		PersonalAssessment storedAssessment;
		if (people.TryGetValue(rootObject, out storedAssessment)){
			return storedAssessment;
		}

		PersonalAssessment assessment = new PersonalAssessment(knowledgebase[rootObject]);
		people.Add(rootObject, assessment);
		return assessment;
	}
	void AttackedByPerson(GameObject g){
		PersonalAssessment assessment = FormPersonalAssessment(g);
		if (assessment != null){
			if (assessment.status != PersonalAssessment.friendStatus.friend){
				assessment.status = PersonalAssessment.friendStatus.enemy;
			}
			assessment.knowledge.lastSeenPosition = g.transform.position;
		}
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)incoming;
			hitState = hits.hitState;
		}
		if (hitState >= Controllable.HitState.unconscious)
			return;
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			AttackedByPerson(message.responsibleParty);
		}
		if (incoming is MessageInsult){
			PersonalAssessment assessment = FormPersonalAssessment(incoming.messenger.gameObject);
			assessment.status = PersonalAssessment.friendStatus.enemy;
		}
		if (incoming is MessageThreaten){
			PersonalAssessment assessment = FormPersonalAssessment(incoming.messenger.gameObject);
			assessment.status = PersonalAssessment.friendStatus.enemy;
		}
		if (incoming is MessageInventoryChanged){
			MessageInventoryChanged message = (MessageInventoryChanged)incoming;
			if (message.holding != null){
				Knowledge knowledge = null;
				if (knowledgebase.TryGetValue(message.holding, out knowledge)){
					knowledge.UpdateInfo();
				} else {
					knowledge = new Knowledge(message.holding);
					knowledgebase.Add(message.holding, knowledge);
				}
			}
			if (message.dropped != null){
				Knowledge knowledge = null;
				if (knowledgebase.TryGetValue(message.dropped, out knowledge)){
					knowledge.UpdateInfo();
				} else {
					knowledge = new Knowledge(message.dropped);
					knowledgebase.Add(message.dropped, knowledge);
				}
			}
		}
	}
}
