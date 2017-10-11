using UnityEngine;

public class RandomizeRotation : MonoBehaviour {
	void Start(){
		transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), new Vector3(0, 0, 1));
	}
}
