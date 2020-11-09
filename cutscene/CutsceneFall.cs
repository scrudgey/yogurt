using UnityEngine;
using System.Collections.Generic;

public class CutsceneFall : Cutscene {
    protected GameObject player;
    AdvancedAnimation playerAnimation;
    Collider2D playerCollider;
    protected Rigidbody2D playerBody;
    Controllable playerControl;
    Hurtable playerHurtable;
    Inventory playerInv;
    Pickup initHolding;
    protected float initDrag;
    protected virtual float fallDist() { return -0.3f; }
    public override void Configure() {
        player = GameManager.Instance.playerObject;
        playerAnimation = player.GetComponent<AdvancedAnimation>();
        playerCollider = player.GetComponent<Collider2D>();
        playerBody = player.GetComponent<Rigidbody2D>();
        playerControl = player.GetComponent<Controllable>();
        playerHurtable = player.GetComponent<Hurtable>();
        playerInv = GameManager.Instance.playerObject.GetComponent<Inventory>();
        // if (playerCollider == null) {
        //     playerCollider = player.transform.Find("footPoint").GetComponent<Collider2D>();
        // }
        // if (playerBody == null) {
        //     player.AddComponent<Rigidbody2D>();
        // }
        if (playerInv != null) {
            initHolding = playerInv.holding;
        }
        if (playerAnimation) {
            playerAnimation.enabled = false;
            playerAnimation.LateUpdate();
        }
        if (playerCollider)
            playerCollider.enabled = false;
        if (playerControl) {
            playerControl.enabled = false;
        }
        if (playerBody) {
            playerBody.gravityScale = 1f;
            initDrag = playerBody.drag;
            playerBody.drag = 0;
        }
        UINew.Instance.RefreshUI();
        configured = true;
    }
    public override void Update() {
        if (player.transform.position.y < fallDist()) {
            Toolbox.Instance.AudioSpeaker("Poof 01", player.transform.position);
            if (playerAnimation)
                playerAnimation.enabled = true;
            if (playerCollider)
                playerCollider.enabled = true;
            if (playerControl) {
                playerControl.enabled = true;
            }
            if (playerBody) {
                playerBody.gravityScale = 0; ;
                playerBody.drag = initDrag;
            }
            if (playerHurtable) {
                playerHurtable.KnockDown();
                playerHurtable.downedTimer = 3f;
            }
            UINew.Instance.RefreshUI(active: true);
            if (playerInv != null && initHolding != null) {
                playerInv.GetItem(initHolding);
            }
            complete = true;
            // Debug.Log("complete");
        }
    }
}