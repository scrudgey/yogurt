using UnityEngine;
public class LiquidResevoir : Interactive {
	public Liquid liquid;
	public AudioClip fillSound;
	public enum liquidType{
		water,
		drainCleaner,
		toiletWater
	}

	public liquidType containedLiquid;
	public string type;

	 void Awake () {

		switch (containedLiquid){
		case liquidType.water:
			type = "water";
			break;
		case liquidType.drainCleaner:
			type = "drain cleaner";
			break;
		case liquidType.toiletWater:
			type = "toilet water";
			break;
		default:
			type = "default";
			break;
		}
		liquid = LiquidCollection.LoadLiquid(type);
	}

}
