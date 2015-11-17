using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactor{

	static public float interactionDistance = 0.4f;
//	static string lastToolTip;
//	static public List<Interaction> actionDictionary = new List<Interaction>();
//	static private Vector3 mouseClickLocation;

	//okay, why the fuck does removing this break everything?
	// don't remove it!
	new void OnDestroy() {
//		Debug.Log("something destroyed the interactor!!!");
	}

	// maybe the most important method, it tells all available actions
	// from the perspective of a player or whatever
	static public List<Interaction> GetInteractions(GameObject focus, GameObject target){

		List<Interaction> actionDictionary = new List<Interaction>();

		// no manual actions in here, only right click
		actionDictionary = ReportRightClickActions(target,focus);

		// free actions are okay whatever the occasion
		List<Interaction> freeActions = ReportFreeActions(target);
		actionDictionary.AddRange(freeActions);

		// what actions can the target do on *me* and my junk?
		List<Interaction> inverseActions = ReportRightClickActions(focus,target);
		foreach (Interaction inter in inverseActions)
			if (!actionDictionary.Contains(inter) )   // inverse double-count diode
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
		
		List<Interactive> interactives = new List<Interactive> (source.GetComponentsInChildren<Interactive>() );
		List<Interaction> returnDictionary = new List<Interaction> ();
		
		foreach ( Interactive interactive in interactives)
			interactive.target = targ;

		
		foreach ( Interactive interactive in interactives){
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

	// n.b. : my own registered event- NOT OnMouseUp().
//	public void MouseUp(){
//		clickMenu = false;
//		if (lastToolTip != null && lastToolTip != "")
//			foreach (Interaction inter in actionDictionary)
//				if (inter.actionName == lastToolTip)
//					inter.DoAction();
//		lastToolTip = null;
//	}


//	protected virtual void OnGUI(){
//
//		if ( externalVariableController){
//			int items = actionDictionary.Count * 100;
//			Vector3 coords = mouseClickLocation;
//			coords.x -= 10;
//			coords.y += 10;
//
//			GUILayout.BeginArea(new Rect(coords.x,Screen.height - coords.y,100,items));
//			GUILayout.BeginVertical();
//			foreach (Interaction action in actionDictionary)
//				if( GUILayout.Button(new GUIContent(action.actionName,action.actionName),Toolbox.Instance.actionStyle) ) {}
//
//			GUILayout.EndVertical();
//			GUILayout.EndArea();
//
//			if (Event.current.type == EventType.Repaint && GUI.tooltip != lastToolTip) 
//				lastToolTip = GUI.tooltip;
//				
//		}
//
//		// this weird bit of fuckery avoids pissing off Unity's GUI handling system.
//		// just don't touch it, okay?
//		if (Event.current.type == EventType.Layout)
//		{
//			innerController = 1;
//		}
//		
//		if (Event.current.type == EventType.Repaint)
//		{
//			innerController = 2;
//		}
//		
//		if (innerController == 2)
//		{
//			externalVariableController = clickMenu && actionDictionary.Count > 0;
//			innerController = 0;
//		}
//
//	
//	}
}
