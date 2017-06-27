using UnityEngine;

public class Duplicator : Interactive, IDirectable {
	public AudioClip dupSound;
	public AudioClip failSound;
	private AudioSource audioSource;
	public ParticleSystem particles;
	// private Vector2 direction = Vector2.right;
	public void DirectionChange(Vector2 d){
		// direction = d;
		if (particles){
			// particles.transform.Rotate(new Vector3(0,0,1), Toolbox.Instance.ProperAngle(d.x, d.y) * 6.28f/360f);
			particles.transform.rotation = Quaternion.AngleAxis(Toolbox.Instance.ProperAngle(d.x, d.y)-20f, new Vector3(0, 0, 1));
		}
	}
	void Start () {
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		Interaction dup = new Interaction(this, "Duplicate", "Duplicate");
		dup.limitless = true;
		dup.validationFunction = true;
		interactions.Add(dup);
	}
	public void Duplicate(Pickup target){
		string prefabName = Toolbox.Instance.CloneRemover(target.name);
		GameObject dup = Instantiate(Resources.Load("prefabs/"+prefabName), target.transform.position, Quaternion.identity) as GameObject;
		if (dup == null){
			audioSource.PlayOneShot(failSound);
		} else {
			audioSource.PlayOneShot(dupSound);
			Instantiate(Resources.Load("particles/poof"), dup.transform.position, Quaternion.identity);
			if (prefabName == "dollar"){
				GameManager.Instance.data.achievementStats.dollarsDuplicated += 1;
				GameManager.Instance.CheckAchievements();
			}
			if (particles != null){
				particles.Play();
			}
		}
	}
	public bool Duplicate_Validation(Pickup target){
		if (target.gameObject == gameObject){
			return false;
		} else {
			return true;
		}
	}
	public string Duplicate_desc(Pickup target){
		return "Duplicate "+Toolbox.Instance.GetName(target.gameObject);
	}
}
