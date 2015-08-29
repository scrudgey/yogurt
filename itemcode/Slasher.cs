using UnityEngine;
using System.Collections;

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
			Physical phys = collider.gameObject.GetComponentInParent<Physical>();
			if (phys){
				if(phys.currentMode == Physical.mode.ground){
//					Vector2 force = 100*Vector2.up + 250 * Vector2.right * transform.lossyScale.x;
//					force = -100*Vector2.up;
					Vector2 force = direction * 1500;
					// this is the important aspect that will need changing in order to apply forces correctly.
					phys.Impact(force);
				}
			}

		}
		
		LiquidContainer container = collider.GetComponent<LiquidContainer>();
		if (container){
			container.Spill();
		}
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
