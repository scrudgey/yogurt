using UnityEngine;
using System;
using System.Collections.Generic;
using AI;

public abstract class SaveHandler{
	public abstract void SaveData(Component instance, PersistentComponent data);
	public abstract void LoadData(Component instance, PersistentComponent data);
}
public abstract class SaveHandler<T> : SaveHandler where T : Component
{
	public sealed override void SaveData(Component instance, PersistentComponent data){
		// TODO: handle it better if instance is not T.
		SaveSpecificData(instance as T, data);
	}
	public sealed override void LoadData(Component instance, PersistentComponent data){
		LoadSpecificData(instance as T, data);
	}
	public abstract void SaveSpecificData(T instance, PersistentComponent data);
	public abstract void LoadSpecificData(T instance, PersistentComponent data);
}
public class InventoryHandler : SaveHandler<Inventory> {
	public override void SaveSpecificData(Inventory instance, PersistentComponent data){
		if (instance.holding != null){
			data.ints["holdingID"] = MySaver.GameObjectToID(instance.holding.gameObject);
			MySaver.AddToReferenceTree(data, instance.holding.gameObject);
		} else {
			data.ints["holdingID"] = -1;
		}
		data.ints["itemCount"] = instance.items.Count;
		if (instance.items.Count > 0){
			for (int i = 0; i < instance.items.Count; i++){
				data.ints["item"+i.ToString()] = MySaver.GameObjectToID(instance.items[i]);
				MySaver.AddToReferenceTree(data, instance.items[i]);
			}
		} 
		data.vectors["direction"] = instance.direction;
	}
	public override void LoadSpecificData(Inventory instance, PersistentComponent data){
		instance.direction = data.vectors["direction"];
		if (data.ints["holdingID"] != -1){
			GameObject go = MySaver.IDToGameObject(data.ints["holdingID"]);
			if (go != null){
				instance.GetItem(go.GetComponent<Pickup>());
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
	public override void SaveSpecificData(Container instance, PersistentComponent data){
		data.ints["maxItems"] = 						instance.maxNumber;
		data.bools["disableContents"] = 				instance.disableContents;
		data.ints["itemCount"] = instance.items.Count;
		if (instance.items.Count > 0){
			for (int i = 0; i < instance.items.Count; i++){
				data.ints["item"+i.ToString()] = MySaver.GameObjectToID(instance.items[i].gameObject);
				MySaver.AddToReferenceTree(data, instance.items[i].gameObject);
			}
		}
	}
	public override void LoadSpecificData(Container instance, PersistentComponent data){
		instance.maxNumber = 							data.ints["maxItems"];
		instance.disableContents = 						data.bools["disableContents"];
		if (data.ints["itemCount"] > 0){
			for (int i = 0; i < data.ints["itemCount"]; i++){
				GameObject go = MySaver.IDToGameObject(data.ints["item"+i.ToString()]);
				if (go != null){
					instance.AddItem(go.GetComponent<Pickup>());
					PhysicalBootstrapper phys = go.GetComponent<PhysicalBootstrapper>();
					if (phys)
						phys.doInit = false;
				}
				// Debug.Log("container containing "+MySaver.loadedObjects[data.ints["item"+i.ToString()]].name);
			}
		}
	}
}
public class BlenderHandler: SaveHandler<Blender> {
	public override void SaveSpecificData(Blender instance, PersistentComponent data){
		ContainerHandler handler = new ContainerHandler();
		handler.SaveData((Container)instance, data);
		data.bools["power"] = instance.power;
	}
	public override void LoadSpecificData(Blender instance, PersistentComponent data){
		ContainerHandler handler = new ContainerHandler();
		handler.LoadData((Container)instance, data);
		instance.power = data.bools["power"];
	}
}
public class HeadHandler: SaveHandler<Head> {
	public override void SaveSpecificData(Head instance, PersistentComponent data){
		if (instance.hat != null){
			data.ints["hat"] = MySaver.GameObjectToID(instance.hat.gameObject);
			MySaver.AddToReferenceTree(data, instance.hat.gameObject);
		} else {
			data.ints["hat"] = -1;
		}
	}
	public override void LoadSpecificData(Head instance, PersistentComponent data){
		instance.initHat = null;
		if (data.ints["hat"] != -1){
			GameObject hat = MySaver.IDToGameObject(data.ints["hat"]);
			if (hat != null){
				// Debug.Log("donning hat in load");
				instance.DonHat(hat.GetComponent<Hat>());
			}
		}
	}
}
public class TraderHandler: SaveHandler<Trader>{
	public override void SaveSpecificData(Trader instance, PersistentComponent data){
		if (instance.give != null){
			data.ints["give"] = MySaver.GameObjectToID(instance.give);
			MySaver.AddToReferenceTree(data, instance.give);
		}
		data.strings["receive"] = instance.receive;
	}
	public override void LoadSpecificData(Trader instance, PersistentComponent data){
		if (data.ints.ContainsKey("give")){
			instance.give = MySaver.IDToGameObject(data.ints["give"]);
		}
		instance.receive = data.strings["receive"];
	}
}
public class PackageHandler: SaveHandler<Package> {
	public override void SaveSpecificData(Package instance, PersistentComponent data){
		data.strings["contents"] = instance.contents;
	}
	public override void LoadSpecificData(Package instance, PersistentComponent data){
		instance.contents = data.strings["contents"];
	}
}
public class PhysicalBootStrapperHandler: SaveHandler<PhysicalBootstrapper> {
	public override void SaveSpecificData(PhysicalBootstrapper instance, PersistentComponent data){
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
	public override void LoadSpecificData(PhysicalBootstrapper instance, PersistentComponent data){
		if (data.bools["physical"]){
			instance.loadData = data;
		}
		instance.doLoad = true;
	}
}
// to add: food preference enums
public class EaterHandler: SaveHandler<Eater> {
	public override void SaveSpecificData(Eater instance, PersistentComponent data){
		data.floats["nutrition"] = instance.nutrition;
	}
	public override void LoadSpecificData(Eater instance, PersistentComponent data){
		instance.nutrition = data.floats["nutrition"];
	}
}
// public class 
public class AdvancedAnimationHandler: SaveHandler<AdvancedAnimation> {
	public override void SaveSpecificData(AdvancedAnimation instance, PersistentComponent data){
		data.strings["baseName"] = instance.baseName;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadSpecificData(AdvancedAnimation instance, PersistentComponent data){
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		instance.baseName = data.strings["baseName"];
		instance.LoadSprites();
	}
}
public class HeadAnimationHandler: SaveHandler<HeadAnimation> {
	public override void SaveSpecificData(HeadAnimation instance, PersistentComponent data){
		data.ints["hitstate"] = (int)instance.hitState;
		data.strings["baseName"] = instance.baseName;
	}
	public override void LoadSpecificData(HeadAnimation instance, PersistentComponent data){
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		instance.baseName = data.strings["baseName"];
	}
}
public class SpeechHandler: SaveHandler<Speech>{
	public override void SaveSpecificData(Speech instance, PersistentComponent data){
		data.bools["speaking"] = instance.speaking;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadSpecificData(Speech instance, PersistentComponent data){
		instance.speaking = data.bools["speaking"];
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
public class FlammableHandler: SaveHandler<Flammable> {
	public override void SaveSpecificData(Flammable instance, PersistentComponent data){
		data.floats["heat"] = 				instance.heat;
		data.floats["flashpoint"] = 		instance.flashpoint;
		data.bools["onFire"] = 				instance.onFire;
	}
	public override void LoadSpecificData(Flammable instance, PersistentComponent data){
		instance.heat =						data.floats["heat"];
		instance.flashpoint = 				data.floats["flashpoint"];
		instance.onFire = 					data.bools["onFire"];
	}
}
public class DropDripperHandler: SaveHandler<DropDripper> {
	public override void SaveSpecificData(DropDripper instance, PersistentComponent data){
		data.ints["amount"] = instance.amount;
	}
	public override void LoadSpecificData(DropDripper instance, PersistentComponent data){
		instance.amount = data.ints["amount"];
	}
}
public class DestructibleHandler: SaveHandler<Destructible> {
	public override void SaveSpecificData(Destructible instance, PersistentComponent data){
		data.floats["health"] = 			instance.health;
		data.bools["invulnerable"] = 		instance.invulnerable;
		data.bools["fireproof"] = 			instance.fireproof;
		data.bools["noPhysDam"] = 			instance.no_physical_damage;
		data.ints["lastDamage"] =			(int)instance.lastDamage;
	}
	public override void LoadSpecificData(Destructible instance, PersistentComponent data){
		instance.health = 					data.floats["health"];
		instance.invulnerable = 			data.bools["invulnerable"];
		instance.fireproof = 				data.bools["fireproof"];
		instance.no_physical_damage =		data.bools["noPhysDam"];
		instance.lastDamage =				(damageType)data.ints["lastDamage"];
	}
}
public class OutfitHandler: SaveHandler<Outfit> {
	public override void SaveSpecificData(Outfit instance, PersistentComponent data){
		data.strings["worn"] = 			instance.wornUniformName;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadSpecificData(Outfit instance, PersistentComponent data){
		instance.wornUniformName = 		data.strings["worn"];
		instance.initUniform = null;
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
public class CabinetHandler: SaveHandler<Cabinet> {
	public override void SaveSpecificData(Cabinet instance, PersistentComponent data){
		data.bools["opened"] = 			instance.opened;
	}
	public override void LoadSpecificData(Cabinet instance, PersistentComponent data){
		instance.opened = data.bools["opened"];
		if (instance.opened){
			instance.contained = new List<GameObject>();
		}
	}
}
public class LiquidContainerHandler: SaveHandler<LiquidContainer>{
	public override void SaveSpecificData(LiquidContainer instance, PersistentComponent data){
		data.floats["fillCapacity"] = 		instance.fillCapacity;
		data.floats["amount"] = 			instance.amount;
		data.bools["lid"] = 				instance.lid;
		if (instance.liquid != null){
			data.strings["liquid"] =		instance.liquid.filename;
		} else {
			data.strings["liquid"] = 		"";
		}
	}
	// if i had some nicer methods to handle loading an amount of liquid 
	// i think this would be a little better.
	public override void LoadSpecificData(LiquidContainer instance, PersistentComponent data){
		if (data.strings["liquid"] != ""){
			instance.FillByLoad(data.strings["liquid"]);
		} else {
			instance.FillByLoad("water");
		}
		instance.fillCapacity = 			data.floats["fillCapacity"];
		instance.amount = 					data.floats["amount"];
		instance.lid =						data.bools["lid"];
	}
}
public class IntrinsicsHandler: SaveHandler<Intrinsics>{
	public override void SaveSpecificData(Intrinsics instance, PersistentComponent data){
		data.intrinsics = new List<Intrinsic>();
		foreach(Intrinsic intrinsic in instance.intrinsics){
			data.intrinsics.Add(intrinsic);
		}
		// Debug.Log(data.intrinsics.Count);
	}
	public override void LoadSpecificData(Intrinsics instance, PersistentComponent data){
		instance.intrinsics = new List<Intrinsic>();
		foreach(Intrinsic intrinsic in data.intrinsics){
			instance.intrinsics.Add(intrinsic);
		}
		// Debug.Log(instance.name + " " + data.intrinsics.Count.ToString());
		if (data.intrinsics.Count > 0)
			instance.IntrinsicsChanged();
	}
}
public class HurtableHandler: SaveHandler<Hurtable>{
	public override void SaveSpecificData(Hurtable instance, PersistentComponent data){
		data.floats["health"] = instance.health;
		data.floats["maxHealth"] = instance.maxHealth;
		data.floats["bonusHealth"] = instance.bonusHealth;
		data.floats["impulse"] = instance.impulse;
		data.floats["downed_timer"] = instance.downedTimer;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadSpecificData(Hurtable instance, PersistentComponent data){
		instance.health = data.floats["health"];
		instance.maxHealth = data.floats["maxHealth"];
		instance.bonusHealth = data.floats["bonusHealth"];
		instance.impulse = data.floats["impulse"];
		instance.downedTimer = data.floats["downed_timer"];
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
public class HumanoidHandler: SaveHandler<Humanoid>{
	public override void SaveSpecificData(Humanoid instance, PersistentComponent data){
		data.strings["lastPressed"] = instance.lastPressed;
		data.vectors["direction"] = instance.direction;
		data.bools["fightMode"] = instance.fightMode;
		data.bools["disabled"] = instance.disabled;
		data.ints["hitstate"] = (int)instance.hitState;
	}
	public override void LoadSpecificData(Humanoid instance, PersistentComponent data){
		instance.lastPressed = data.strings["lastPressed"];
		instance.SetDirection(data.vectors["direction"]);
		instance.disabled = data.bools["disabled"];
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		if (data.bools["fightMode"] && !instance.fightMode){
			instance.ToggleFightMode();
		}	
	}
}
public class VideoCameraHandler: SaveHandler<VideoCamera>{
	public override void SaveSpecificData(VideoCamera instance, PersistentComponent data){
		data.commercials = new List<Commercial>();
		data.commercials.Add(instance.commercial);
		data.bools["live"] = instance.live;
	}
	public override void LoadSpecificData(VideoCamera instance, PersistentComponent data){
		instance.commercial = data.commercials[0];
		instance.live = data.bools["live"];
		if (data.bools["live"]){
			instance.Enable();
		}
	}
}
public class StainHandler: SaveHandler<Stain>{
	public override void SaveSpecificData(Stain instance, PersistentComponent data){
		data.ints["parentID"] = MySaver.GameObjectToID(instance.parent);
		MySaver.AddToReferenceTree(data, instance.gameObject);
		// PersistentObject stainPersistent = MySaver.persistentObjects.FindKeyByValue(instance.gameObject);
		// PersistentObject parentPersistent = MySaver.persistentObjects.FindKeyByValue(instance.parent);
		MonoLiquid stainLiquid = instance.GetComponent<MonoLiquid>();
		if (stainLiquid != null)
			data.strings["liquid"] = stainLiquid.liquid.filename;
	}
	public override void LoadSpecificData(Stain instance, PersistentComponent data){
		if (data.ints["parentID"] != -1){
			instance.ConfigureParentObject(MySaver.IDToGameObject(data.ints["parentID"]));
			if (data.strings.ContainsKey("liquid")){
				Liquid.MonoLiquidify(instance.gameObject, Liquid.LoadLiquid(data.strings["liquid"]));
			}
		} else {
			instance.RemoveStain();
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
	public override void SaveSpecificData(DecisionMaker instance, PersistentComponent data){
		data.ints["hitstate"] = (int)instance.hitState;
		foreach (Type priorityType in priorityTypes){
			foreach(Priority priority in instance.priorities){
				if (priority.GetType() == priorityType){
					data.floats[priorityType.ToString()] = priority.urgency;
				}
			}
		}
		if (instance.protectionZone != null)
			data.ints["protectID"] = MySaver.GameObjectToID(instance.protectionZone.gameObject);
		if (instance.warnZone != null)
			data.ints["warnID"] = MySaver.GameObjectToID(instance.warnZone.gameObject);
	}
	public override void LoadSpecificData(DecisionMaker instance, PersistentComponent data){
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		instance.initialAwareness = new List<GameObject>();
		if (data.ints.ContainsKey("protectID")){
			GameObject protectObject = MySaver.IDToGameObject(data.ints["protectID"]);
			if (protectObject != null){
				instance.awareness.protectZone = protectObject.GetComponent<BoxCollider2D>();
			}
		}
		if (data.ints.ContainsKey("warnID")){
			GameObject warnObject = MySaver.IDToGameObject(data.ints["warnID"]);
			Debug.Log(data.ints["warnID"]);
			Debug.Log(warnObject);
			if (warnObject != null){
				instance.awareness.warnZone = warnObject.GetComponent<Collider2D>();
			}
		}
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
	public override void SaveSpecificData(Awareness instance, PersistentComponent data){
		data.ints["hitstate"] = (int)instance.hitState;
		data.knowledgeBase = new List<SerializedKnowledge>();
		foreach(SerializedKnowledge sk in instance.longTermMemory){
			data.knowledgeBase.Add(sk);
		}
		data.people = new List<SerializedPersonalAssessment>();
		foreach(SerializedPersonalAssessment spa in instance.longtermPersonalAssessments){
			data.people.Add(spa);
		}
		if (instance.possession != null)
			data.ints["possession"] = MySaver.GameObjectToID(instance.possession);
			MySaver.AddToReferenceTree(data, instance.possession);
		foreach (KeyValuePair<GameObject, Knowledge> keyVal in instance.knowledgebase){
			SerializedKnowledge knowledge = SaveKnowledge(keyVal.Value, data.persistent);
			if (knowledge.gameObjectID == -1)
				continue;
			data.knowledgeBase.Add(knowledge);
		}
		foreach (KeyValuePair<GameObject, PersonalAssessment> keyVal in instance.people){
			data.people.Add(SavePerson(keyVal.Value, data.persistent));
		}
		if (instance.possessionDefaultState != null){
			SerializedKnowledge knowledge = SaveKnowledge(instance.possessionDefaultState, data.persistent);
			if (knowledge.gameObjectID != -1)
				data.knowledges["defaultState"] = knowledge;
		}
	}
	public override void LoadSpecificData(Awareness instance, PersistentComponent data){
		if (data.ints.ContainsKey("possession")){
			instance.possession = MySaver.IDToGameObject(data.ints["possession"]);
		}
		instance.hitState = (Controllable.HitState)data.ints["hitstate"];
		foreach(SerializedKnowledge knowledge in data.knowledgeBase){
			Knowledge newKnowledge = LoadKnowledge(knowledge);
			if (newKnowledge.obj != null){
				instance.knowledgebase[newKnowledge.obj] = newKnowledge;
			} else {
				// TODO: how to save the serialized knowledge if it isnt in scene?'
				instance.longTermMemory.Add(knowledge);
			}
		}
		foreach(SerializedPersonalAssessment pa in data.people){
			GameObject go = MySaver.IDToGameObject(pa.gameObjectID);
			if (go != null){
				PersonalAssessment assessment = LoadPerson(pa);
				assessment.knowledge = instance.knowledgebase[go];
				instance.people[go] = assessment;
			} else {
				instance.longtermPersonalAssessments.Add(pa);
			}
		}
		if (data.knowledges.ContainsKey("defaultState")){
			instance.possessionDefaultState = LoadKnowledge(data.knowledges["defaultState"]);
		}
		instance.SetNearestEnemy();
		instance.SetNearestFire();
	}
	SerializedKnowledge SaveKnowledge(Knowledge input, PersistentObject persistent){
		SerializedKnowledge data = new SerializedKnowledge();
		data.lastSeenPosition = input.lastSeenPosition;
		data.lastSeenTime = input.lastSeenTime;
		data.gameObjectID = MySaver.GameObjectToID(input.obj);
		return data;
	}
	Knowledge LoadKnowledge(SerializedKnowledge input){
		Knowledge knowledge = new Knowledge();
		knowledge.lastSeenPosition = input.lastSeenPosition;
		knowledge.lastSeenTime = input.lastSeenTime;
		knowledge.obj = MySaver.IDToGameObject(input.gameObjectID);
		if (knowledge.obj != null){
			knowledge.transform = knowledge.obj.transform;
			knowledge.flammable = knowledge.obj.GetComponent<Flammable>();
		}
		return knowledge;
	}
	SerializedPersonalAssessment SavePerson(PersonalAssessment input, PersistentObject persistent){
		SerializedPersonalAssessment data = new SerializedPersonalAssessment();
		data.status = input.status;
		data.unconscious = input.unconscious;
		data.gameObjectID = MySaver.GameObjectToID(input.knowledge.obj);
		return data;
	}
	PersonalAssessment LoadPerson(SerializedPersonalAssessment input){
		PersonalAssessment assessment = new PersonalAssessment();
		assessment.status = input.status;
		assessment.unconscious = input.unconscious;
		return assessment;
	}
}



