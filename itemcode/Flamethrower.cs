using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : SquirtGun {
    public float sprayTimer;
    public float sprayInterval = 0.1f;
    public AudioClip flameStart;
    public AudioClip flameLoop;
    public float soundTimer;
    public bool startSoundPlayed;
    public override void doSquirt(Vector3 direction, Vector3 position) {
        if (sprayTimer > 0)
            return;
        GameObject droplet = Toolbox.Instance.SpawnDroplet(liquid, 0, gameObject, 0.05f, velocity * direction, noCollision: false);
        droplet.transform.position = position;
        foreach (Collider2D myCollider in transform.root.GetComponentsInChildren<Collider2D>()) {
            foreach (Collider2D dropCollider in droplet.transform.root.GetComponentsInChildren<Collider2D>()) {
                Physics2D.IgnoreCollision(myCollider, dropCollider, true);
            }
        }
        amount -= 1f;
        Flammable dropletFlammable = droplet.GetComponent<Flammable>();
        if (dropletFlammable) {
            dropletFlammable.SpontaneouslyCombust();
            dropletFlammable.responsibleParty = gameObject;
        }
        sprayTimer = sprayInterval;
        soundTimer = 0.2f;
    }
    void Update() {
        if (sprayTimer > 0) {
            sprayTimer -= Time.deltaTime;
        }
        if (soundTimer > 0) {
            soundTimer -= Time.deltaTime;
            if (!audioSource.isPlaying) {
                if (!startSoundPlayed) {
                    startSoundPlayed = true;
                    audioSource.loop = false;
                    audioSource.PlayOneShot(flameStart);
                } else {
                    audioSource.loop = true;
                    audioSource.clip = flameLoop;
                    audioSource.Play();
                }
            }
            if (soundTimer <= 0) {
                startSoundPlayed = false;
                audioSource.Stop();
            }
        }
    }
    override public void Awake() {
        if (!configured) {
            configured = true;
            // Interaction fillReservoir = new Interaction(this, "Fill", "FillFromReservoir");
            // Interaction fillContainer = new Interaction(this, "Fill", "FillFromContainer");
            // fillContainer.validationFunction = true;
            // interactions.Add(fillContainer);
            // interactions.Add(fillReservoir);

            Interaction squirtAction = new Interaction(this, "Shoot", "Squirt");//, false, true);
            squirtAction.selfOnOtherConsent = false;
            squirtAction.otherOnSelfConsent = false;
            squirtAction.defaultPriority = 6;
            squirtAction.validationFunction = true;
            squirtAction.dontWipeInterface = true;
            squirtAction.continuous = true;
            interactions.Add(squirtAction);

            Interaction spray2 = new Interaction(this, "Shoot", "SprayObject");//, true, false);
            spray2.selfOnSelfConsent = false;
            spray2.otherOnSelfConsent = false;
            spray2.unlimitedRange = true;
            spray2.dontWipeInterface = true;
            spray2.validationFunction = true;
            spray2.continuous = true;

            interactions.Add(spray2);
            if (liquidSprite)
                liquidSprite.enabled = false;
            if (initLiquid != "") {
                FillByLoad(initLiquid);
            }
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
    }
}
