using UnityEngine;

public class CosmicNullifier : Pickup {
	public Interaction nullify;
	public AudioSource audioSource;
	public AudioClip[] nullifySound;
	public GameObject nullifyParticleEffect;
	void Start(){
		nullify = new Interaction(this, "Nullify", "Nullify");
		nullify.limitless = true;
		nullify.validationFunction = true;
		// nullify.inertOnPlayerConsent = false;
		// nullify.otherOnPlayerConsent = false;
		interactions.Add(nullify);
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	public void Nullify(Duplicatable duplicatable){
		if (duplicatable == null)
			return;
		duplicatable.Nullify();
		Destroy(gameObject);
		ClaimsManager.Instance.WasDestroyed(gameObject);
	}
	public bool Nullify_Validation(Duplicatable duplicatable){
		if (duplicatable.gameObject == null)
			return false;
		if (duplicatable.gameObject == gameObject){
			return false;
		} else {
			return duplicatable.Nullifiable();
		}
	}
	public string Nullify_desc(Duplicatable duplicatable){
		return "Nullify "+Toolbox.Instance.GetName(duplicatable.gameObject);
	}
}
