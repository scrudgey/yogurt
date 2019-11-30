using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecroGate : Doorway {
    public bool locked;
    MessageSpeech message;
    public AnimateFrames animateFrames;
    public Sprite[] unlockedFrames;
    public ParticleSystem[] particles;
    public override void Awake() {
        Interaction leaveaction = new Interaction(this, actionDesc, "Portal");
        interactions.Add(leaveaction);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);

        message = new MessageSpeech("It's shut.");

    }

    public void Portal() {
        if (locked) {
            Toolbox.Instance.SendMessage(Controller.Instance.focus.gameObject, this, message);
        } else {
            Leave();
        }
    }
    public string Portal_desc() {
        if (leaveDesc != "") {
            return leaveDesc;
        } else {
            return "Go to " + destination;
        }
    }
    public void Unlock() {
        locked = false;
        animateFrames.frames = new List<Sprite>(unlockedFrames);
        foreach (ParticleSystem particle in particles) {
            particle.Play();
        }
    }
}
