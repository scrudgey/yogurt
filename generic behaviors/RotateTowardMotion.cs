using UnityEngine;
using System.Collections;

public class RotateTowardMotion : MonoBehaviour {

	Vector3 tempRotate = Vector3.zero;

	void Update () {
		tempRotate.z = Toolbox.Instance.ProperAngle(GetComponent<Rigidbody2D>().velocity.x,GetComponent<Rigidbody2D>().velocity.y) + 90f;
		transform.rotation = Quaternion.identity;
		transform.Rotate(tempRotate);
	}
}
