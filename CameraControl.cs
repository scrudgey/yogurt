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

	private Camera camera;

	// Use this for initialization
	void Start () {
		camera = GetComponent<Camera>();
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
			camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, minSize + focus.GetComponent<Rigidbody2D>().velocity.magnitude/8f , 0.05f);
			if (camera.orthographicSize > maxSize) { camera.orthographicSize = maxSize;}

			// update camera world coordinates.
			RectTransform UIRect = UISystem.Instance.gameObject.GetComponent<RectTransform>();
			//			lowerLeftWorld = camera.ScreenToWorldPoint( Vector2.zero);
			lowerLeftWorld = camera.ScreenToWorldPoint( new Vector2(0, camera.WorldToScreenPoint(UIRect.position).y + UIRect.rect.height/2));
			upperRightWorld = camera.ScreenToWorldPoint ( new Vector2(camera.pixelWidth,camera.pixelHeight) );
			screenWidthWorld = upperRightWorld.x - lowerLeftWorld.x;
			screenHeightWorld = upperRightWorld.y - lowerLeftWorld.y;

			Vector3 POV = new Vector3();
			// calculate the height of the UI panel
			if (UISystem.Instance.gameObject){
				POV = new Vector3(camera.pixelWidth/2, camera.pixelHeight/2 + camera.WorldToScreenPoint(UIRect.position).y/2 + UIRect.rect.height/4);
				POV = transform.position - camera.ScreenToWorldPoint(POV);
			} 

			tempVector = Vector3.SmoothDamp(transform.position , focus.transform.position + POV + focusVelocity* focusLead,ref smoothVelocity,0.1f);
			tempVector = tempVector + shakeVector;

			//check for edge of level
//			if ( lowerLeftWorld.x < minXY.x){
//				tempVector.x = minXY.x + screenWidthWorld/2;
//			}
//			if ( lowerLeftWorld.y < minXY.y){
//				tempVector.y = minXY.y + screenHeightWorld/2;
//			}
//			if ( upperRightWorld.x > maxXY.x){
//				tempVector.x = maxXY.x - screenWidthWorld/2;
//			}
//			if (upperRightWorld.y > maxXY.y){
//				tempVector.y = maxXY.y - screenHeightWorld/2;
//			}
			if (tempVector.x - screenWidthWorld/2 < minXY.x){
				tempVector.x = minXY.x + screenWidthWorld/2;
			}
			if (tempVector.y - screenHeightWorld/2 - POV.y < minXY.y){
				tempVector.y = minXY.y + screenHeightWorld/2 + POV.y;
			}
			if (tempVector.x + screenWidthWorld/2 > maxXY.x){
				tempVector.x = maxXY.x - screenWidthWorld/2;
			}
			if (tempVector.y + screenHeightWorld/2 + POV.y > maxXY.y){
				tempVector.y = maxXY.y - screenHeightWorld/2 - POV.y;
			}

			// update camera position
			tempVector.z = -1f;
			transform.position = tempVector;
			focusLastPosition = focus.transform.position;
		}
						
	}
	
}
