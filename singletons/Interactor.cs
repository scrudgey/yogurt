using UnityEngine;
// using System.Collections;
using System.Collections.Generic;

public class Interactor{
	// maybe the most important method, it tells all available actions
	// from the perspective of a player or whatever
	static public List<Interaction> GetInteractions(GameObject focus, GameObject target){

		List<Interaction> actionDictionary = new List<Interaction>();

		// no manual actions in here, only right click
		actionDictionary = ReportRightClickActions(target, focus);

		// free actions are okay whatever the occasion
		List<Interaction> freeActions = ReportFreeActions(target);
		actionDictionary.AddRange(freeActions);

		// what actions can the target do on *me* and my junk?
		List<Interaction> inverseActions = ReportRightClickActions(focus, target);
		foreach (Interaction inter in inverseActions)
			if (!actionDictionary.Contains(inter))   // inverse double-count diode
				actionDictionary.Add(inter);

		return actionDictionary;
	}

	static public Interaction GetDefaultAction( List<Interaction> actionList){
		int highestP = 0;
		Interaction returnInteraction = null;

		foreach ( Interaction action in actionList){
				if(action.defaultPriority > highestP && action.enabled){
					returnInteraction = action;
					highestP = action.defaultPriority;
				}
		}

		return returnInteraction;
	}

	static public List<Interaction> ReportFreeActions(GameObject targ){

		List<Interaction> returnDictionary = new List<Interaction> ();
		foreach (Interactive interactive in targ.GetComponentsInChildren<Interactive>() ){
			List<Interaction> freeActions = interactive.GetFreeActions();
			foreach (Interaction action in freeActions)
				returnDictionary.Add(action);
		}
		return returnDictionary;
	}

	static public List<Interaction> ReportManualActions(GameObject targ, GameObject source){
		List<Interaction> returnDictionary = new List<Interaction > ();
		List<Interactive> interactives = new List<Interactive> (source.GetComponentsInChildren<Interactive>() );
		
		foreach ( Interactive interactive in interactives)
			interactive.target = targ;

		foreach (Interactive interactive in interactives ){
			List<Interaction> actions = interactive.GetManualActions();
			foreach (Interaction action in actions)
				returnDictionary.Add(action);
		}
		return returnDictionary;
	}

	static public List<Interaction> ReportRightClickActions(GameObject targ, GameObject source){
		
		List<Interactive>interactives = new List<Interactive>(source.GetComponentsInChildren<Interactive>() );
		List<Interaction>returnDictionary = new List<Interaction> ();
		
		foreach (Interactive interactive in interactives)
			interactive.target = targ;

		foreach (Interactive interactive in interactives){
			List<Interaction> possibleActions = interactive.GetRightClickActions();
			foreach (Interaction action in possibleActions)
				returnDictionary.Add(action);
		}
		
		return returnDictionary;
		
	}

	static public List<Interaction> ReportActions(GameObject targ, GameObject source){

		List<Interactive> interactives = new List<Interactive> (source.GetComponentsInChildren<Interactive>() );
		List<Interaction> returnDictionary = new List<Interaction> ();

		foreach ( Interactive interactive in interactives)
			interactive.target = targ;
		
		foreach ( Interactive interactive in interactives){
			List<Interaction> possibleActions = interactive.GetEnabledActions();
			foreach (Interaction action in possibleActions)
				returnDictionary.Add(action);
		}

		return returnDictionary;

	}
}
