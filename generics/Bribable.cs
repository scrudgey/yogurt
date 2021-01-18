using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

public class Bribable : Interactive {
    public string receive = "dollar";
    private Inventory inv;
    void Start() {
        inv = GetComponent<Inventory>();
        Interaction tradeAct = new Interaction(this, "Bribe", "Bribe");
        tradeAct.selfOnOtherConsent = false;
        tradeAct.selfOnSelfConsent = false;
        tradeAct.validationFunction = true;
        interactions.Add(tradeAct);
    }
    public bool Bribe_Validation(Inventory other) {
        return other.holding != null && Toolbox.Instance.CloneRemover(other.holding.name) == receive;
    }
    public void Bribe(Inventory other) {

        // player presents an item
        if (other.holding) {
            // success
            if (Toolbox.Instance.CloneRemover(other.holding.name) == receive) {
                Exchange(other, other.holding);
                Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I'll see what I can do."));
                return;
            }
            // holding the wrong item
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Don't insult me with that!"));
            return;
        }
        foreach (GameObject item in other.items) {
            // player has the item, but it is stashed
            if (Toolbox.Instance.CloneRemover(item.name) == Toolbox.Instance.CloneRemover(receive)) {
                Toolbox.Instance.SendMessage(other.gameObject, this, new MessageSpeech("Hold on, let me find it..."));
                return;
            }
        }
        // player does not have the correct item at all
        Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Just sweeten the deal a little."));
    }

    public string Trade_desc(Inventory other) {
        return "Attempt a bribe";
    }

    public void Exchange(Inventory otherInv, Pickup other) {
        otherInv.SoftDropItem();
        inv.GetItem(other);
        inv.GetItem(other);

        Awareness awareness = gameObject.GetComponent<Awareness>();
        if (awareness != null) {
            PersonalAssessment pa = awareness.FormPersonalAssessment(otherInv.gameObject);
            pa.status = PersonalAssessment.friendStatus.friend;

            // null out protection zones
            awareness.protectZone = null;
        }

        DecisionMaker ai = gameObject.GetComponent<DecisionMaker>();
        if (ai != null) {
            if (ai.defaultPriorityType == DecisionMaker.PriorityType.ProtectZone) {
                ai.defaultPriorityType = DecisionMaker.PriorityType.Wander;
                foreach (Priority priority in ai.priorities) {
                    if (priority.GetType() == typeof(PriorityWander)) {
                        ai.defaultPriority = priority;
                    }
                }
            }
        }
    }
}
