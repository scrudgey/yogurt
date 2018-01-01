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
	public Vector2 maxXY;
	public Vector2 minXY;
	private Camera mainCamera;
	public AudioSource audioSource;
	void Start () {
		mainCamera = GetComponent<Camera>();
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	public void Shake(float intensity){
		StartCoroutine(screenShake(intensity));
	}
	private IEnumerator screenShake(float intensity){
		while(intensity > 0.01){
			shakeVector = Random.insideUnitCircle * intensity;
			intensity = intensity * 0.95f;
			yield return null;
		}
	}
	void FixedUpdate () {
		if (focus == null)
			return;
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
		mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, minSize + focus.GetComponent<Rigidbody2D>().velocity.magnitude/8f, 0.05f);
		if (mainCamera.orthographicSize > maxSize) {
			mainCamera.orthographicSize = maxSize;
		}
		// update camera world coordinates.
		lowerLeftWorld = GetComponent<Camera>().ScreenToWorldPoint( Vector2.zero);
		upperRightWorld = GetComponent<Camera>().ScreenToWorldPoint ( new Vector2(GetComponent<Camera>().pixelWidth,GetComponent<Camera>().pixelHeight) );
		screenWidthWorld = upperRightWorld.x - lowerLeftWorld.x;
		screenHeightWorld = upperRightWorld.y - lowerLeftWorld.y;
		tempVector = Vector3.SmoothDamp(transform.position , focus.transform.position + focusVelocity* focusLead,ref smoothVelocity, 0.1f);
		//check for edge of level
		if (tempVector.x - screenWidthWorld/2 < minXY.x){
			tempVector.x = minXY.x + screenWidthWorld/2;
		}
		if (tempVector.x + screenWidthWorld/2 > maxXY.x){
			tempVector.x = maxXY.x - screenWidthWorld/2;
		}
		if (tempVector.y - screenHeightWorld/2 < minXY.y){
			tempVector.y = minXY.y + screenHeightWorld/2;
		}
		if (tempVector.y + screenHeightWorld/2 > maxXY.y){
			tempVector.y = maxXY.y - screenHeightWorld/2;
		}
		tempVector = tempVector + shakeVector;
		// update camera position
		tempVector.z = -1f;
		transform.position = tempVector;
		focusLastPosition = focus.transform.position;
	}
}
