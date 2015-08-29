using UnityEngine;
using System.Collections;

public class MyMarker : MonoBehaviour {


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
