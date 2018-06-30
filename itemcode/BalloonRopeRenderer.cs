using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonRopeRenderer : MonoBehaviour {
    public LineRenderer lineRenderer;
    public GameObject balloon;
    public Vector3 tieOffset;
	void Awake () {
		lineRenderer = GetComponent<LineRenderer>();
        // balloon = transform.Find("/balloon").gameObject;
        DistanceJoint2D joint = GetComponent<DistanceJoint2D>();
        tieOffset = new Vector3(joint.connectedAnchor.x, joint.connectedAnchor.y, 0);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 newPoint = balloon.transform.localPosition;
        // float z = 0f;
        float z = balloon.transform.rotation.eulerAngles.z * transform.lossyScale.x;
        z *= Mathf.Deg2Rad;

        // newPoint += tieOffset;

        Vector3 offset = Vector2.zero;
        offset.x = tieOffset.x * Mathf.Cos(z) - tieOffset.y * Mathf.Sin(z);
        offset.y = tieOffset.x * Mathf.Sin(z) + tieOffset.y * Mathf.Cos(z);
        newPoint += offset;

        lineRenderer.SetPosition(1, newPoint);
	}
}
