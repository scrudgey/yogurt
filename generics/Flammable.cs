using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

public class Flammable : MonoBehaviour {

	public float heat;
	public float flashpoint;
	public bool onFire;
	public ParticleSystem smoke;
	public ParticleSystem fireParticles;
	private CircleCollider2D fireRadius;
	private AudioClip[] igniteSounds = new AudioClip[2];
	private AudioClip burnSounds;
	private Qualities quality;

	void Start () {
		//ensure that there is a speaker
		Toolbox.Instance.SetUpAudioSource(gameObject);
		burnSounds = Resources.Load("sounds/Crackling Fire", typeof(AudioClip)) as AudioClip;
		igniteSounds[0] = Resources.Load("sounds/Flash Fire Ignite 01", typeof(AudioClip)) as AudioClip;
		igniteSounds[1] = Resources.Load("sounds/Flash Fire Ignite 02", typeof(AudioClip)) as AudioClip;

		//add the particle effect and set its position
		GameObject thing = Instantiate(Resources.Load("particles/smoke"), transform.position, Quaternion.identity) as GameObject;
		smoke = thing.GetComponent<ParticleSystem>();
		smoke.transform.parent = transform;
		thing = Instantiate(Resources.Load("particles/fire"), transform.position, Quaternion.identity) as GameObject;
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
		quality = Toolbox.Instance.GetQuality(gameObject);
		}

	void Update () {
		if (heat > -2 && !onFire){
			heat -= Time.deltaTime;
		}
		if (heat < -3){
			heat += Time.deltaTime;
		}
		if (!onFire && fireParticles.isPlaying){
			fireParticles.Stop();
			GetComponent<AudioSource>().Stop();
			quality.quality.flaming = false;
		}
		if (heat <= -2 && smoke.isPlaying){
			smoke.Stop();
			quality.quality.flaming = false;
		}
		if (heat > 1 && smoke.isStopped){
			smoke.Play();
		}
		if (heat > flashpoint && fireParticles.isStopped){
			quality.quality.flaming = true;
			fireParticles.Play();
			onFire = true;
			GetComponent<AudioSource>().PlayOneShot(igniteSounds[Random.Range(0, 1)]);
			GetComponent<AudioSource>().loop=true;
			GetComponent<AudioSource>().PlayOneShot(burnSounds);
		}
		if (onFire){
			MessageDamage message = new MessageDamage(Time.deltaTime, damageType.fire);
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
	}

}
