using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bones : MonoBehaviour {
    static bool xraySought;
    static bool xrayFound;
    public Sprite boneSprite;
    public BonesFollower follower;
    public void Start() {

        // if (!xraySought) {
        //     xraySought = true;
        //     xrayFound = GameObject.Find("xray screen") != null;
        // }
        // if (!xrayFound) {
        //     Destroy(this);
        //     return;
        // }

        GameObject bone = GameObject.Instantiate(Resources.Load("bonesFollower")) as GameObject;
        SpriteRenderer boneSpriteRenderer = bone.GetComponent<SpriteRenderer>();
        follower = bone.GetComponent<BonesFollower>();
        follower.follow = this;
        if (boneSprite != null) {
            boneSpriteRenderer.sprite = boneSprite;
        } else {
            SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();

            boneSpriteRenderer.sprite = myRenderer.sprite;
            boneSpriteRenderer.material = Resources.Load("material/bones") as Material;
        }
    }
}
