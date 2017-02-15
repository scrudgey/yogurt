using UnityEngine;
// using System.Collections;

public class DoorwayZone : Doorway {
	public Transform enterPoint;
	public override void Start () {
		// enterPoint = transform.Find("enterPoint");
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	public override void Enter(GameObject player){
		enterPoint = transform.Find("enterPoint");
		player.transform.position = enterPoint.position;
		PlayEnterSound();
	}
	void OnTriggerEnter2D(Collider2D collider){
		if (collider.gameObject == GameManager.Instance.playerObject){
			Leave();
		}
	}
}
