using UnityEngine;
public class LiquidResevoir : Interactive {
	public Liquid liquid;
	public AudioClip fillSound;
	public enum liquidType{
		water,
		test2
	}

	public liquidType containedLiquid;
	public string type;

	 void Start () {

		switch (containedLiquid){
		case liquidType.water:
			type = "water";
			break;
		case liquidType.test2:
			type = "drain cleaner";
			break;
		default:
			type = "default";
			break;
		}

		liquid = LiquidCollection.LoadLiquid(type);
	}

}
