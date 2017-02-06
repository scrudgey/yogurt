using UnityEngine;
using System;
using System.Collections.Generic;
// using System.Collections;
[Serializable]
public struct ParallaxLayer {
	public Vector3 originalPosition;
	public Transform transform;
	public float speed;
}
public class Parallax : MonoBehaviour {
	public Transform baseLayer;
	public Vector3 originalPosition;
	public List<ParallaxLayer> layers = new List<ParallaxLayer>();
	// public SerializableDictionary<Transform, float> layerSpeeds = new SerializableDictionary<Transform, float>();
	void Start(){
		Transform cameraTransform = GameObject.FindObjectOfType<Camera>().transform;
		// Debug
		Debug.Log(cameraTransform.position);
		originalPosition = cameraTransform.position;
		// originalPosition = baseLayer.transform.position;
		// Transform cameraTransform = GameManager.Instance.cam.transform;
		// originalPosition = new Vector3(cameraTransform.position.x, cameraTransform.position.y, cameraTransform.position.z);
		// foreac(ParallaxLayer layer in layers){
		for (int i = 0; i < layers.Count; i++){
			ParallaxLayer layer = layers[i];
			// layer.originalPosition = layers[i].transform.TransformPoint(Vector2.zero);
			layer.originalPosition = layers[i].transform.position;
			layers[i] = layer;
		}
	}
	void Update () {
		Vector3 difference = GameManager.Instance.cam.transform.position - originalPosition;
		foreach(ParallaxLayer layer in layers){
			// layer.transform.position = layer.originalPosition + difference * layer.speed;
			layer.transform.position = baseLayer.position - difference * layer.speed;
			// layer.transform.position.z = 0;
		}
	}
}
