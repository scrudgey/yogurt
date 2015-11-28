using UnityEngine;
using System.Collections.Generic;

public class Toilet : Container {

	private AudioSource audioSource;
	public AudioClip flushSound;
	private float timeout;
	public float refractoryPeriod;
	// Use this for initialization
	protected override void Start () {
		base.Start();
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		Interaction flushAct = new Interaction(this, "Flush", "Flush");
		interactions.Add(flushAct);
	}
	
	public void Flush(){
		if (timeout > 0)
			return;
		if (flushSound)
			audioSource.PlayOneShot(flushSound);
		
		for (int i = 0; i < items.Count; i++){
		// foreach (Pickup item in items){
			Pickup target = items[i];
			Dump(target);
			Destroy(target.gameObject);
		}
		items = new List<Pickup>();
		timeout = refractoryPeriod;
	}
	
	void Update(){
		if (timeout > 0)
			timeout -= Time.deltaTime;
	}
}
