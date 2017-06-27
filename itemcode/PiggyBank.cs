using UnityEngine;

public class PiggyBank : MonoBehaviour {
	void Start(){
		if (GameManager.Instance.data == null)
			return;
		if (GameManager.Instance.data.completeCommercials.Count > 0){
			// Debug.Log("get paid");
			Gibs dollarGib = gameObject.AddComponent<Gibs>();
			dollarGib.particle = Resources.Load("prefabs/dollar") as GameObject;
			dollarGib.number = 1;

		} else {
			// Debug.Log("not paid yet");
		}
	}
}
