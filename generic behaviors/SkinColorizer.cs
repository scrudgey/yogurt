using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinColorizer : MonoBehaviour, ISaveable {
    public SkinColor skinColor;
    public SpriteRenderer spriteRenderer;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Toolbox.ApplySkinToneToSprite(spriteRenderer.sprite, skinColor);
    }

    public void SaveData(PersistentComponent data) {
        data.ints["skinColor"] = (int)skinColor;
    }

    public void LoadData(PersistentComponent data) {
        skinColor = (SkinColor)data.ints["skinColor"];
    }
}
