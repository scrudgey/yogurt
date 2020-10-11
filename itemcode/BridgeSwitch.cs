using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSwitch : Interactive, ISaveable {
    public Sprite upSprite;
    public Sprite downSprite;
    public bool on;
    public SpriteRenderer spriteRenderer;
    public AudioClip bridgeAppearSound;
    private AudioSource audioSource;
    public GameObject bridge;
    public GameObject bridgeCollider;
    public AudioClip onSound;
    public AudioClip offSound;
    public void Awake() {
        Interaction teleport = new Interaction(this, "Throw switch", "Teleport");
        interactions.Add(teleport);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void Teleport() {
        on = !on;
        Activate();
    }
    public void Activate() {
        if (on) {
            bridge.SetActive(true);
            bridgeCollider.SetActive(false);
            spriteRenderer.sprite = upSprite;
            audioSource.PlayOneShot(onSound);

        } else {
            bridge.SetActive(false);
            bridgeCollider.SetActive(true);
            spriteRenderer.sprite = downSprite;
            audioSource.PlayOneShot(offSound);

        }
    }
    public string Teleport_desc() {
        return "Throw switch";
    }
    public void SaveData(PersistentComponent data) {
        data.bools["on"] = on;
    }
    public void LoadData(PersistentComponent data) {
        on = data.bools["on"];
        Activate();
    }
}
