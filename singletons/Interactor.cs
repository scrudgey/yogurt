using UnityEngine;
using System.Collections.Generic;

public class Interactor{
	// maybe the most important method, it tells all available actions
	// from the perspective of a player or whatever
	static public List<Interaction> GetInteractions(GameObject focus, GameObject target){
		// Debug.Log(focus.name+" on "+target.name);
		List<Interaction> actionDictionary = new List<Interaction>();
		// no manual actions in here, only right click
		actionDictionary = ReportRightClickActions(target, focus);
		// free actions are okay whatever the occasion
		List<Interaction> freeActions = ReportFreeActions(target, focus);
		actionDictionary.AddRange(freeActions);
		// what actions can the target do on *me* and my junk?
		List<Interaction> inverseActions = ReportRightClickActions(focus, target);
		foreach (Interaction inter in inverseActions)
			if (!actionDictionary.Contains(inter))   // inverse double-count diode
				actionDictionary.Add(inter);
		return actionDictionary;
	}
	static public Interaction GetDefaultAction(List<Interaction> actionList){
		int highestP = 0;
		Interaction returnInteraction = null;
		foreach (Interaction action in actionList){
			if(action.defaultPriority > highestP && action.enabled){
				returnInteraction = action;
				highestP = action.defaultPriority;
			}
		}
		return returnInteraction;
	}
	static public List<Interaction> ReportFreeActions(GameObject targ, GameObject source){
		List<Interaction> returnDictionary = new List<Interaction> ();
		foreach (Interactive interactive in GetInteractorTree(targ)){
			List<Interaction> freeActions = interactive.GetFreeActions(targ, source);
			foreach (Interaction action in freeActions){
				returnDictionary.Add(action);
			}
		}
		return returnDictionary;
	}
	static public List<Interaction> ReportManualActions(GameObject targ, GameObject source){
		List<Interaction> returnDictionary = new List<Interaction> ();
		List<Interactive> interactives = GetInteractorTree(source);
		List<Interactive> targetInteractives = GetInteractorTree(targ);
		foreach (Interactive interactive in interactives)
			interactive.targetUpdate(targetInteractives, targ, source);
		foreach (Interactive interactive in interactives){
			List<Interaction> actions = interactive.GetManualActions();
			foreach (Interaction action in actions){
				returnDictionary.Add(action);
			}
		}
		return returnDictionary;
	}

	static public List<Interaction> ReportRightClickActions(GameObject targ, GameObject source){
		List<Interactive>targetInteractives = new List<Interactive>();
		List<Interactive>sourceInteractives = new List<Interactive>();
		targetInteractives = GetInteractorTree(targ);
		sourceInteractives = GetInteractorTree(source);
		List<Interaction>returnDictionary = new List<Interaction>();
		foreach (Interactive interactive in sourceInteractives)
			interactive.targetUpdate(targetInteractives, targ, source);
		foreach (Interactive interactive in sourceInteractives){
			foreach (Interaction action in interactive.GetRightClickActions()){
				returnDictionary.Add(action);
			}	
		}
		return returnDictionary;
	}
	static public List<Interactive> GetInteractorTree(GameObject target){
		List<Interactive>targetInteractives = new List<Interactive>(target.GetComponents<Interactive>());
		Interactive.targetType targType = Interactive.TypeOfTarget(target);
		if (targType == Interactive.targetType.inert){
			foreach(Interactive interactive in target.GetComponentsInParent<Interactive>()){
				if (!targetInteractives.Contains(interactive))
					targetInteractives.Add(interactive);
			}
			foreach(Interactive interactive in target.GetComponentsInChildren<Interactive>()){
				if (!targetInteractives.Contains(interactive))
					targetInteractives.Add(interactive);
			}
			return targetInteractives;
		}
		Inventory targetInv = target.GetComponent<Inventory>();
		if (targetInv != null){
			if (targetInv.holding != null){
				targetInteractives.AddRange(targetInv.holding.GetComponents<Interactive>());
			}
		}
		Pickup pickup = target.GetComponent<Pickup>();
		if (pickup != null){
			// Debug.Log("pickup");
			// Debug.Log(pickup.holder);
			if (pickup.holder != null)
				targetInteractives.AddRange(pickup.holder.gameObject.GetComponents<Interactive>());
		}
		Head targetHead = target.GetComponentInChildren<Head>();
		if (targetHead != null){
			targetInteractives.AddRange(targetHead.gameObject.GetComponents<Interactive>());
		}
		return targetInteractives;
	}
}
