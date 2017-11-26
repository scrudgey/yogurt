using UnityEngine;

public class FollowGameObject : MonoBehaviour {
	public GameObject target;
	private Vector3 offset;
	public void Init(){
		if (target == null)
			Destroy(this);
		offset = transform.position - target.transform.position;
	}
	public void Update(){
		if (target == null){
			Destroy(this);
			return;
		}
		transform.position = target.transform.position + offset;
	}
}
