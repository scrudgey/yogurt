using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airplane : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Sprite openSprite;
    public Sprite closedSprite;
    public AudioSource audioSource;
    public AudioClip doorSound;
    void Start() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        spriteRenderer.sprite = openSprite;
    }
    void OnTriggerEnter2D(Collider2D other) {
        if (!InputController.forbiddenTags.Contains(other.tag))
            CloseDoor(other.gameObject);
    }
    void CloseDoor(GameObject other) {
        audioSource.PlayOneShot(doorSound);
        spriteRenderer.sprite = closedSprite;
        Destroy(other);
    }
}
