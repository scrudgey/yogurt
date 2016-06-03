using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("LiquidCollection")]
public class LiquidCollection{

	[XmlArray("Liquids"), XmlArrayItem("liquid")]
	public List<Liquid> Liquids = new List<Liquid>();

	public static LiquidCollection Load(){

		var serializer = new XmlSerializer(typeof(LiquidCollection));
		TextAsset textAsset = Resources.Load("data/liquids/testLiquid.xml") as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		XmlTextReader xmlReader = new XmlTextReader(stringReader);
		LiquidCollection lc = (LiquidCollection)serializer.Deserialize(xmlReader);
		return lc;

	}


	public static void LoadMonoLiquid(string type, MonoLiquid monoLiquid){
		Liquid returnLiquid;

		returnLiquid = LoadLiquid(type);

		monoLiquid.liquid = returnLiquid;

		monoLiquid.edible.nutrition = returnLiquid.nutrition;
		monoLiquid.edible.vegetable = returnLiquid.vegetable;
		monoLiquid.edible.meat = returnLiquid.meat;
		monoLiquid.edible.immoral = returnLiquid.immoral;
		monoLiquid.edible.offal = returnLiquid.offal;

	}

	public static void MonoLiquidify(GameObject target, Liquid liquid){

		MonoLiquid monoLiquid = target.GetComponent<MonoLiquid>();

		if (!monoLiquid){
			monoLiquid = target.AddComponent<MonoLiquid>();
		}
		monoLiquid.liquid = liquid;
		
		monoLiquid.edible.nutrition = liquid.nutrition;
		monoLiquid.edible.vegetable = liquid.vegetable;
		monoLiquid.edible.meat = liquid.meat;
		monoLiquid.edible.immoral = liquid.immoral;
		monoLiquid.edible.offal = liquid.offal;
		monoLiquid.edible.pureeColor = liquid.color;
        monoLiquid.edible.poison = liquid.poison;
        monoLiquid.edible.vomit = liquid.vomit;

		if (liquid.flammable){
			Flammable flam = target.AddComponent<Flammable>();
			flam.flashpoint = 1;
		}

	}

	public static Liquid LoadLiquid(string type){
		
		Liquid returnLiquid;

		var serializer = new XmlSerializer(typeof(LiquidCollection));
		TextAsset textAsset = Resources.Load("data/liquids/testLiquid") as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		XmlTextReader xmlReader = new XmlTextReader(stringReader);
		LiquidCollection lc = (LiquidCollection)serializer.Deserialize(xmlReader);
		
		returnLiquid = lc.Liquids[0];
		foreach (Liquid l in lc.Liquids ){
			if (l.name == type){returnLiquid = l;}
		}
		
		returnLiquid.color = new Color(returnLiquid.colorR/255.0F,returnLiquid.colorG/255.0F,returnLiquid.colorB/255.0F);
		
		return returnLiquid;
	}



}
