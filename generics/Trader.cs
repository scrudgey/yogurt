using UnityEngine;

public class Trader : Interactive {
	public GameObject itemForSale;
	public GameObject itemToBuy;
	private Inventory inv;
	private Awareness aware;
	void Start(){
		inv = GetComponent<Inventory>();
		Interaction tradeAct = new Interaction(this, "Buy...", "Trade");
		interactions.Add(tradeAct);
	}
	// TODO: adjust responses, incorporate nimrod
	public void Trade(Inventory other){
		// item is sold, or missing
		if (itemForSale == null){
			// if (speak)
				Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I have nothing to sell!")); 
			return;
		}
		// player presents an item
		if (other.holding){
			// success
			if (Toolbox.Instance.CloneRemover(other.holding.name) == Toolbox.Instance.CloneRemover(itemToBuy.name)){
				Exchange(other, other.holding);
				// if (speak)
					Toolbox.Instance.SendMessage(other.gameObject, this, new MessageSpeech("I bought it!")); 
				return;
			}
			// holding the wrong item
			// if (speak)
				Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I don't want that!")); 
			return;
		}
		foreach (GameObject item in other.items){
			// player has the item, but it is stashed
			if (Toolbox.Instance.CloneRemover(item.name) == Toolbox.Instance.CloneRemover(itemToBuy.name)){
				// if (speak)
					Toolbox.Instance.SendMessage(other.gameObject, this, new MessageSpeech("Hold on, let me find it...")); 
				return;
			}
		}
		// player does not have the correct item at all
		// if (speak)
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Give me one "+itemToBuy.name)); 
	}
	public enum TradeStatus {none, pass, noItemForTrade, noItemOffered, wrongItemOffered}
	public TradeStatus CheckTradeStatus(Inventory other){
		if (itemForSale == null){
			return TradeStatus.noItemForTrade;
		}
		// player presents an item
		if (other.holding){
			// success
			if (Toolbox.Instance.CloneRemover(other.holding.name) == Toolbox.Instance.CloneRemover(itemToBuy.name)){
				return TradeStatus.pass;
			}
			// holding the wrong item
			return TradeStatus.wrongItemOffered;
		}
		// player does not have the correct item at all
		return TradeStatus.noItemOffered;
	}
	public string Trade_desc(Inventory other){
		return "Trade a "+Toolbox.Instance.CloneRemover(itemToBuy.name)+ " for a "+Toolbox.Instance.CloneRemover(itemForSale.name);
	}
	public void Exchange(Inventory otherInv){
		if (otherInv.holding)
			Exchange(otherInv, otherInv.holding);
	}
	public void Exchange(Inventory otherInv, Pickup other){
		aware = GetComponent<Awareness>();
		if (aware){
			if (aware.possession == itemForSale)
				aware.possession = null;
		}
		otherInv.SoftDropItem();
		Pickup myPickup = itemForSale.GetComponent<Pickup>();
		if (myPickup != null){
			otherInv.GetItem(myPickup);
		}
		inv.GetItem(other);
		if (aware){
			aware.possession = other.gameObject;
		}
		itemForSale = null;
	}
}
