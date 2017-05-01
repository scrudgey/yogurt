using UnityEngine;
// using System.Collections;
// using System.Xml;
// using System.Xml.Serialization;

[System.Serializable]
public class Liquid {
	public string name;
	public float colorR;
	public float colorG;
	public float colorB;
	public float colorA;
	public bool poison;
	public Color color;
	public float nutrition;
	public bool vegetable;
	public bool meat;
	public bool immoral;
	public bool offal;
	public bool flammable;
    public bool vomit;

    public Liquid(){
        
    }
    public Liquid(float r, float g, float b){
        color = new Color(r/255.0F, g/255.0F, b/255.0F);
        vegetable = true;
        nutrition = 10;
    }
    
	public static Liquid MixLiquids(Liquid l1, Liquid l2){
		Liquid returnLiquid = l1;

		returnLiquid.vegetable = l1.vegetable | l2.vegetable;
		returnLiquid.meat = l1.meat | l2.meat;
		returnLiquid.immoral = l1.immoral | l2.immoral;
		returnLiquid.offal = l1.offal | l2.offal;
		returnLiquid.poison = l1.poison | l2.poison;

		returnLiquid.colorR = (l1.colorR + l2.colorR) / 2.0f;
		returnLiquid.colorG = (l1.colorG + l2.colorG) / 2.0f;
		returnLiquid.colorB = (l1.colorB + l2.colorB) / 2.0f;

		returnLiquid.color = new Color(returnLiquid.colorR/255.0F,returnLiquid.colorG/255.0F,returnLiquid.colorB/255.0F);
		
		if (!l1.name.Contains("mix")){
			l1.name = l1.name + "-" + l2.name + " mix";
		}
		
		return returnLiquid;
	}
    
    // public static Liquid water = new Liquid(50, 100, 100);
	// public static Liquid water = LiquidCollection.LoadLiquid("water");
	// public static Liquid toiletWater = LiquidCollection.LoadLiquid("toilet water");

}