using UnityEngine;
using System.Collections;

public class LiquidResevoir : Item {
	public Liquid liquid;

	public enum liquidType{
		water,
		test2
	}

	public liquidType containedLiquid;
	public string type;

	// Use this for initialization
	 void Start () {

		switch (containedLiquid){
		case liquidType.water:
			type = "Water";
			break;
		case liquidType.test2:
			type = "Test Liquid 2";
			break;
		default:
			type = "default";
			break;
		}

		liquid = LiquidCollection.LoadLiquid(type);
	}

}
