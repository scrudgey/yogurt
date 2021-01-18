using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralStarSpawner : MonoBehaviour {
    public GameObject starPrefab;
    public float timer;
    public float lifetime;
    void Start() {
        for (int i = 0; i < 50; i++) {
            Spawn(bake: Random.Range(0f, 2f));
        }
    }
    public void Update() {
        timer += Time.deltaTime;
        if (Random.Range(0f, 1f) < timer) {
            Spawn();
            timer = 0;
        }
    }
    public void Spawn(float bake = 0f) {
        GameObject starObject = Instantiate(starPrefab);

        starObject.transform.position = Vector3.zero;
        Star star = starObject.GetComponent<Star>();
        star.size = Random.Range(1, 5);
        star.motion = Star.motionType.spiral;
        star.lifetime = lifetime;
        star.timer = bake;
    }
}
