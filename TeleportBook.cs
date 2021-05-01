using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBook : Interactive {
    public AudioSource audioSource;
    public AudioClip fxSound;
    public ParticleSystem fx;
    public void Awake() {
        Interaction read = new Interaction(this, "Read", "Read");
        read.otherOnSelfConsent = false;
        read.holdingOnOtherConsent = false;
        read.defaultPriority = 6;
        read.descString = "Read the book of Eibon";

        interactions.Add(read);

        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void Read() {
        GameManager.Instance.data.teleportedToday = false;
        fx.Play();
        audioSource.PlayOneShot(fxSound);
    }
}
