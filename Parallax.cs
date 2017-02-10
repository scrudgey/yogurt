using UnityEngine;
using System;
using System.Collections.Generic;
// using System.Collections;


public class Parallax : MonoBehaviour {
	[Serializable]
	public struct ParallaxLayer {
		public Vector2 originalPosition;
		public Transform transform;
		public float speed;
	}
	public Transform baseLayer;
	public Vector2 originalPosition;
	public List<ParallaxLayer> layers = new List<ParallaxLayer>();
	void Start(){
		Transform cameraTransform = GameObject.FindObjectOfType<Camera>().transform;
		originalPosition = cameraTransform.position;
		for (int i = 0; i < layers.Count; i++){
			ParallaxLayer layer = layers[i];
			layer.originalPosition = layers[i].transform.position;
			layers[i] = layer;
		}
	}
	void Update () {
		Vector3 difference = (Vector2)GameManager.Instance.cam.transform.position - originalPosition;
		foreach(ParallaxLayer layer in layers){
			layer.transform.position = baseLayer.position + difference * layer.speed;
		}
	}
}
