// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class AnimateUIBubble : MonoBehaviour {
    public GameObject frame1;
    public GameObject frame2;
    private float animationTimer;
    AudioSource audioSource;
    public AudioClip flipSound;
    public float frameTime = 1f;
    private int frameIndex = 0;
    void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    void Update() {
        if (animationTimer < 0)
            return;
        animationTimer += Time.deltaTime;
        if (animationTimer > frameTime) {
            frameIndex += 1;
            if (frameIndex == 2) {
                frameIndex = 0;
                if (audioSource != null && flipSound != null)
                    audioSource.PlayOneShot(flipSound);
            }
            animationTimer = 0f;
            if (frameIndex == 0) {
                frame1.SetActive(true);
                frame2.SetActive(false);
            } else if (frameIndex == 1) {
                frame1.SetActive(false);
                frame2.SetActive(true);
            }
            // spriteRenderer.sprite = frames[frameIndex];
        }
    }
    public void EnableFrames() {
        Debug.Log("enable frames");
        animationTimer = 0;
    }
    public void DisableFrames() {
        frame1.SetActive(false);
        frame2.SetActive(false);
        animationTimer = -1;
    }
}
