using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Intrinsics : MonoBehaviour {

	public List<Intrinsic> intrinsics = new List<Intrinsic>();
	public bool netTelepath;

	private Humanoid humanoid;
	private float baseSpeed = 0;

	void Start(){
		humanoid = gameObject.GetComponent<Humanoid>();
		if (humanoid)
			baseSpeed = humanoid.maxSpeed;
	}

	public void AddIntrinsic(Intrinsic i){
		intrinsics.Add(i);
		RecalculateIntrinsics();
	}

	public void RemoveIntrinsic(Intrinsic i){
		intrinsics.Remove(i);
		RecalculateIntrinsics();
	}

	public void RecalculateIntrinsics(){
		// TODO: this whole part will change  

		// set base values
		netTelepath = false;
		if (humanoid)
			humanoid.maxSpeed = baseSpeed;

		foreach(Intrinsic i in intrinsics){
			switch(i.type){
			case Intrinsic.IntrinsicType.telepathy:
				netTelepath = netTelepath || i.boolValue;
				break;
			case Intrinsic.IntrinsicType.speed:
				if (humanoid)
					humanoid.maxSpeed += i.floatValue;
				break;
			default:
				break;
			}
		}


		if (GameManager.Instance.playerObject = gameObject){
			GameManager.Instance.telepathyOn = netTelepath;
		}
	}

	public void Update(){
		foreach(Intrinsic i in intrinsics){
			i.lifetime += Time.deltaTime;
			if (i.lifetime >= i.timeout && i.timeout > 0)
				RemoveIntrinsic(i);
		}
	}

}


[System.Serializable]
public class Intrinsic {
	public enum IntrinsicType{none,telepathy,speed}
	public IntrinsicType type;
	public bool boolValue;
	public float floatValue;
	public float lifetime;
	public float timeout;
}