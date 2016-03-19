using UnityEngine;
// using System.Collections;

public class FollowGameObjectInCamera : MonoBehaviour {
    
    public GameObject target;
    private RectTransform rect;
    private RectTransform canvasRect;
	void Start () {
       rect = (RectTransform)transform;
       canvasRect = transform.parent.GetComponent<RectTransform>();
	}
	
    void Update () {
        if (target){
            Vector2 pos = Camera.main.WorldToViewportPoint(target.transform.position + new Vector3(0f, 0.25f, 0f));
            Vector2 screenPos = new Vector2(
                (pos.x * canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f),
                (pos.y * canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f)
                );
            rect.localPosition = screenPos;
        }
        
    }
}
