using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class CosmicDamageEffect : MonoBehaviour {
    float timer = -1f;
    Vector2 target;
    Vector2 initial;
    public float lifetime = 0.2f;
    public void Emit(Vector2 direction) {
        timer = 0f;
        target = direction.normalized / 4f;
        initial = transform.position;
        ParticleSystem particles = GetComponent<ParticleSystem>();
        particles.Play();
    }
    void Update() {
        if (timer == -1f)
            return;
        if (timer < lifetime) {
            timer += Time.deltaTime;
            float x = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initial.x, target.x, lifetime);
            float y = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initial.y, target.y, lifetime);
            transform.position = new Vector3(x, y, transform.position.z);
        } else {

            Destroy(gameObject);
        }
    }
}
