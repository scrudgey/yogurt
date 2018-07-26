using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum targetType { inert, player, other };
public class Interactor {
    // maybe the most important method, it tells all available actions
    // from the perspective of a player or whatever
    static public HashSet<Interaction> GetInteractions(GameObject focus, GameObject target) {
        HashSet<Interaction> actionDictionary = ReportRightClickActions(target, focus);
        actionDictionary.UnionWith(ReportRightClickActions(focus, target));
        return actionDictionary;
    }
    static HashSet<Interaction> ReportRightClickActions(GameObject targ, GameObject source) {
        HashSet<Interaction> returnDictionary = new HashSet<Interaction>();
        foreach (Interactive interactive in GetInteractorTree(source)) {
            returnDictionary.UnionWith(GetEnabledInteractions(interactive, targ));
        }
        return returnDictionary;
    }
    static HashSet<Interaction> GetEnabledInteractions(Interactive sourceInteractive, GameObject targ) {
        HashSet<Interaction> returnList = new HashSet<Interaction>();
        List<Interactive> targetInteractives = Interactor.GetInteractorTree(targ);
        targetType targType = Interactor.TypeOfTarget(targ);
        targetType sourceType = Interactor.TypeOfTarget(sourceInteractive.gameObject);
        var actives =
            from iBase in targetInteractives
            where iBase.disableInteractions == false
            select iBase;
        foreach (Interaction interaction in sourceInteractive.interactions) {
            if (interaction.debug) {
                Debug.Log("Checking the consent for interaction " + interaction.actionName);
                Debug.Log("target is " + targType.ToString());
                Debug.Log("source is " + sourceType);
                Debug.Log("other on player consent: " + interaction.otherOnPlayerConsent);
                Debug.Log("player on other consent: " + interaction.playerOnOtherConsent);
                Debug.Log("inert on other consent: " + interaction.inertOnPlayerConsent);
            }
            bool enabled = interaction.CheckDependency(new List<Interactive>(actives));
            if (!interaction.playerOnOtherConsent && sourceType == targetType.player && targType == targetType.other) {
                enabled = false;
                continue;
            }
            if (!interaction.otherOnPlayerConsent && sourceType == targetType.other && targType == targetType.player) {
                enabled = false;
                continue;
            }
            if (!interaction.inertOnPlayerConsent && sourceType == targetType.inert && targType == targetType.player) {
                enabled = false;
                continue;
            }
            interaction.CheckDependency(new List<Interactive>(actives));
            if (enabled && !interaction.hideInRightClickMenu && interaction.parameterTypes.Count > 0){
                if (interaction.debug)
                    Debug.Log("interaction enabled : " + enabled);
                returnList.Add(interaction);
            }
            if (interaction.parameterTypes.Count == 0 && interaction.IsValid()) {
                if (interaction.debug)
                    Debug.Log("free action " + interaction.actionName + " enabled free action");
                returnList.Add(interaction);
                enabled = true;
            }
        }
        return returnList;
    }
    static public Interaction GetDefaultAction(HashSet<Interaction> actionList) {
        int highestP = 0;
        Interaction returnInteraction = null;
        foreach (Interaction action in actionList) {
            // removed: action.enabled
            if (action.defaultPriority > highestP) {
                returnInteraction = action;
                highestP = action.defaultPriority;
            }
        }
        return returnInteraction;
    }
    static public List<Interactive> GetInteractorTree(GameObject target) {
        List<Interactive> targetInteractives = new List<Interactive>(target.GetComponents<Interactive>());
        targetType targType = TypeOfTarget(target);
        if (targType == targetType.inert) {
            foreach (Interactive interactive in target.GetComponentsInParent<Interactive>()) {
                if (!targetInteractives.Contains(interactive))
                    targetInteractives.Add(interactive);
            }
            foreach (Interactive interactive in target.GetComponentsInChildren<Interactive>()) {
                if (!targetInteractives.Contains(interactive))
                    targetInteractives.Add(interactive);
            }
            return targetInteractives;
        }
        Inventory targetInv = target.GetComponent<Inventory>();
        if (targetInv != null) {
            if (targetInv.holding != null) {
                targetInteractives.AddRange(targetInv.holding.GetComponents<Interactive>());
            }
        }
        Pickup pickup = target.GetComponent<Pickup>();
        if (pickup != null) {
            if (pickup.holder != null)
                targetInteractives.AddRange(pickup.holder.gameObject.GetComponents<Interactive>());
        }
        Head targetHead = target.GetComponentInChildren<Head>();
        if (targetHead != null) {
            targetInteractives.AddRange(targetHead.gameObject.GetComponents<Interactive>());
        }
        return targetInteractives;
    }
    static public targetType TypeOfTarget(GameObject target) {
        targetType returnType = targetType.inert;
        if (GameManager.Instance.playerObject != null){
            if (GameManager.Instance.playerObject == target)
                returnType = targetType.player;
            if (target.transform.IsChildOf(GameManager.Instance.playerObject.transform)) {
                returnType = targetType.player;
            }
        }
        List<DecisionMaker> AIs = new List<DecisionMaker>(target.GetComponentsInParent<DecisionMaker>());
        foreach (DecisionMaker dm in AIs){
            if (dm.enabled)
                returnType = targetType.other;
        }
        // if we're commanding someone, then categories flip
        if (Controller.Instance.state == Controller.ControlState.commandSelect) {
            if (returnType == targetType.player) {
                returnType = targetType.other;
            } else {
                if (returnType == targetType.other)
                    returnType = targetType.player;
            }
        }
        return returnType;
    }
}
