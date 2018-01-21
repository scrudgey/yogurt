using UnityEngine;
using System.Collections.Generic;

public class DamageZone : MonoBehaviour {
	public damageType type;
	public float amount;
	public MessageDamage message;
	public float timerInterval=0.5f;
	private Dictionary<GameObject, float> hitTimeouts = new Dictionary<GameObject, float>();
	public AudioClip damageSound;
	public AudioSource audioSource;
	void Start(){
		message = new MessageDamage(amount, type);
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		audioSource.clip = damageSound;
		// message.impactSounds = damageSound;
	}
	void OnTriggerStay2D(Collider2D other){
		float timerVal = 0;
		if (other.tag == "fire")
			return;
		if (hitTimeouts.TryGetValue(other.gameObject, out timerVal)){
			timerVal -= Time.deltaTime;
			if (timerVal <= 0){
				timerVal = timerInterval;
				message.responsibleParty = gameObject;
				Toolbox.Instance.SendMessage(other.gameObject, this, message, sendUpwards: true);
			}
			hitTimeouts[other.gameObject] = timerVal;
		} else {
			hitTimeouts[other.gameObject] = 0;
		}
	}
	void OnTriggerExit2D(){
	}
	public void ImpactReceived(ImpactResult result){
		if (result == ImpactResult.normal){
			if (!audioSource.isPlaying && audioSource.clip != null){
				audioSource.Play();
			}
		}
	}
}
