﻿using UnityEngine;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class Interaction {
	public List<System.Type> parameterTypes;
	public List<Interactive> targetComponents;
	public bool enabled;
	public bool debug;
	public Interactive parent;
	public string action;
	public List<Component> parameters;

	public bool hideInManualActions;
	public bool hideInRightClickMenu;
	public bool staticInteraction;
	public int defaultPriority;
	public float range = 0.5f;
	public bool limitless = false;
	public bool dontWipeInterface = false;
	public string actionName;
	public string displayVerb;

	private bool _validationFunction;
	public bool validationFunction{
		get {return _validationFunction;}
		set {
			_validationFunction = value;
			ConfigureValidator();
		}
	}

	public Action<Component> actionDelegate;
	public bool continuous;
	
	
	private System.Reflection.MethodInfo methodInfo;
	private System.Reflection.MethodInfo validationMethodInfo;
	private System.Reflection.MethodInfo descMethodInfo;
	
	
	public Interaction (Interactive o, string name, string action) : this(o,name,action,false,false){ }
	
	public Interaction (Interactive o, string name, string action, bool manualHide, bool rightHide){
		
		this.action = action;
		actionName = name;
		displayVerb = actionName;
		enabled = false;
		parent = o;
		methodInfo = parent.GetType().GetMethod(action);
		parameterTypes = new List<System.Type> ();
		hideInManualActions = manualHide;
		hideInRightClickMenu = rightHide;
		
		if (methodInfo != null){
			System.Reflection.ParameterInfo[] pars = methodInfo.GetParameters();
			foreach( System.Reflection.ParameterInfo p in pars){
				parameterTypes.Add( p.ParameterType);
			}
		} else {
			Debug.Log("interaction has failed to find its parent's method");
		}
		descMethodInfo = parent.GetType().GetMethod(action+"_desc");	
	}
	
	public Interaction (Interactive o, string name, Action<Component> initAction){
		actionName = name;
		enabled = false;
		parent = o;
		actionDelegate = initAction;
	}
	
	// if the code has specified to use a validator function, we need to look up that
	// method and store the reference.
	public void ConfigureValidator(){
		if (validationFunction){
			validationMethodInfo = parent.GetType().GetMethod(action+"_Validation");		
			if (validationMethodInfo == null)
				Debug.Log("interaction validation function was not located.");
		}
	}
	
	
	public void CheckDependency(){
		parameters = new List<Component> ();
		int parameterMatches = 0;
		int parameterMisses = 0;
		
		// set up the parameter list: check each Type required to be passed, check
		// if any Component in targetComponents is of that type, if so add it to 
		// the parameter list.
		if (debug)
			Debug.Log("Checking the dependency for interaction "+actionName);
		// there's probably a nicer way to do this with linq or something
		// for (int i =0; i < parameterTypes.Count; i++){
		foreach (Type requiredType in parameterTypes){
			bool parameterMatched = false;
			if (debug)
				Debug.Log("Looking for the required argument of type "+requiredType.ToString());
			// for (int ii=0; ii < targetComponents.Count; ii++){
			foreach (Component component in targetComponents){
				if (debug)
					Debug.Log("Comparing against target component "+component.GetType().ToString());
				if ( (component.GetType() == requiredType || component.GetType().IsSubclassOf(requiredType)) &&
				    component != this.parent){
						if (debug)
							Debug.Log("***** MATCH *****");
					parameterMatched=true;
					parameterMatches++;
					parameters.Add(component);
				}
			}
			if (parameterMatched == false){
				parameterMisses++;
			}
		}
		
		if (parameterMatches == parameterTypes.Count){
			enabled = true;
			
			// if a validation function is specified, we have to also check to see whether it 
			// is okay with being enabled.
			if (validationFunction){
				if (debug)
					Debug.Log("Validating interaction.");
				bool validation = (bool)validationMethodInfo.Invoke(parent, parameters.ToArray());
				if (!validation)
					enabled = false;
			}
		} else {
			enabled = false;
		}
	}
	
	public bool IsValid(){
		bool validation = true;
		if (validationFunction){
				if (validationMethodInfo == null)
					ConfigureValidator();
				if (parameters != null){
					validation = (bool)validationMethodInfo.Invoke(parent, parameters.ToArray());
				} else {
					validation = (bool)validationMethodInfo.Invoke(parent, null);
				}
		}
		return validation;
	}

	public string Description(){
		if (descMethodInfo != null){
			if (parameters != null){
					return (string)descMethodInfo.Invoke(parent, parameters.ToArray());
				} else {
					return (string)descMethodInfo.Invoke(parent, null);
				}
		} else {
			return "";
		}
	}
	
	// this can be sped up if I store it in a delegate instead of calling Invoke
	// public Occurrence DoAction(){
	// 	Occurrence result = null;
	// 	if (enabled){
	// 		if (actionDelegate == null){
	// 			if (parameters != null ){
	// 				methodInfo.Invoke(parent, parameters.ToArray());
	// 			} else {
	// 				methodInfo.Invoke(parent, new object[0]);
	// 			}
	// 		} else {
	// 			actionDelegate(parameters[0] as Component);
	// 		}
	// 		GameObject occurrenceObject = GameObject.Instantiate(Resources.Load("OccurrenceFlag"), parent.transform.position, Quaternion.identity) as GameObject;
	// 		result = occurrenceObject.GetComponent<Occurrence>();
	// 		result.functionName = this.actionName;
	// 		result.subjectName = parent.name;
	// 		result.actor = parent.gameObject;
	// 		result.target = parent.target;
            
	// 		foreach (Component parameter in parameters){
	// 			result.parameters.Add(parameter);
    //             result.parameterStrings.Add(parameter.ToString());
	// 		}
	// 	}
	// 	return result;
	// }
	public void DoAction(){
		if (enabled){
			if (actionDelegate == null){
				if (parameters != null ){
					methodInfo.Invoke(parent, parameters.ToArray());
				} else {
					methodInfo.Invoke(parent, new object[0]);
				}
			} else {
				actionDelegate(parameters[0] as Component);
			}
		}
	}
	
}


