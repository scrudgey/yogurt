using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinColorizer : MonoBehaviour {
    public SkinColor skinColor;
    public SpriteRenderer spriteRenderer;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Toolbox.ApplySkinToneToSprite(spriteRenderer.sprite, skinColor);
    }
}
