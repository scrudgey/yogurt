using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSwitch : Interactive {
    public Sprite upSprite;
    public Sprite downSprite;
    public bool on;
    public SpriteRenderer spriteRenderer;
    public AudioClip teleportSound;
    private AudioSource audioSource;
    public TrapDoor trapDoor;
    public void Awake() {
        Interaction teleport = new Interaction(this, "Throw switch", "Teleport");
        interactions.Add(teleport);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        teleport.validationFunction = true;
    }
    public void Teleport() {
        on = !on;
        if (on)
            spriteRenderer.sprite = downSprite;
        else spriteRenderer.sprite = upSprite;
        // do teleport
        audioSource.PlayOneShot(teleportSound);
        trapDoor.Activate();
    }
    public string Teleport_desc() {
        return "Throw switch";
    }

    public bool Teleport_Validation() {
        return !on;
    }

}
