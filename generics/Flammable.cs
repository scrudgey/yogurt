using UnityEngine;
public class Flammable : MonoBehaviour, ISaveable {
	public float heat;
	public float flashpoint;
	public bool onFire;
	public bool fireSource;
	public bool noDamage;
	public ParticleSystem smoke;
	public ParticleSystem fireParticles;
	private CircleCollider2D fireRadius;
	private AudioClip[] igniteSounds = new AudioClip[2];
	private AudioClip burnSounds;
	private AudioSource audioSource;
	private float flagTimer;
	public GameObject responsibleParty;
	public bool playSounds = true;
	public Pickup pickup;
	public float fireRetardantBuffer = 2f;
	void Start () {
		pickup = GetComponent<Pickup>();

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
		if (heat > (-1f * fireRetardantBuffer) && !onFire){
			heat -= Time.deltaTime;
		}
		if (heat < (-1f * fireRetardantBuffer - 1f)){
			heat += Time.deltaTime;
		}
		if (!onFire && fireParticles.isPlaying){
			fireParticles.Stop();
			audioSource.Stop();
		}
		if (heat <= (-1f * fireRetardantBuffer) && smoke.isPlaying){
			smoke.Stop();
		}
		if (heat > 1 && smoke.isStopped){
			smoke.Play();
		}
		if (heat > flashpoint && fireParticles.isStopped){
			fireParticles.Play();
			onFire = true;
			if (playSounds){
				audioSource.PlayOneShot(igniteSounds[Random.Range(0, 1)]);
				audioSource.loop = true;
				audioSource.clip = burnSounds;
				audioSource.Play();
			}
			OccurrenceFire fireData = new OccurrenceFire();
			fireData.flamingObject = gameObject;
			Toolbox.Instance.OccurenceFlag(gameObject, fireData);
		}
		if (onFire){
			if (pickup){
				if (pickup.holder != null)
					responsibleParty = pickup.holder.gameObject;
			}
			flagTimer += Time.deltaTime;
			if (flagTimer > 0.5f){
				flagTimer = 0;
				OccurrenceFire fireData = new OccurrenceFire();
				fireData.flamingObject = gameObject;
				Toolbox.Instance.OccurenceFlag(gameObject, fireData);
			}
			// if i am on fire, i take damage.
			MessageDamage message = new MessageDamage(Time.deltaTime, damageType.fire);
			Toolbox.Instance.SendMessage(gameObject, this, message, sendUpwards: false);
			if (Random.Range(0, 100f) < 1){
				MessageSpeech speechMessage = new MessageSpeech();
				speechMessage.phrase = "this "+gameObject.name+" is hot!";
				Toolbox.Instance.SendMessage(gameObject, this, speechMessage, sendUpwards: true);
			}
		}
	}
	public void SaveData(PersistentComponent data){
		data.floats["heat"] = heat;
		data.floats["flashpoint"] = flashpoint;
		data.bools["onFire"] = onFire;
	}
	public void LoadData(PersistentComponent data){
		heat = data.floats["heat"];
		flashpoint = data.floats["flashpoint"];
		onFire = data.bools["onFire"];
	}
}
