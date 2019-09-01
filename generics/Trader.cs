using UnityEngine;

public class Trader : Interactive, ISaveable {
    public GameObject give;
    public string receive;
    private Inventory inv;
    private Awareness aware;
    void Start() {
        inv = GetComponent<Inventory>();
        Interaction tradeAct = new Interaction(this, "Buy...", "Trade");
        tradeAct.hideInManualActions = true;
        tradeAct.playerOnOtherConsent = false;
        interactions.Add(tradeAct);
    }
    // TODO: adjust responses, incorporate nimrod
    public void Trade(Inventory other) {
        // item is sold, or missing
        if (give == null) {
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I have nothing to sell!"));
            return;
        }
        // player presents an item
        if (other.holding) {
            // success
            if (Toolbox.Instance.CloneRemover(other.holding.name) == receive) {
                Exchange(other, other.holding);
                Toolbox.Instance.SendMessage(other.gameObject, this, new MessageSpeech("I bought it!"));
                return;
            }
            // holding the wrong item
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I don't want that!"));
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
        Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Give me one " + receive));
    }
    public enum TradeStatus { none, pass, noItemForTrade, noItemOffered, wrongItemOffered }
    public TradeStatus CheckTradeStatus(Inventory other) {
        if (give == null) {
            return TradeStatus.noItemForTrade;
        }
        // player presents an item
        if (other.holding) {
            // success
            if (Toolbox.Instance.CloneRemover(other.holding.name) == receive) {
                return TradeStatus.pass;
            }
            // holding the wrong item
            return TradeStatus.wrongItemOffered;
        }
        // player does not have the correct item at all
        return TradeStatus.noItemOffered;
    }
    public string Trade_desc(Inventory other) {
        if (give != null) {
            return "Trade a " + Toolbox.Instance.CloneRemover(receive) + " for a " + Toolbox.Instance.CloneRemover(give.name);
        } else {
            return "Attempt barter";
        }
    }
    public void Exchange(Inventory otherInv) {
        if (otherInv.holding)
            Exchange(otherInv, otherInv.holding);
    }
    public void Exchange(Inventory otherInv, Pickup other) {
        aware = GetComponent<Awareness>();
        if (aware) {
            if (aware.possession == give)
                aware.possession = null;
        }
        otherInv.SoftDropItem();
        Pickup myPickup = give.GetComponent<Pickup>();
        if (myPickup != null) {
            otherInv.GetItem(myPickup);
        }
        inv.GetItem(other);
        if (aware) {
            aware.possession = other.gameObject;
        }
        give = null;
    }
    public void SaveData(PersistentComponent data) {
        if (give != null) {
            MySaver.UpdateGameObjectReference(give, data, "give");
            // MySaver.AddToReferenceTree(data.id, give);
        }
        data.strings["receive"] = receive;
    }
    public void LoadData(PersistentComponent data) {
        if (data.ints.ContainsKey("give")) {
            give = MySaver.IDToGameObject(data.ints["give"]);
        }
        receive = data.strings["receive"];
    }
}
