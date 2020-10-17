using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalInstrument : Interactive {
    public SoundEffect soundEffect;
    public AudioSource audioSource;
    public List<string> playTexts = new List<string>();
    public List<AudioClip> playSounds = new List<AudioClip>();
    public LoHi pitchRange;
    void Awake() {
        soundEffect = GetComponent<SoundEffect>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        Interaction balloon = new Interaction(this, "Play", "Play");
        balloon.defaultPriority = 2;
        // balloon.hideInClickMenu = true;
        balloon.otherOnSelfConsent = false;
        balloon.selfOnOtherConsent = false;
        balloon.holdingOnOtherConsent = false;
        // balloon.inertOnPlayerConsent = false;
        interactions.Add(balloon);
    }

    public void Play() {
        if (playSounds.Count > 0) {
            AudioClip sound = playSounds[Random.Range(0, playSounds.Count)];
            audioSource.pitch = Random.Range(pitchRange.low, pitchRange.high);
            audioSource.PlayOneShot(sound);
        }
        if (playTexts.Count > 0) {
            soundEffect.Say(playTexts[Random.Range(0, playTexts.Count)]);
        }
    }
}
