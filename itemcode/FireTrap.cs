using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour {
    public bool on;
    public ParticleSystem particles;
    public DamageZone zone;
    public AudioSource source;
    public AudioClip clip;
    public float timer;
    public float onTime;
    public float offTime;

    void Start() {
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
        source.clip = clip;
        source.loop = true;
    }
    void Update() {
        timer += Time.deltaTime;
        if (on && timer > onTime) {
            on = !on;
            timer = 0;
            particles.Stop();
            zone.enabled = false;
            source.clip = clip;
            source.loop = true;
            source.Stop();
        } else if (!on && timer > offTime) {
            on = !on;
            timer = 0;
            particles.Play();
            zone.enabled = true;
            source.clip = clip;
            source.loop = true;
            source.Play();
        }
    }
}
