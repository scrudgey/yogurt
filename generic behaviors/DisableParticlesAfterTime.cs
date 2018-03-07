using UnityEngine;

public class DisableParticlesAfterTime : MonoBehaviour {
    public ParticleSystem[] particles;
    public float lifetime;
    public float time;
    void Start() {
        particles = GetComponentsInChildren<ParticleSystem>();
        if (particles == null) {
            Destroy(this);
        }
    }
    void Update() {
        time += Time.deltaTime;
        if (time > lifetime) {
            foreach (ParticleSystem particle in particles)
                particle.Stop();
            this.enabled = false;
        }
    }
}
