using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public GameObject focus;
	private Vector3 focusLastPosition = Vector3.zero;
	private Vector3 focusVelocity = Vector3.zero;
	public float focusLead;
	private Vector3 smoothVelocity = Vector3.zero;
	private Vector3 shakeVector = Vector3.zero;
	public float maxSize;
	public float minSize;

	// Use this for initialization
	void Start () {
	}

	public void Shake(float intensity){
		StartCoroutine(screenShake(intensity));
	}

	private IEnumerator screenShake(float intensity){

		while(intensity > 0.01){
			shakeVector = Random.insideUnitCircle * intensity;
			intensity = intensity * 0.90f;
			yield return null;
		}
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (focus){

			Vector3 lowerLeftWorld;
			Vector3 upperRightWorld;
			Vector3 tempVector;
			float screenWidthWorld;
			float screenHeightWorld;
			
			tempVector = transform.position;
			// interpolate target velocity
			if (focusLastPosition != Vector3.zero){
				focusVelocity = (focus.transform.position-focusLastPosition)/Time.deltaTime;
			} 

			// update camera zoom
			GetComponent<Camera>().orthographicSize = Mathf.Lerp(GetComponent<Camera>().orthographicSize, minSize + focus.GetComponent<Rigidbody2D>().velocity.magnitude/8f , 0.05f);
			if (GetComponent<Camera>().orthographicSize > maxSize) { GetComponent<Camera>().orthographicSize = maxSize;}

			// update camera world coordinates.
			lowerLeftWorld = GetComponent<Camera>().ScreenToWorldPoint( Vector2.zero);
			upperRightWorld = GetComponent<Camera>().ScreenToWorldPoint ( new Vector2(GetComponent<Camera>().pixelWidth,GetComponent<Camera>().pixelHeight) );
			screenWidthWorld = upperRightWorld.x - lowerLeftWorld.x;
			screenHeightWorld = upperRightWorld.y - lowerLeftWorld.y;

			tempVector = Vector3.SmoothDamp(transform.position,focus.transform.position + focusVelocity* focusLead,ref smoothVelocity,0.1f);
			tempVector = tempVector + shakeVector;

			//check for edge of level
			if (tempVector.x - screenWidthWorld/2 < -5){
				tempVector.x = -5f + screenWidthWorld/2;
			}
			if (tempVector.y - screenHeightWorld/2 < -5){
				tempVector.y = -5f + screenHeightWorld/2;
			}
			if (tempVector.x + screenWidthWorld/2 > 5){
				tempVector.x = 5f - screenWidthWorld/2;
			}
			if (tempVector.y + screenHeightWorld/2 > 5){
				tempVector.y = 5f - screenHeightWorld/2;
			}

			// update camera position
			tempVector.z = -1f;
			transform.position = tempVector;
			focusLastPosition = focus.transform.position;
		}
						
	}
	
}
