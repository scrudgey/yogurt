using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {
    public AudioClip spawnSound;
    public SpriteRenderer spriteRenderer;
    public GameObject zombiePrefab;
    public GameObject dirtParticle;
    public float timer;
    enum State { start, hand }
    private State state;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "background" || col.tag == "sky")
            Destroy(gameObject);
    }
    void OnTriggerStay2D(Collider2D col) {
        if (col.tag == "background" || col.tag == "sky")
            Destroy(gameObject);
    }
    void Update() {
        timer += Time.deltaTime;
        switch (state) {
            default:
            case State.start:
                if (timer > 1f) {
                    state = State.hand;
                    timer = 0f;
                    Toolbox.Instance.AudioSpeaker(spawnSound, transform.position);
                    spriteRenderer.enabled = true;
                    GameObject dirt1 = GameObject.Instantiate(dirtParticle, transform.position, Quaternion.identity);
                    GameObject dirt2 = GameObject.Instantiate(dirtParticle, transform.position, Quaternion.identity);
                    Rigidbody2D dirt1Body = dirt1.GetComponent<Rigidbody2D>();
                    Rigidbody2D dirt2Body = dirt2.GetComponent<Rigidbody2D>();
                    dirt1Body.velocity = new Vector2(0.6f, 0.8f);
                    dirt2Body.velocity = new Vector2(-0.6f, 0.8f);
                }
                break;
            case State.hand:
                if (timer > 5f) {
                    Toolbox.Instance.AudioSpeaker(spawnSound, transform.position);
                    GameObject.Instantiate(zombiePrefab, transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }
    }
}
