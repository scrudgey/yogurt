using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathRayLamp : MonoBehaviour {
    public GameObject ray;
    public AudioClip shootSound;
    public AudioClip humSound;
    public AudioSource audioSource;
    private HashSet<GameObject> damageQueue = new HashSet<GameObject>();
    public MessageDamage message;
    public Collider2D myCollider;
    public float raySpeed;
    public float rayConstant;
    public float rayInterval;
    private float timer;
    void Start() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    void OnTriggerEnter2D(Collider2D coll) {
        if (InputController.forbiddenTags.Contains(coll.tag))
            return;
        if (coll.gameObject.name.Contains(ray.name)) {
            return;
        }
        damageQueue.Add(coll.gameObject);
        // Debug.Log($"adding {coll.gameObject.transform.root}");
    }
    void OnTriggerExit2D(Collider2D collider) {
        if (InputController.forbiddenTags.Contains(collider.tag))
            return;
        if (damageQueue.Contains(collider.gameObject)) {
            damageQueue.Remove(collider.gameObject);
            // Debug.Log($"removing {collider.gameObject.transform.root}");
        }
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > rayInterval) {
            timer = 0f;
            if (damageQueue.Count == 0) {
                return;
            }
            foreach (GameObject target in damageQueue) {
                if (Random.Range(0, 1f) < rayConstant) {
                    if (!audioSource.isPlaying && shootSound != null) {
                        audioSource.PlayOneShot(shootSound);
                    }
                    GameObject dartObj = Instantiate(ray, transform.position, Quaternion.identity);
                    Rigidbody2D dartBody = dartObj.GetComponent<Rigidbody2D>();
                    Vector2 velocity = (target.transform.position - transform.position + Random.onUnitSphere * 0.2f) * raySpeed;
                    dartBody.velocity = velocity;
                    foreach (Collider2D dartCollider in dartObj.GetComponentsInChildren<Collider2D>()) {
                        Physics2D.IgnoreCollision(dartCollider, myCollider, true);
                    }
                }
            }
        }
    }

}
