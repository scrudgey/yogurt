using UnityEngine;

public class DoorwayZone : Doorway {
    public override void Enter(GameObject player) {
        enterPoint = transform.Find("enterPoint");
        player.transform.position = enterPoint.position;
        PlayEnterSound();
    }
    void OnTriggerEnter2D(Collider2D collider) {
        Exit(collider);
    }
    void OnTriggerStay2D(Collider2D collider) {
        Exit(collider);
    }
    void Exit(Collider2D collider) {
        if (disableInteractions)
            return;
        if (InputController.forbiddenTags.Contains(collider.tag))
            return;
        if (GameManager.Instance.playerObject == null)
            return;
        if (collider.transform == GameManager.Instance.playerObject.transform || collider.transform.IsChildOf(GameManager.Instance.playerObject.transform)) {
            Leave();
        }
    }
}
