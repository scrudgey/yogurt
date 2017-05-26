﻿using UnityEngine;
using System.Collections.Generic;
using AI;

[System.Serializable]
public class Knowledge{
	public GameObject obj;
	public Transform transform;
	public Vector3 lastSeenPosition;
	public float lastSeenTime;

	public Flammable flammable;
	public MeleeWeapon meleeWeapon;
	public Knowledge(GameObject o){
		this.obj = o;
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
	public Knowledge knowledge;
	public bool unconscious;
	public PersonalAssessment(Knowledge k){
		knowledge = k;
	}
}

public class Awareness : MonoBehaviour, IMessagable {
	public List<GameObject> initialAwareness;
	public GameObject possession;
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
	void Start () {
		control = gameObject.GetComponent<Controllable>();
		sightCone = Instantiate( Resources.Load("sightcone1"), gameObject.transform.position, Quaternion.identity ) as GameObject;
		sightConeScale = sightCone.transform.localScale;
		sightConeTransform = sightCone.transform;
		sightConeTransform.parent = transform;
		if (initialAwareness.Count > 0){
			fieldOfView = initialAwareness;
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
			Vector3 directionToTarget = assessment.knowledge.lastSeenPosition - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
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
	// process the list of objects in the field of view.
	void Perceive(){
		viewed = false;
		speciousPresent = 1f;
		foreach (GameObject g in fieldOfView){
			if (g == null)
				continue;
			Knowledge knowledge = null;
			if (knowledgebase.TryGetValue(g, out knowledge)){
				knowledge.UpdateInfo();
			} else {
				knowledge = new Knowledge(g);
				knowledgebase.Add(g, knowledge);
			}
			PersonalAssessment assessment = FormPersonalAssessment(g);
			Humanoid human = g.GetComponent<Humanoid>();
			if (human){
				// assessment.unconscious = human.hitstun;
				assessment.unconscious = human.hitState >= Controllable.HitState.stun;
			}
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
	void AttackedByPerson(GameObject g){
		PersonalAssessment assessment = FormPersonalAssessment(g);
		if (assessment != null){
			assessment.status = PersonalAssessment.friendStatus.enemy;
		}
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)incoming;
			hitState = hits.hitState;
			// if (hits.updateUnconscious)	
			// 	unconscious = hits.unconscious;
		}
		if (hitState >= Controllable.HitState.unconscious)
			return;
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			foreach (GameObject responsible in message.responsibleParty){
				AttackedByPerson(responsible);
			}
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
				fieldOfView.Add(message.holding);
				// Perceive();
			}
		}
	}
}
