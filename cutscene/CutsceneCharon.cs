using UnityEngine;
using System.Collections.Generic;
using Nimrod;

public class CutsceneCharon : Cutscene {
    protected enum State { none, walkRight, boatRight, stop };
    protected State state;
    protected GameObject player;
    protected Transform footPoint;
    // Controllable playerControl;
    // Hurtable playerHurtable;
    Controller playerController;
    BoxCollider2D loadZone;
    GameObject charon;
    Intrinsics playerIntrinsics;
    BoxCollider2D leftCollider;
    BoxCollider2D rightCollider;
    BoxCollider2D ratchetCollider;

    public override void Configure() {
        player = GameManager.Instance.playerObject;
        playerIntrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(player);
        InputController.Instance.suspendInput = true;

        leftCollider = GameObject.Find("charonLeftBlocker").GetComponent<BoxCollider2D>();
        leftCollider.enabled = false;
        ratchetCollider = GameObject.Find("charonRatchet").GetComponent<BoxCollider2D>();
        ratchetCollider.enabled = false;

        charon = GameObject.Find("Charon");
        loadZone = charon.transform.Find("loadZone").GetComponent<BoxCollider2D>();

        playerController = new Controller(InputController.Instance.focus);

        state = State.walkRight;

        UINew.Instance.RefreshUI(active: false);
        configured = true;
    }
    public override void Update() {
        //4.989473 5.089
        if (state == State.walkRight) {
            playerController.rightFlag = true;
            if (loadZone.bounds.Contains(player.transform.position)) {
                state = State.boatRight;
                playerController.ResetInput();
                InputController.Instance.suspendInput = false;
                playerController.Deregister();
                leftCollider.enabled = true;


                playerIntrinsics.AddNewLiveBuff(new Buff(BuffType.death, true, 1f, 0f));
                playerIntrinsics.IntrinsicsChanged();
                charon.GetComponent<Speech>().defaultMonologue = "charon2";
                Toolbox.Instance.AudioSpeaker("ominous", player.transform.position);
            }
        } else if (state == State.boatRight) {
            player = GameManager.Instance.playerObject;
            if (player == null) {
                return;
            }
            foreach (Transform transform in new List<Transform> { charon.transform, player.transform }) {
                Vector3 pos = transform.position;
                pos.x += Time.deltaTime / 2f;
                transform.position = pos;
            }
            if (player.transform.position.x >= 10.1) {
                // state = State.stop;
                ratchetCollider.enabled = true;
            }
            if (charon.transform.position.x >= 8.2) {
                state = State.stop;
            }
        } else if (state == State.stop) {
            End();
        }

    }
    void End() {
        UINew.Instance.RefreshUI(active: true);
        complete = true;
    }
}