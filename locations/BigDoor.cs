using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigDoor : MonoBehaviour {
    public Sprite openSprite;
    public Sprite closedSprite;
    public Doorway doorway;
    public bool configured;
    void Update() {
        if (configured)
            return;
        configured = true;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (GameManager.Instance.data.days > GameManager.HellDoorClosesOnDay) {
            spriteRenderer.sprite = closedSprite;
            doorway.disableInteractions = true;
            doorway.enabled = false;
        } else {
            spriteRenderer.sprite = openSprite;
            doorway.disableInteractions = false;
            doorway.enabled = true;
        }
    }
}
