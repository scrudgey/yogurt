using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursedHandkerchief : MonoBehaviour {
    public Pickup pickup;
    // public Explosive explosive;
    public SpriteRenderer spriteRenderer;
    public GameObject ray;
    public AudioClip shootSound;
    public Sprite heldSprite;
    public Sprite groundSprite;
    float timer;
    public float rayInterval;
    void Start() {
        // explosive.enabled = true;
        pickup = GetComponent<Pickup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (pickup.holder != null) {
            spriteRenderer.sprite = heldSprite;
        } else {
            spriteRenderer.sprite = groundSprite;
        }

        timer += Time.deltaTime;
        if (timer > rayInterval) {
            timer = 0;
            if (Random.Range(0, 100) < 1) {
                TriggerCurse();
            }
        }
    }

    void TriggerCurse() {
        // instantiate ray far above heading down
        Vector2 position = transform.position;
        position.y += 1f;
        GameObject dartObj = Instantiate(ray, position, Quaternion.identity);
        Rigidbody2D dartBody = dartObj.GetComponent<Rigidbody2D>();
        // Vector2 velocity = (target.transform.position - transform.position + Random.onUnitSphere * 0.2f) * raySpeed;
        dartBody.velocity = Vector2.down * 5f;

        Collider2D myCollider = GetComponent<Collider2D>();

        // disable all collisions between the ray and environment
        foreach (Collider2D dartCollider in dartObj.GetComponentsInChildren<Collider2D>()) {
            foreach (Collider2D otherCollider in GameObject.FindObjectsOfType<Collider2D>()) {
                if (otherCollider != myCollider && otherCollider.transform.root != transform.root)
                    Physics2D.IgnoreCollision(dartCollider, otherCollider, true);
            }
        }

        // play sound
        Toolbox.Instance.AudioSpeaker(shootSound, transform.position);
    }
}
