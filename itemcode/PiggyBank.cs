using UnityEngine;

public class PiggyBank : Interactive {
    void Start() {
        Interaction openAct = new Interaction(this, "Open", "Open");
        openAct.descString = "Open piggybank";
        interactions.Add(openAct);
        if (GameManager.Instance.debug)
            SetUpDollar();
        if (GameManager.Instance.data == null)
            return;
        if (GameManager.Instance.data.completeCommercials.Count > 0) {
            SetUpDollar();
        }
    }
    public void SetUpDollar() {
        Gibs dollarGib = gameObject.AddComponent<Gibs>();
        dollarGib.particle = Resources.Load("prefabs/dollar") as GameObject;
        dollarGib.number = 1;
    }
    public void Open() {
        Destructible destructo = GetComponent<Destructible>();
        MessageDamage message = new MessageDamage();
        message.type = damageType.physical;
        message.force = new Vector3(Random.Range(0, 0.1f), Random.Range(0, 0.1f), 0.2f);
        destructo.lastMessage = message;
        destructo.Die();
    }
}
