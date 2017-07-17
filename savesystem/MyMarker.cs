using UnityEngine;
using System.Collections.Generic;
public class MyMarker : MonoBehaviour {
	public int id = -1;
	public List<GameObject> persistentChildren;
	void OnDisable(){
		MySaver.disabledPersistents.Add(gameObject);
	}
	void OnEnable(){
		MySaver.disabledPersistents.Remove(gameObject);
	}
	void OnDestroy(){
		if (MySaver.disabledPersistents.Contains(gameObject)){
			MySaver.disabledPersistents.Remove(gameObject);
		}
	}
}
