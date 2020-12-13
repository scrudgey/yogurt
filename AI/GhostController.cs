using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour {

    private DirectionEnum dir;
    private float wanderTime = 0;
    public Controller control;
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public int spriteIndex;
    float timer;
    public Rigidbody2D body;

    void Awake() {
        wanderTime = UnityEngine.Random.Range(0, 2);
        dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        control = new Controller(gameObject);
    }
    public void Update() {
        control.ResetInput();
        if (wanderTime > 0) {
            switch (dir) {
                case DirectionEnum.down:
                    control.downFlag = true;
                    break;
                case DirectionEnum.left:
                    control.leftFlag = true;
                    break;
                case DirectionEnum.right:
                    control.rightFlag = true;
                    break;
                case DirectionEnum.up:
                    control.upFlag = true;
                    break;
            }
        }
        if (wanderTime < -1f) {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        } else {
            wanderTime -= Time.deltaTime;
        }
        if (spriteRenderer != null) {
            if (body.velocity.magnitude > 0.02f) {
                timer += Time.deltaTime;
                if (timer > 0.1f) {
                    timer = 0f;
                    spriteIndex += 1;
                    if (spriteIndex == sprites.Length) {
                        spriteIndex = 0;
                    }
                    spriteRenderer.sprite = sprites[spriteIndex];
                }
            } else {
                spriteRenderer.sprite = sprites[0];
            }
        }
    }
}
