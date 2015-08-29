using UnityEngine;
using System.Collections;

public class PlaySoundOnGroundImpact : MonoBehaviour {

	public AudioClip[] groundImpactSounds;

	void Start () {
		if (groundImpactSounds.Length> 0){
			if (!GetComponent<AudioSource>()){
				gameObject.AddComponent<AudioSource>();
			}
			GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Logarithmic;
			GetComponent<AudioSource>().minDistance = 0.4f;
			GetComponent<AudioSource>().maxDistance = 5.42f;
		}
	}

	public void OnGroundImpact(Physical phys){
		if (groundImpactSounds.Length > 0){
			GameObject speaker = Instantiate(Resources.Load("Speaker"),transform.position,Quaternion.identity) as GameObject;
			speaker.GetComponent<AudioSource>().clip = groundImpactSounds[Random.Range(0,groundImpactSounds.Length)];
			speaker.GetComponent<AudioSource>().Play();
		}
	}

}
