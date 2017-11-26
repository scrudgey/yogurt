// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class HypnosisEffect : MonoBehaviour {
	// code for directing the hypnosis direction and lifetime toward a target
	public GameObject target;
	public ParticleSystem psystem;
	float totalTime = 3f;
	void Awake(){
		psystem = GetComponent<ParticleSystem>();
	}
	void LateUpdate(){
		if (target == null){
			Destroy(gameObject);
			return;
		}
		totalTime -= Time.deltaTime;
		if (totalTime < 0){
			if (psystem.isEmitting)
				psystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
		}
		float distance = Vector2.Distance(transform.position, target.transform.position);
		ParticleSystem.MainModule main = psystem.main;
		float lifetime = distance / main.startSpeed.constant;
		main.startLifetime = lifetime;

		Quaternion myRot = transform.rotation;
		Vector2 d = target.transform.position - transform.position;
		psystem.transform.rotation = Quaternion.AngleAxis(Toolbox.Instance.ProperAngle(d.x, d.y)-90f, new Vector3(0, 0, 1));

		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];
		int n = psystem.GetParticles(particles);
		for(int i = 0; i<n; i++){
			float deltaTime = lifetime - particles[i].startLifetime;
			// if (i == 0)
			// 	Debug.Log(particles[i].remainingLifetime);
			// Debug.Log(particles[i].remainingLifetime);
			particles[i].startLifetime = lifetime;
			// Debug.Log(particles[i].remainingLifetime);
			// Debug.Log(particles[i].startLifetime.ToString()+" "+lifetime.ToString());
			// Debug.Log(startLifetime);
			particles[i].remainingLifetime += deltaTime;
		}
		psystem.SetParticles(particles, n);
		if (totalTime < 0 && n == 0){
			Destroy(gameObject);
		}
	}
}