public class Interactive : MonoBehaviour{

	private GameObject _target;
	public GameObject target{
		get{return _target;}
		set{
			_target = value;
			targetUpdate();
		}
	}

	public bool disableInteractions;
	public List<Interaction> interactions = new List<Interaction>();

	public List<Interaction> GetEnabledActions(){
		List<Interaction> returnList = new List<Interaction>();
		foreach (Interaction interaction in interactions){
			if (interaction.enabled && interaction.parameterTypes.Count > 0)
				returnList.Add(interaction);
		}
		return returnList;
	}

	public List<Interaction> GetRightClickActions(){
		List<Interaction> returnList = new List<Interaction> ();
		foreach (Interaction interaction in interactions){
			if (interaction.enabled && !interaction.hideInRightClickMenu && interaction.parameterTypes.Count > 0)
				returnList.Add(interaction);
		}
		return returnList;
	}

	public List<Interaction> GetManualActions(){
		List<Interaction> returnList = new List<Interaction> ();
		foreach (Interaction interaction in interactions){
			if (interaction.enabled && !interaction.hideInManualActions)
				returnList.Add(interaction);
		}
		return returnList;	
	}

	public List<Interaction> GetFreeActions(){
		List<Interaction> returnList = new List<Interaction> ();
		foreach (Interaction interaction in interactions){
			if (interaction.parameterTypes.Count == 0 && interaction.IsValid()){
				returnList.Add(interaction);
				interaction.enabled = true;
			}
		}
		return returnList;
	}

	public void CallAction(string requestAction, bool continuous = false){
		Interaction doThis = null;
		// this logic might be tuned up a bit- checking versus duplicate action names
		foreach (Interaction interaction in interactions){
			if (interaction.actionName == requestAction && interaction.enabled)
				doThis = interaction;
		}
		// find a smarter way to do a distance calculation
		if ( doThis != null  ){
			doThis.DoAction();
		}

	}


	public Interaction GetAction(string requestAction){
		Interaction returnAction = null;
		// this logic might be tuned up a bit- checking versus duplicate action names
		foreach (Interaction interaction in interactions){
			if (interaction.actionName == requestAction )
				returnAction = interaction;
		}

		return returnAction;
	}

	private void targetUpdate(){
		// this is what i will have to update: change from monobehavior to InteractiveBase
		// and take only the enabled ones
		Interactive[] components = target.GetComponentsInChildren<Interactive>();

		var actives = 
			from iBase in components
			where iBase.disableInteractions == false
			select iBase;

		foreach (Interaction interaction in interactions){
			if (!interaction.staticInteraction ){
				interaction.targetComponents = new List<Interactive>(actives);
				interaction.CheckDependency();
			}
		}
	}

	public Interaction ReportHighestPriority(){
		Interaction returnInteraction = null;
		int highestP = 0;

		foreach (Interaction interaction in interactions){
			if (interaction.defaultPriority > highestP && interaction.enabled){
				returnInteraction = interaction;
				highestP = interaction.defaultPriority;
			}
		}

		return returnInteraction;
	}

}
