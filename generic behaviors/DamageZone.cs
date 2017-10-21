using UnityEngine;
using System.Collections.Generic;

public class DamageZone : MonoBehaviour {
	public damageType type;
	public float amount;
	public MessageDamage message;
	public float timerInterval=0.5f;
	private Dictionary<GameObject, float> hitTimeouts = new Dictionary<GameObject, float>();
	void Start(){
		message = new MessageDamage(amount, type);
	}
	void OnTriggerStay2D(Collider2D other){
		float timerVal = 0;
		if (other.tag == "fire")
			return;
		if (hitTimeouts.TryGetValue(other.gameObject, out timerVal)){
			timerVal -= Time.deltaTime;
			if (timerVal <= 0){
				timerVal = timerInterval;
				Toolbox.Instance.SendMessage(other.gameObject, this, message, sendUpwards: true);
			}
			hitTimeouts[other.gameObject] = timerVal;
		} else {
			hitTimeouts[other.gameObject] = 0;
		}
	}
}
