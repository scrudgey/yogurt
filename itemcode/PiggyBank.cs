using UnityEngine;

public class PiggyBank : Interactive {
    void Start() {
        Interaction openAct = new Interaction(this, "Open", "Open");
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
        destructo.Die();
    }
}
