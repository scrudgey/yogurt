using UnityEngine;
public class Flammable : MonoBehaviour {
	public float heat;
	public float flashpoint;
	public bool onFire;
	public bool fireSource;
	public ParticleSystem smoke;
	public ParticleSystem fireParticles;
	private CircleCollider2D fireRadius;
	private AudioClip[] igniteSounds = new AudioClip[2];
	private AudioClip burnSounds;
	private AudioSource audioSource;
	private float flagTimer;
	void Start () {
		//ensure that there is a speaker
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		burnSounds = Resources.Load("sounds/Crackling Fire", typeof(AudioClip)) as AudioClip;
		igniteSounds[0] = Resources.Load("sounds/Flash Fire Ignite 01", typeof(AudioClip)) as AudioClip;
		igniteSounds[1] = Resources.Load("sounds/Flash Fire Ignite 02", typeof(AudioClip)) as AudioClip;

		//add the particle effect and set its position
		Vector3 flamePosition = transform.position;
		Transform flamepoint = transform.Find("flamepoint");
		if (flamepoint != null){
			flamePosition = flamepoint.position;
		}
		GameObject thing = Instantiate(Resources.Load("particles/smoke"), flamePosition, Quaternion.identity) as GameObject;
		smoke = thing.GetComponent<ParticleSystem>();
		smoke.transform.parent = transform;
		thing = Instantiate(Resources.Load("particles/fire"), flamePosition, Quaternion.identity) as GameObject;
		fireParticles = thing.GetComponent<ParticleSystem>();
		fireParticles.transform.parent = transform;

		//add the fire object - can pare this down probably
		GameObject fireChild = new GameObject();
		fireChild.transform.position = transform.position;
		fireChild.transform.parent = transform;
		fireChild.tag = "fire";
		Rigidbody2D fireBody = fireChild.AddComponent<Rigidbody2D>();
		fireBody.isKinematic = true;
		fireBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
		Fire fire = fireChild.AddComponent<Fire>();
		fire.flammable = this;
		fireRadius = fireChild.AddComponent<CircleCollider2D>();
		fireRadius.isTrigger = true;
		fireRadius.radius = 0.2f;
		fireRadius.name = "fire";
		fire.gameObject.layer = 13;
	}

	void Update () {
		if (fireSource){
			onFire = true;
			heat = 100;
		}
		if (heat > -2 && !onFire){
			heat -= Time.deltaTime;
		}
		if (heat < -3){
			heat += Time.deltaTime;
		}
		if (!onFire && fireParticles.isPlaying){
			fireParticles.Stop();
			GetComponent<AudioSource>().Stop();
		}
		if (heat <= -2 && smoke.isPlaying){
			smoke.Stop();
		}
		if (heat > 1 && smoke.isStopped){
			smoke.Play();
		}
		if (heat > flashpoint && fireParticles.isStopped){
			fireParticles.Play();
			onFire = true;
			audioSource.PlayOneShot(igniteSounds[Random.Range(0, 1)]);
			audioSource.loop = true;
			audioSource.clip = burnSounds;
			audioSource.Play();

			OccurrenceFire fireData = new OccurrenceFire();
			fireData.objectName = Toolbox.Instance.CloneRemover(name);
			fireData.chaos = 10;
			Toolbox.Instance.OccurenceFlag(gameObject, fireData);
		}
		if (onFire){
			flagTimer += Time.deltaTime;
			if (flagTimer > 0.5f){
				flagTimer = 0;
				OccurrenceFire fireData = new OccurrenceFire();
				fireData.objectName = Toolbox.Instance.CloneRemover(name);
				fireData.chaos = 100;
				Toolbox.Instance.OccurenceFlag(gameObject, fireData);
			}
			MessageDamage message = new MessageDamage(Time.deltaTime, damageType.fire);
			Toolbox.Instance.SendMessage(gameObject, this, message, sendUpwards: false);
			if (Random.Range(0, 100f) < 1){
				MessageSpeech speechMessage = new MessageSpeech();
				speechMessage.phrase = "hot!";
				Toolbox.Instance.SendMessage(gameObject, this, speechMessage, sendUpwards: true);
			}
		}
	}

}
