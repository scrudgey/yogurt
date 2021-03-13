using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public enum TargetType { self, other, holding };
public class Interactor {
    static public HashSet<InteractionParam> SelfOnOtherInteractions(GameObject focus, GameObject target) {
        HashSet<InteractionParam> actionDictionary = GetAllEnabledInteractions(target, focus, TargetType.other, TargetType.self);
        foreach (InteractionParam ip in GetAllEnabledInteractions(focus, target, TargetType.self, TargetType.other))
            actionDictionary.Add(ip);

        Inventory focusInv = focus.GetComponent<Inventory>();
        if (focusInv != null && focusInv.holding != null) {
            foreach (InteractionParam ip in GetAllEnabledInteractions(target, focusInv.holding.gameObject, TargetType.other, TargetType.holding))
                actionDictionary.Add(ip);
            foreach (InteractionParam ip in GetAllEnabledInteractions(focusInv.holding.gameObject, target, TargetType.holding, TargetType.other))
                actionDictionary.Add(ip);
        }
        // foreach (InteractionParam ip in actionDictionary) {
        //     if (ip.parameters != null) {
        //         Debug.Log(ip.interaction.actionName + " " + String.Join(",", ip.parameters.ToArray()));
        //     } else {
        //         Debug.Log(ip.interaction.actionName + " " + ip.interaction.parent.ToString());
        //     }
        // }
        return actionDictionary;
    }
    static public HashSet<InteractionParam> SelfOnHoldingInteractions(GameObject focus) {
        Inventory inv = focus.GetComponent<Inventory>();
        if (inv && inv.holding) {
            HashSet<InteractionParam> actionDictionary = GetAllEnabledInteractions(inv.holding.gameObject, focus, TargetType.other, TargetType.self);
            foreach (InteractionParam ip in GetAllEnabledInteractions(focus, inv.holding.gameObject, TargetType.self, TargetType.other))
                actionDictionary.Add(ip);
            return actionDictionary;
        } else {
            return new HashSet<InteractionParam>();
        }
    }
    static public HashSet<InteractionParam> SelfOnSelfInteractions(GameObject focus) {
        HashSet<InteractionParam> actionDictionary = GetAllEnabledInteractions(focus, focus, TargetType.self, TargetType.self);
        Inventory inv = focus.GetComponent<Inventory>();
        if (inv && inv.holding) {
            foreach (InteractionParam ip in GetAllEnabledInteractions(inv.holding.gameObject, focus, TargetType.self, TargetType.self))
                actionDictionary.Add(ip);
            foreach (InteractionParam ip in GetAllEnabledInteractions(focus, inv.holding.gameObject, TargetType.self, TargetType.self))
                actionDictionary.Add(ip);
        }
        return actionDictionary;
    }

    private static HashSet<InteractionParam> GetAllEnabledInteractions(GameObject targ, GameObject source, TargetType targType, TargetType sourceType) {
        HashSet<InteractionParam> returnDictionary = new HashSet<InteractionParam>();
        // get source full interactor tree
        foreach (Interactive interactive in GetInteractorTree(source, sourceType)) {

            // compare with target full interactor tree
            foreach (InteractionParam ip in GetEnabledInteractions(interactive, targ, targType, sourceType))
                returnDictionary.Add(ip);
        }
        return returnDictionary;
    }
    static HashSet<InteractionParam> GetEnabledInteractions(Interactive sourceInteractive, GameObject targ, TargetType targType, TargetType sourceType) {
        HashSet<InteractionParam> returnList = new HashSet<InteractionParam>();
        if (sourceInteractive.disableInteractions)
            return returnList;
        List<Interactive> targetInteractives = Interactor.GetInteractorTree(targ, targType);
        var actives =
            from iBase in targetInteractives
            where iBase.disableInteractions == false
            select iBase;
        List<object> objectParams = new List<object>();
        foreach (Interactive interactive in actives)
            objectParams.Add((object)interactive);
        objectParams.Add(targ);
        foreach (Interaction interaction in sourceInteractive.interactions) {
            if (interaction.debug) {
                Debug.Log("Checking the consent for interaction " + interaction.actionName);
                Debug.Log("source is " + sourceType);
                Debug.Log("target is " + targType.ToString());
                Debug.Log("other on self consent: " + interaction.otherOnSelfConsent);
                Debug.Log("self on other consent: " + interaction.selfOnOtherConsent);
                Debug.Log("self on self consent: " + interaction.selfOnSelfConsent);
                Debug.Log("holding on other consent: " + interaction.holdingOnOtherConsent);
            }
            bool enabled = true;
            // source on target
            if (!interaction.selfOnOtherConsent && sourceType == TargetType.self && targType == TargetType.other) {
                enabled = false;
                continue;
            }
            if (!interaction.otherOnSelfConsent && sourceType == TargetType.other && targType == TargetType.self) {
                enabled = false;
                continue;
            }
            if (!interaction.selfOnSelfConsent && sourceType == TargetType.self && targType == TargetType.self) {
                enabled = false;
                continue;
            }
            // source is other, target is holding
            if (sourceType == TargetType.holding && targType == TargetType.other) {
                if (!interaction.holdingOnOtherConsent || (interaction.holdingOnOtherConsent && !interaction.selfOnOtherConsent)) {
                    enabled = false;
                    continue;
                }
            }
            if (targType == TargetType.holding && !interaction.holdingOnOtherConsent) {
                enabled = false;
                continue;
            }
            List<object> parameters = interaction.CheckDependency(objectParams);
            enabled = parameters != null;
            if (enabled && interaction.parameterTypes.Count > 0) {
                if (interaction.debug)
                    Debug.Log("interaction enabled : " + enabled);
                returnList.Add(new InteractionParam(interaction, parameters));
            }
            if (interaction.parameterTypes.Count == 0 && interaction.IsValid(new List<object>())) {
                if (interaction.debug)
                    Debug.Log("free action " + interaction.actionName + " enabled free action");
                returnList.Add(new InteractionParam(interaction, null));
                enabled = true;
            }
        }
        return returnList;
    }
    static public InteractionParam GetDefaultAction(HashSet<InteractionParam> interactions) {
        int highestP = 0;
        InteractionParam returnInteraction = null;
        foreach (InteractionParam ip in interactions) {
            // removed: action.enabled
            // Debug.Log(action.actionName + ": " + action.defaultPriority.ToString());
            if (ip.interaction.defaultPriority > highestP) {
                returnInteraction = ip;
                highestP = ip.interaction.defaultPriority;
            }
        }
        return returnInteraction;
    }
    static public List<Interactive> GetInteractorTree(GameObject target, TargetType targType) {
        List<Interactive> targetInteractives = new List<Interactive>(target.GetComponents<Interactive>());
        Head targetHead = target.GetComponentInChildren<Head>();
        if (targetHead != null) {
            targetInteractives.AddRange(targetHead.gameObject.GetComponents<Interactive>());
        }
        return targetInteractives;
    }
}
