using UnityEngine;

public class DoorwayZone : Doorway {
    // public Transform enterPoint;
    public override void Enter(GameObject player) {
        enterPoint = transform.Find("enterPoint");
        player.transform.position = enterPoint.position;
        PlayEnterSound();
    }
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject == GameManager.Instance.playerObject) {
            Leave();
        }
    }
}
