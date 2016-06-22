using UnityEngine;

public class CutsceneObject : MonoBehaviour {

	public float dx;
	public float dy;
	public float tickInterval;
	private float timer;

	void Start(){
		timer = 0f;
	}
	void Update(){
		timer += Time.deltaTime;
		if (timer > tickInterval){
			Vector3 tempPos = transform.position;
			tempPos.x += dx;
			tempPos.y += dy;
			transform.position = tempPos;
			timer = 0f;
		}
	}
}
