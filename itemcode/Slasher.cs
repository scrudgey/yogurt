using UnityEngine;
// using System.Collections;

public class Slasher : MonoBehaviour {
	public AudioClip[] impactSounds;
	public Vector2 direction; 
	
	void SlashOn(){
		GetComponent<Collider2D>().enabled = true;
	}

	void SlashOff(){
		GetComponent<Collider2D>().enabled = false;
	}

	void OnTriggerStay2D(Collider2D collider){
		if (collider.gameObject.tag == "Physical"){
			Toolbox.Instance.DataFlag(gameObject, 70f, 0f, 0f, 0f, 0f);
			Physical phys = collider.gameObject.GetComponentInParent<Physical>();
			if (phys){
				if(phys.currentMode == Physical.mode.ground){
					Vector2 force = direction;
					// this is the important aspect that will need changing in order to apply forces correctly.
					phys.Impact(force);
				}
			}
		}
		LiquidContainer container = collider.GetComponent<LiquidContainer>();
		if (container){
			container.Spill();
		}
		float damage = direction.magnitude * 20f;
		MessageDamage message = new MessageDamage(damage, damageType.physical);
		Toolbox.Instance.SendMessage(collider.gameObject, this, message);
	}

	void OnTriggerEnter2D(Collider2D collider){
		if (collider.gameObject.tag == "Physical" && impactSounds.Length > 0){
			GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0,impactSounds.Length)]);
		}
	}

	void SlashEnd(){
		Destroy(gameObject);
	}
}
