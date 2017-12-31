using UnityEngine;

public class CosmicNullifier : Pickup {
	public Interaction nullify;
	public AudioSource audioSource;
	public AudioClip[] nullifySound;
	public GameObject nullifyParticleEffect;
	public bool LoadInitialized = false;
	void Start () {
		if (!LoadInitialized)
			LoadInit();
	}
	public void LoadInit(){
		LoadInitialized = true;
		// if (nullify == null){
		nullify = new Interaction(this, "Nullify", "Nullify");
		nullify.limitless = true;
		nullify.duplicator = true;
		nullify.validationFunction = true;
		nullify.inertOnPlayerConsent = false;
		nullify.otherOnPlayerConsent = false;
		interactions.Add(nullify);
		// }
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	
	public void Nullify(){
		if (nullifySound.Length > 0){
			Toolbox.Instance.AudioSpeaker(nullifySound[Random.Range(0, nullifySound.Length)], nullify.duplicationTarget.transform.position);
		}
		if (nullifyParticleEffect != null){
			Instantiate(nullifyParticleEffect, nullify.duplicationTarget.transform.position, Quaternion.identity);
		}
		Destroy(nullify.duplicationTarget);
		ClaimsManager.Instance.WasDestroyed(gameObject);
		Destroy(gameObject);
	}
	public bool Nullify_Validation(){
		GameObject target = nullify.duplicationTarget;
		if (target == null)
			return false;
		if (target.gameObject == gameObject){
			return false;
		} else {
			return true;
		}
	}
	public string Nullify_desc(){
		return "Nullify "+Toolbox.Instance.GetName(nullify.duplicationTarget);
	}
}
