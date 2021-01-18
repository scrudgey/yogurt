using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespondentBar : MonoBehaviour {
    enum State { normal, fistPound }
    State state;
    float timer;
    public SpriteRenderer spriteRenderer;
    public Sprite mouthClosed;
    public Sprite mouthOpen;
    public Sprite armUp;
    public Sprite armDown;
    public bool speaking;
    public bool mouth;
    public SoundEffect soundEffect;
    public AudioSource soundEffectAudio;
    Vector2 soundEffectInitialPosition;
    public bool playedHitSound;
    public void PoundFist() {
        playedHitSound = false;
        state = State.fistPound;
        timer = 0f;
        soundEffect.transform.position = 0.05f * Random.insideUnitCircle + soundEffectInitialPosition;
    }
    void Start() {
        soundEffectInitialPosition = soundEffect.transform.position;
        Toolbox.RegisterMessageCallback<MessageHead>(this, HandleMessageHead);
        soundEffectAudio = soundEffect.GetComponent<AudioSource>();
    }
    void Update() {
        timer += Time.deltaTime;
        switch (state) {
            case State.normal:
                if (speaking && timer > 0.1f) {
                    timer = 0f;
                    mouth = !mouth;
                    if (mouth) {
                        spriteRenderer.sprite = mouthOpen;
                    } else {
                        spriteRenderer.sprite = mouthClosed;
                    }
                } else if (!speaking) {
                    spriteRenderer.sprite = mouthClosed;
                }
                break;
            case State.fistPound:
                if (timer < 0.2f) {
                    spriteRenderer.sprite = armUp;
                } else if (timer >= 0.2f && timer < 0.4f) {
                    spriteRenderer.sprite = armDown;
                    if (!playedHitSound) {
                        soundEffectAudio.Play();
                        playedHitSound = true;
                    }
                    soundEffect.Say("*WHAM*");
                } else if (timer >= 0.4f) {
                    state = State.normal;
                    timer = 0f;
                }
                break;
        }

    }

    void HandleMessageHead(MessageHead message) {
        speaking = message.value;
    }
}
