using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrenade : Interactive {
    public Sprite liveSprite;
    public SpriteRenderer spriteRenderer;
    public bool live;
    public Explosive explosive;
    public AudioSource audioSource;
    public AudioClip liveSound;
    public float timer;
    public float fuseTime = 2f;
    void Start() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);

        explosive.enabled = false;

        Interaction pinAct = new Interaction(this, "Pull pin", "Pin");
        pinAct.holdingOnOtherConsent = false;
        pinAct.holdingOnOtherConsent = false;
        pinAct.validationFunction = true;
        pinAct.descString = "Pull pin";
        interactions.Add(pinAct);
    }
    void Update() {
        if (live) {
            timer += Time.deltaTime;
            if (timer > fuseTime) {
                explosive.Explode();
            }
        }
    }
    public void Pin() {
        live = true;

        audioSource.PlayOneShot(liveSound);
        explosive.enabled = true;
        timer = 0;
        spriteRenderer.sprite = liveSprite;
    }
    public bool Pin_Validation() {
        return !live;
    }
}
