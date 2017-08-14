using UnityEngine;
// using System.Collections;
using System.Collections.Generic;

public class Fire : MonoBehaviour {
	public Flammable flammable;
	private Dictionary<GameObject, Flammable> flammables = new Dictionary<GameObject, Flammable>();
	private MessageDamage message;
	private HashSet<GameObject> damageQueue = new HashSet<GameObject>();
	private float damageTimer;
	static List<string> forbiddenTags = new List<string>(new string[]{"fire", "occurrenceFlag", "background", "sightcone"});
	void Start(){
		message = new MessageDamage(1f, damageType.fire);
	}
	void Update(){
		damageTimer += Time.deltaTime;
		if (damageTimer > 0.2f){
			damageTimer = 0;
			if (flammable)
				message.responsibleParty = flammable.responsibleParty;
			foreach(GameObject obj in damageQueue){
				if (obj != null){
					Toolbox.Instance.SendMessage(obj, this, message);
					Debug.Log("fire damaging "+obj.name);
				}
			}
			damageQueue = new HashSet<GameObject>();
		}
	}
	void OnTriggerEnter2D(Collider2D coll){
		if (!flammables.ContainsKey(coll.gameObject)){
			Flammable flam = coll.GetComponentInParent<Flammable>();
			if (flam != null && flam != flammable){
				flammables.Add(coll.gameObject, flam);
			}
		}
	}
	
	void OnTriggerStay2D(Collider2D coll){
		if (!flammable.onFire)
			return;
		if (forbiddenTags.Contains(coll.tag))
			return;
		damageQueue.Add(coll.gameObject);
		if (flammables.ContainsKey(coll.gameObject)){
			flammables[coll.gameObject].heat += Time.deltaTime * 2f;
			if (flammable.responsibleParty != null){
				flammables[coll.gameObject].responsibleParty = flammable.responsibleParty;
			}
		}
	}
}
