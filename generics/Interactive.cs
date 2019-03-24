using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public enum desire { none, accept, decline }
public delegate desire DesireFunction(Personality myPersonality, GameObject requester);

[System.Serializable]
public class Interaction {
    public List<System.Type> parameterTypes;
    public bool debug;
    public Interactive parent;
    public string action;
    public GameObject lastTarget;
    public GameObject defaultTarget;
    public List<object> parameters;
    public List<Component> defaultParameters;
    public bool hideInManualActions;
    public bool hideInRightClickMenu;
    public int defaultPriority;
    public float range = Mathf.Pow(0.35f, 2f);
    public bool limitless = false;
    public bool dontWipeInterface = false;
    public string actionName;
    private bool _validationFunction;
    public bool validationFunction {
        get { return _validationFunction; }
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
    private DesireFunction desireFunction;
    public bool playerOnOtherConsent = true;
    public bool otherOnPlayerConsent = true;
    public bool inertOnPlayerConsent = true;
    public string descString = null;
    public Interaction(Interactive o, string name, string functionName) : this(o, name, functionName, false, false) { }
    public Interaction(Interactive o, string name, string functionName, bool manualHide, bool rightHide) {
        this.parameterTypes = new List<System.Type>();
        this.action = functionName;
        this.actionName = name;
        this.parent = o;
        this.hideInManualActions = manualHide;
        this.hideInRightClickMenu = rightHide;
        methodInfo = parent.GetType().GetMethod(functionName);
        if (methodInfo != null) {
            System.Reflection.ParameterInfo[] pars = methodInfo.GetParameters();
            foreach (System.Reflection.ParameterInfo p in pars) {
                // Debug.Log(actionName + " " + p.ParameterType.ToString());
                parameterTypes.Add(p.ParameterType);
            }
        } else {
            Debug.Log("interaction has failed to find its parent's method");
        }
        descMethodInfo = parent.GetType().GetMethod(functionName + "_desc");
        desireFunction += defaultDesireFunction;
    }
    public Interaction(Interactive o, string name, Action<Component> initAction) {
        this.parameterTypes = new List<System.Type>();
        actionName = name;
        // enabled = false;
        parent = o;
        actionDelegate = initAction;
    }
    // if the code has specified to use a validator function, we need to look up that
    // method and store the reference.
    public void ConfigureValidator() {
        if (validationFunction) {
            validationMethodInfo = parent.GetType().GetMethod(action + "_Validation");
            if (validationMethodInfo == null)
                Debug.Log("interaction validation function was not located.");
        }
    }
    public bool CheckDependency(List<object> targetComponents) {
        parameters = new List<object>();
        bool enabled = true;
        int parameterMatches = 0;
        int parameterMisses = 0;
        // set up the parameter list: check each Type required to be passed, check
        // if any Component in targetComponents is of that type, if so add it to 
        // the parameter list.
        if (debug)
            Debug.Log("Checking the dependency for interaction " + actionName);
        // there's probably a nicer way to do this with linq or something
        foreach (Type requiredType in parameterTypes) {
            bool parameterMatched = false;
            if (debug)
                Debug.Log("Looking for the required argument of type " + requiredType.ToString());
            foreach (object component in targetComponents) {
                if (debug)
                    Debug.Log("Comparing against target component " + component.GetType().ToString());
                if ((component.GetType() == requiredType || component.GetType().IsSubclassOf(requiredType)) &&
                    component != (object)this.parent) {
                    if (debug)
                        Debug.Log("***** MATCH *****");
                    parameterMatched = true;
                    parameterMatches++;
                    parameters.Add(component);
                    break;
                }
            }
            if (parameterMatched == false) {
                parameterMisses++;
            }
        }
        if (parameterMatches == parameterTypes.Count) {
            if (debug)
                Debug.Log("enabled");
            enabled = true;
            // if a validation function is specified, we have to also check to see whether it 
            // is okay with being enabled.
            if (validationFunction) {
                if (debug)
                    Debug.Log("Validating interaction.");
                bool validation = (bool)validationMethodInfo.Invoke(parent, parameters.ToArray());
                if (!validation)
                    enabled = false;
            }
        } else {
            if (debug)
                Debug.Log("disabled");
            enabled = false;
        }
        return enabled;
    }
    public bool IsValid() {
        bool validation = true;
        if (validationFunction) {
            if (validationMethodInfo == null)
                ConfigureValidator();
            if (parameters != null) {
                validation = (bool)validationMethodInfo.Invoke(parent, parameters.ToArray());
            } else {
                validation = (bool)validationMethodInfo.Invoke(parent, null);
            }
        }
        return validation;
    }
    public string Description() {
        if (descString != null) {
            return descString;
        }
        if (descMethodInfo != null) {
            if (parameters != null) {
                return (string)descMethodInfo.Invoke(parent, parameters.ToArray());
            } else {
                return (string)descMethodInfo.Invoke(parent, null);
            }
        } else {
            return "";
        }
    }
    // this can be sped up if I store it in a delegate instead of calling Invoke
    public void DoAction(List<object> customParameters=null) {
        if (customParameters == null){
            customParameters = parameters;
        }
        if (actionDelegate == null) {
            if (parameters != null) {
                methodInfo.Invoke(parent, parameters.ToArray());
            } else {
                methodInfo.Invoke(parent, new object[0]);
            }
        } else {
            actionDelegate(parameters[0] as Component);
        }
    }
    public desire defaultDesireFunction(Personality myPersonality, GameObject requester) {
        if (myPersonality.suggestible == Personality.Suggestible.stubborn) {
            return desire.decline;
        }
        return desire.accept;
    }
    public void AddDesireFunction(DesireFunction df) {
        desireFunction -= defaultDesireFunction;
        desireFunction += df;
    }
    public desire GetDesire(GameObject commandTarget, GameObject requester) {
        DecisionMaker dm = commandTarget.GetComponent<DecisionMaker>();
        return desireFunction(dm.personality, requester);
    }
}
public class Interactive : MonoBehaviour {
    public bool disableInteractions;
    public List<Interaction> interactions = new List<Interaction>();
}
