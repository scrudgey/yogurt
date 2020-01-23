using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchbladeEffect : MonoBehaviour {
    public List<Sprite> frames;
    public AudioClip openSound;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Pickup pickup;
    private float timer;
    public float switchTime;
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickup = GetComponent<Pickup>();
        Reset();
    }
    void OnEnable() {
        Reset();
    }
    void Reset() {
        timer = 0f;
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        spriteRenderer.sprite = frames[0];
    }
    void Update() {
        if (pickup.holder == null)
            return;
        if (timer <= switchTime) {
            timer += Time.deltaTime;
            if (timer > switchTime) {
                audioSource.PlayOneShot(openSound);
                spriteRenderer.sprite = frames[1];
            }
        }
    }
}
