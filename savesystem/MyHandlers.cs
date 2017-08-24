using UnityEngine;
using System;
using System.Collections.Generic;
using AI;

public abstract class SaveHandler{
	public abstract void SaveData(Component instance, PersistentComponent data, ReferenceResolver resolver);
	public abstract void LoadData(Component instance, PersistentComponent data);
}
public abstract class SaveHandler<T> : SaveHandler where T : Component
{
	public sealed override void SaveData(Component instance, PersistentComponent data, ReferenceResolver resolver){
		SaveData(instance as T, data, resolver);
	}
	public sealed override void LoadData(Component instance, PersistentComponent data){
		LoadData(instance as T, data);
	}
	public abstract void SaveData(T instance, PersistentComponent data, ReferenceResolver resolver);
	public abstract void LoadData(T instance, PersistentComponent data);
}
public class InventoryHandler : SaveHandler<Inventory> {
	public override void SaveData(Inventory instance, PersistentComponent data, ReferenceResolver resolver){
		if (instance.holding != null){
			data.ints["holdingID"] = resolver.ResolveReference(instance.holding.gameObject, data.persistent);
		} else {
			data.ints["holdingID"] = -1;
		}
		data.ints["itemCount"] = instance.items.Count;
		if (instance.items.Count > 0){
			for (int i = 0; i < instance.items.Count; i++){
				data.ints["item"+i.ToString()] = resolver.ResolveReference(instance.items[i], data.persistent);
			}
		} 
		data.vectors["direction"] = instance.direction;
	}
	public override void LoadData(Inventory instance, PersistentComponent data){
		instance.direction = data.vectors["direction"];
		if (data.ints["holdingID"] != -1){
			if (MySaver.loadedObjects.ContainsKey(data.ints["holdingID"]) ){
				instance.GetItem( MySaver.loadedObjects[data.ints["holdingID"]].GetComponent<Pickup>() );
			} else {
				Debug.Log("tried to get loadedobject " + data.ints["holdingID"].ToString() + " but was not found!");
			}
		}
		// note: trying to reference a key in data.ints that didn't exist here caused a hard crash at runtime
		// so PROTECT YA NECK!!!
		if (data.ints["itemCount"] > 0){
			for (int i = 0; i < data.ints["itemCount"]; i++){
				GameObject theItem = MySaver.loadedObjects[data.ints["item"+i.ToString()]];
				instance.items.Add(theItem);
				theItem.SetActive(false);
				PhysicalBootstrapper testBoot = theItem.GetComponent<PhysicalBootstrapper>();
				if (testBoot){
					testBoot.DestroyPhysical();
				}
			}
		}
		instance.initHolding = null;
	}
}
public class ContainerHandler: SaveHandler<Container> {
	public override void SaveData(Container instance, PersistentComponent data, ReferenceResolver resolver){
		data.ints["maxItems"] = 						instance.maxNumber;
		data.bools["disableContents"] = 				instance.disableContents;
		data.ints["itemCount"] = instance.items.Count;
		if (instance.items.Count > 0){
			for (int i = 0; i < instance.items.Count; i++){
				data.ints["item"+i.ToString()] = resolver.ResolveReference(instance.items[i].gameObject, data.persistent);
			}
		}
	}
	public override void LoadData(Container instance, PersistentComponent data){
		instance.maxNumber = 							data.ints["maxItems"];
		instance.disableContents = 						data.bools["disableContents"];
		if (data.ints["itemCount"] > 0){
			for (int i = 0; i < data.ints["itemCount"]; i++){
				instance.AddItem(MySaver.loadedObjects[data.ints["item"+i.ToString()]].GetComponent<Pickup>());
				PhysicalBootstrapper phys = instance.items[i].GetComponent<PhysicalBootstrapper>();
				if (phys)
					phys.doInit = false;
				// Debug.Log("container containing "+MySaver.loadedObjects[data.ints["item"+i.ToString()]].name);
			}
		}
	}
}
public class BlenderHandler: SaveHandler<Blender> {
	public override void SaveData(Blender instance, PersistentComponent data, ReferenceResolver resolver){
		ContainerHandler handler = new ContainerHandler();
		handler.SaveData((Container)instance, data, resolver);
		data.bools["power"] = instance.power;
	}
	public override void LoadData(Blender instance, PersistentComponent data){
		ContainerHandler handler = new ContainerHandler();
		handler.LoadData((Container)instance, data);
		instance.power = data.bools["power"];
	}
}
public class HeadHandler: SaveHandler<Head> {
	public override void SaveData(Head instance,PersistentComponent data, ReferenceResolver resolver){
		if (instance.hat != null){
			data.ints["hat"] = resolver.ResolveReference(instance.hat.gameObject, data.persistent);
		} else {
			data.ints["hat"] = -1;
		}
	}
	public override void LoadData(Head instance,PersistentComponent data){
		instance.initHat = null;
		if (data.ints["hat"] != -1){
			instance.DonHat(MySaver.loadedObjects[data.ints["hat"]].GetComponent<Hat>());
		}
	}
}
public class TraderHandler: SaveHandler<Trader>{
	public override void SaveData(Trader instance, PersistentComponent data, ReferenceResolver resolver){
		if (instance.give != null){
			data.ints["give"] = resolver.ResolveReference(instance.give, data.persistent);
			// Debug.Log("saving reference to give: "+data.ints["give"].ToString());
		}
		data.strings["receive"] = instance.receive;
	}
	public override void LoadData(Trader instance, PersistentComponent data){
		if (data.ints.ContainsKey("give")){
			// Debug.Log("handler loading give");
			// Debug.Log("looking for id "+data.ints["give"]);
			// foreach(KeyValuePair<int, GameObject> keyval in MySaver.loadedObjects){
			// 	Debug.Log(keyval.Key.ToString()+" "+keyval.Value.ToString());
			// }
			instance.give = MySaver.loadedObjects[data.ints["give"]];
		}
		instance.receive = data.strings["receive"];
	}
}
public class PackageHandler: SaveHandler<Package> {
	public override void SaveData(Package instance, PersistentComponent data, ReferenceResolver resolver){
		data.strings["contents"] = instance.contents;
	}
	public override void LoadData(Package instance, PersistentComponent data){
		instance.contents = data.strings["contents"];
	}
}
public class PhysicalBootStrapperHandler: SaveHandler<PhysicalBootstrapper> {
	public override void SaveData(PhysicalBootstrapper instance, PersistentComponent data, ReferenceResolver resolver){
		data.bools["doInit"] = instance.doInit;
		data.bools["physical"] = false;
		if (instance.physical != null){
			if (!instance.physical.isActiveAndEnabled)
				return;
			data.bools["physical"] = true;
			data.ints["mode"] = (int)instance.physical.currentMode;
			data.vectors["groundPosition"] = instance.physical.transform.position;
			data.vectors["objectPosition"] = instance.physical.hinge.transform.localPosition;

			data.floats["horizonOffsetX"] = instance.physical.horizonCollider.offset.x;
			data.floats["horizonOffsetY"] = instance.physical.horizonCollider.offset.y;
		}
	}
	public override void LoadData(PhysicalBootstrapper instance, PersistentComponent data){
		if (data.bools["physical"]){
			instance.loadData = data;
		}
		instance.doLoad = true;
	}
}
// to add: food preference enums
public class EaterHandler: SaveHandler<Eater> {
	public override void SaveData(Eater instance, PersistentComponent data, ReferenceResolver resolver){
		data.floats["nutrition"] = instance.nutrition;
	}
	public override void LoadData(Eater instance, PersistentComponent data){
		instance.nutrition = data.floats["nutrition"];
	}
}
// public class 
public class AdvancedAnimationHandler: SaveHandler<AdvancedAnimation> {
	public override void SaveData(AdvancedAnimation instance, PersistentComponent data, ReferenceResolver resolver){
		data.strings["baseName"] = instance.baseName;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadData(AdvancedAnimation instance, PersistentComponent data){
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		instance.baseName = data.strings["baseName"];
		instance.LoadSprites();
	}
}
public class HeadAnimationHandler: SaveHandler<HeadAnimation> {
	public override void SaveData(HeadAnimation instance, PersistentComponent data, ReferenceResolver resolver){
		data.ints["hitstate"] = (int)instance.hitState;
		data.strings["baseName"] = instance.baseName;
	}
	public override void LoadData(HeadAnimation instance, PersistentComponent data){
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		instance.baseName = data.strings["baseName"];
	}
}
public class SpeechHandler: SaveHandler<Speech>{
	public override void SaveData(Speech instance, PersistentComponent data, ReferenceResolver resolver){
		data.bools["speaking"] = instance.speaking;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadData(Speech instance, PersistentComponent data){
		instance.speaking = data.bools["speaking"];
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
public class FlammableHandler: SaveHandler<Flammable> {
	public override void SaveData(Flammable instance, PersistentComponent data, ReferenceResolver resolver){
		data.floats["heat"] = 				instance.heat;
		data.floats["flashpoint"] = 		instance.flashpoint;
		data.bools["onFire"] = 				instance.onFire;
	}
	public override void LoadData(Flammable instance, PersistentComponent data){
		instance.heat =						data.floats["heat"];
		instance.flashpoint = 				data.floats["flashpoint"];
		instance.onFire = 					data.bools["onFire"];
	}
}
public class DestructibleHandler: SaveHandler<Destructible> {
	public override void SaveData(Destructible instance, PersistentComponent data, ReferenceResolver resolver){
		data.floats["health"] = 			instance.health;
		data.bools["invulnerable"] = 		instance.invulnerable;
		data.bools["fireproof"] = 			instance.fireproof;
		data.bools["noPhysDam"] = 			instance.no_physical_damage;
		data.ints["lastDamage"] =			(int)instance.lastDamage;
	}
	public override void LoadData(Destructible instance, PersistentComponent data){
		instance.health = 					data.floats["health"];
		instance.invulnerable = 			data.bools["invulnerable"];
		instance.fireproof = 				data.bools["fireproof"];
		instance.no_physical_damage =		data.bools["noPhysDam"];
		instance.lastDamage =				(damageType)data.ints["lastDamage"];
	}
}
public class OutfitHandler: SaveHandler<Outfit> {
	public override void SaveData(Outfit instance, PersistentComponent data, ReferenceResolver resolver){
		data.strings["worn"] = 			instance.wornUniformName;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadData(Outfit instance, PersistentComponent data){
		instance.wornUniformName = 		data.strings["worn"];
		instance.initUniform = null;
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
public class CabinetHandler: SaveHandler<Cabinet> {
	public override void SaveData(Cabinet instance, PersistentComponent data, ReferenceResolver resolver){
		data.bools["opened"] = 			instance.opened;
	}
	public override void LoadData(Cabinet instance, PersistentComponent data){
		instance.opened = data.bools["opened"];
		if (instance.opened){
			instance.contained = new List<GameObject>();
		}
	}
}
public class LiquidContainerHandler: SaveHandler<LiquidContainer>{
	public override void SaveData(LiquidContainer instance, PersistentComponent data, ReferenceResolver resolver){
		data.floats["fillCapacity"] = 		instance.fillCapacity;
		data.floats["amount"] = 			instance.amount;
		data.bools["lid"] = 				instance.lid;
		if (instance.liquid != null){
			data.strings["liquid"] =			instance.liquid.name;
		} else {
			data.strings["liquid"] = "null";
		}
	}
	// if i had some nicer methods to handle loading an amount of liquid 
	// i think this would be a little better.
	public override void LoadData(LiquidContainer instance, PersistentComponent data){
		if (data.strings["liquid"] != "null"){
			instance.FillByLoad(data.strings["liquid"]);
		} else {
			instance.FillByLoad("Water");
		}
		instance.fillCapacity = 			data.floats["fillCapacity"];
		instance.amount = 					data.floats["amount"];
		instance.lid =						data.bools["lid"];
	}
}
public class IntrinsicsHandler: SaveHandler<Intrinsics>{
	public override void SaveData(Intrinsics instance, PersistentComponent data, ReferenceResolver resolver){
		data.intrinsics = new List<Intrinsic>();
		foreach(Intrinsic intrinsic in instance.intrinsics){
			if (intrinsic.persistent)
				data.intrinsics.Add(intrinsic);
		}
	}
	public override void LoadData(Intrinsics instance, PersistentComponent data){
		foreach(Intrinsic intrinsic in data.intrinsics){
			instance.intrinsics.Add(intrinsic);
		}
		if (data.intrinsics.Count > 0)
			instance.IntrinsicsChanged();
	}
}
public class HurtableHandler: SaveHandler<Hurtable>{
	public override void SaveData(Hurtable instance, PersistentComponent data, ReferenceResolver resolver){
		data.floats["health"] = instance.health;
		data.floats["maxHealth"] = instance.maxHealth;
		data.floats["bonusHealth"] = instance.bonusHealth;
		data.floats["impulse"] = instance.impulse;
		data.floats["downed_timer"] = instance.downedTimer;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadData(Hurtable instance, PersistentComponent data){
		instance.health = data.floats["health"];
		instance.maxHealth = data.floats["maxHealth"];
		instance.bonusHealth = data.floats["bonusHealth"];
		instance.impulse = data.floats["impulse"];
		instance.downedTimer = data.floats["downed_timer"];
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
public class HumanoidHandler: SaveHandler<Humanoid>{
	public override void SaveData(Humanoid instance, PersistentComponent data, ReferenceResolver resolver){
		data.strings["lastPressed"] = instance.lastPressed;
		data.vectors["direction"] = instance.direction;
		data.bools["fightMode"] = instance.fightMode;
		data.bools["disabled"] = instance.disabled;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadData(Humanoid instance, PersistentComponent data){
		instance.lastPressed = data.strings["lastPressed"];
		instance.SetDirection(data.vectors["direction"]);
		instance.disabled = data.bools["disabled"];
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		if (data.bools["fightMode"] && !instance.fightMode){
			instance.ToggleFightMode();
		}	
	}
}
public class DecisionMakerHandler: SaveHandler<DecisionMaker>{
	static List<Type> priorityTypes = new List<Type>(){
		{typeof(PriorityAttack)},
		{typeof(PriorityFightFire)},
		{typeof(PriorityProtectPossessions)},
		{typeof(PriorityReadScript)},
		{typeof(PriorityRunAway)},
		{typeof(PriorityWander)}
	};
	public override void SaveData(DecisionMaker instance, PersistentComponent data, ReferenceResolver resolver){
		data.ints["hitstate"] = (int)instance.hitState;
		// if (instance.possession != null)
		// 	data.ints["possession"] = resolver.ResolveReference(instance.possession, data.persistent);
		foreach (Type priorityType in priorityTypes){
			foreach(Priority priority in instance.priorities){
				if (priority.GetType() == priorityType){
					data.floats[priorityType.ToString()] = priority.urgency;
				}
			}
		}
	}
	public override void LoadData(DecisionMaker instance, PersistentComponent data){
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		instance.initialAwareness = new List<GameObject>();
		foreach (Priority priority in instance.priorities){
			foreach (Type priorityType in priorityTypes){
				if (priority.GetType() == priorityType){
					string priorityName = priorityType.ToString();
					if (data.floats.ContainsKey(priorityName)){
						priority.urgency = data.floats[priorityName];
						// Debug.Log("loaded "+priorityName+" with urgency "+priority.urgency.ToString());
					}
				}
			}
		}
	}
}
public class AwarenessHandler: SaveHandler<Awareness>{
	public override void SaveData(Awareness instance, PersistentComponent data, ReferenceResolver resolver){
		data.ints["hitstate"] = (int)instance.hitState;
		data.knowledgeBase = new List<SerializedKnowledge>();
		data.people = new List<SerializedPersonalAssessment>();
		if (instance.possession != null)
			data.ints["possession"] = resolver.ResolveReference(instance.possession, data.persistent);
		foreach (KeyValuePair<GameObject, Knowledge> keyVal in instance.knowledgebase){
			SerializedKnowledge knowledge = SaveKnowledge(keyVal.Value, resolver, data.persistent);
			if (knowledge.gameObjectID == -1)
				continue;
			data.knowledgeBase.Add(knowledge);
		}
		foreach (KeyValuePair<GameObject, PersonalAssessment> keyVal in instance.people){
			data.people.Add(SavePerson(keyVal.Value, resolver, data.persistent));
		}
		if (instance.possessionDefaultState != null){
			SerializedKnowledge knowledge = SaveKnowledge(instance.possessionDefaultState, resolver, data.persistent);
			if (knowledge.gameObjectID != -1)
				data.knowledges["defaultState"] = knowledge;
		}
	}
	public override void LoadData(Awareness instance, PersistentComponent data){
		if (data.ints.ContainsKey("possession")){
			instance.possession = MySaver.loadedObjects[data.ints["possession"]];
		}
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		foreach(SerializedKnowledge knowledge in data.knowledgeBase){
			Knowledge newKnowledge = LoadKnowledge(knowledge);
			GameObject subject = MySaver.loadedObjects[knowledge.gameObjectID];
			instance.knowledgebase[subject] = newKnowledge;
		}
		foreach(SerializedPersonalAssessment pa in data.people){
			PersonalAssessment assessment = LoadPerson(pa);
			GameObject subject = MySaver.loadedObjects[pa.gameObjectID];
			assessment.knowledge = instance.knowledgebase[subject];
			instance.people[subject] = assessment;
		}
		if (data.knowledges.ContainsKey("defaultState")){
			instance.possessionDefaultState = LoadKnowledge(data.knowledges["defaultState"]);
		}
		instance.SetNearestEnemy();
		instance.SetNearestFire();
	}
	SerializedKnowledge SaveKnowledge(Knowledge input, ReferenceResolver resolver, Persistent persistent){
		SerializedKnowledge data = new SerializedKnowledge();
		data.lastSeenPosition = input.lastSeenPosition;
		data.lastSeenTime = input.lastSeenTime;
		data.gameObjectID = resolver.ResolveReference(input.obj, persistent);
		return data;
	}
	Knowledge LoadKnowledge(SerializedKnowledge input){
		Knowledge knowledge = new Knowledge();
		knowledge.lastSeenPosition = input.lastSeenPosition;
		knowledge.lastSeenTime = input.lastSeenTime;
		knowledge.obj = MySaver.loadedObjects[input.gameObjectID];
		knowledge.transform = knowledge.obj.transform;
		knowledge.flammable = knowledge.obj.GetComponent<Flammable>();
		return knowledge;
	}
	SerializedPersonalAssessment SavePerson(PersonalAssessment input, ReferenceResolver resolver, Persistent persistent){
		SerializedPersonalAssessment data = new SerializedPersonalAssessment();
		data.status = input.status;
		data.unconscious = input.unconscious;
		data.gameObjectID = resolver.ResolveReference(input.knowledge.obj, persistent);
		return data;
	}
	PersonalAssessment LoadPerson(SerializedPersonalAssessment input){
		PersonalAssessment assessment = new PersonalAssessment();
		assessment.status = input.status;
		assessment.unconscious = input.unconscious;
		return assessment;
	}
}



