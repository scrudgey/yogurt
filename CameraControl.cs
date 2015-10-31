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

	// Use this for initialization
	void Start () {
		mainCamera = GetComponent<Camera>();
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
			mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, minSize + focus.GetComponent<Rigidbody2D>().velocity.magnitude/8f , 0.05f);
			if (mainCamera.orthographicSize > maxSize) { mainCamera.orthographicSize = maxSize;}

			// update camera world coordinates.
			RectTransform UIRect = UISystem.Instance.background.GetComponent<RectTransform>();
//			lowerLeftWorld = mainCamera.ScreenToWorldPoint(new Vector2(0, mainCamera.WorldToScreenPoint(UIRect.position).y + UIRect.rect.height/2));
//			upperRightWorld = mainCamera.ScreenToWorldPoint(new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight) );
			lowerLeftWorld = GetComponent<Camera>().ScreenToWorldPoint( Vector2.zero);
			upperRightWorld = GetComponent<Camera>().ScreenToWorldPoint ( new Vector2(GetComponent<Camera>().pixelWidth,GetComponent<Camera>().pixelHeight) );
			screenWidthWorld = upperRightWorld.x - lowerLeftWorld.x;
			screenHeightWorld = upperRightWorld.y - lowerLeftWorld.y;

			Vector3 POV = new Vector3();
			// calculate the height of the UI panel
			if (UISystem.Instance.background){
				POV = new Vector3(mainCamera.pixelWidth/2, mainCamera.pixelHeight/2 + mainCamera.WorldToScreenPoint(UIRect.position).y/2 + UIRect.rect.height/4);
				POV = transform.position - mainCamera.ScreenToWorldPoint(POV);
			} 

			tempVector = Vector3.SmoothDamp(transform.position , focus.transform.position + POV + focusVelocity* focusLead,ref smoothVelocity,0.1f);
			tempVector = tempVector + shakeVector;

			//check for edge of level
//			if (tempVector.x - screenWidthWorld/2 < minXY.x){
//				tempVector.x = minXY.x + screenWidthWorld/2;
//			}
//			if (tempVector.y - screenHeightWorld/2 - POV.y < minXY.y){
//				tempVector.y = minXY.y + screenHeightWorld/2 + POV.y;
//			}
//			if (tempVector.x + screenWidthWorld/2 > maxXY.x){
//				tempVector.x = maxXY.x - screenWidthWorld/2;
//			}
//			if (tempVector.y + screenHeightWorld/2 + POV.y > maxXY.y){
//				tempVector.y = maxXY.y - screenHeightWorld/2 - POV.y;
//			}

			if (tempVector.x - screenWidthWorld/2 < minXY.x){
				tempVector.x = minXY.x + screenWidthWorld/2;
			}
			if (tempVector.y - screenHeightWorld/2 < minXY.y){
				tempVector.y = minXY.y + screenHeightWorld/2;
			}
			if (tempVector.x + screenWidthWorld/2 > maxXY.x){
				tempVector.x = maxXY.x - screenWidthWorld/2;
			}
			if (tempVector.y + screenHeightWorld/2 > maxXY.y){
				tempVector.y = maxXY.y - screenHeightWorld/2;
			}

			// update camera position
			tempVector.z = -1f;
			transform.position = tempVector;
			focusLastPosition = focus.transform.position;
		}
						
	}
	
}
