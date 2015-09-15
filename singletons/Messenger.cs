using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Messenger : Singleton<Messenger> {

	public string MOTD = "smoke weed every day";
	public Dictionary<GameObject,IExcludable> claimedItems;

	void Start(){
		claimedItems = new Dictionary<GameObject,IExcludable >();
//		MySaver.OnPostLoad += ListObjects;
	}

	public void ListObjects(){
		foreach (GameObject o in claimedItems.Keys){
			Debug.Log(o.name);
		}
	}


	public void ClaimObject(GameObject obj, IExcludable owner){
//		Debug.Log("claiming "+obj.name);
		// if someone else owns the object, tell them that it's being taken.
		if (claimedItems.ContainsKey(obj)){
			claimedItems[obj].DropMessage(obj);
		}
		
		claimedItems[obj] = owner;
	}

	public void DisclaimObject(GameObject obj, IExcludable owner){
//		Debug.Log("disclaiming "+obj.name);
		if (claimedItems.ContainsKey(obj))
			if (claimedItems[obj] == owner)
				claimedItems.Remove(obj);
	}

	public void WasDestroyed(GameObject obj){
		if (claimedItems.ContainsKey(obj)){
			claimedItems[obj].WasDestroyed(obj);
		}
		Destroy(obj);
	}

}
