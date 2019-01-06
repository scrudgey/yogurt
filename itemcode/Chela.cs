using UnityEngine;
public class Chela : Item {
    public int chelaID;
    public void Start() {
        if (GameManager.Instance.data != null && GameManager.Instance.data.collectedChelas[chelaID] == 1){
            Destroy(gameObject);
        }
        Interaction perkAction = new Interaction(this, "Choose Perk...", "OpenPerkMenu");
        interactions.Add(perkAction);
    }
    public void OpenPerkMenu() {
        UINew.Instance.ShowMenu(UINew.MenuType.perk);
        GameManager.Instance.data.collectedChelas[chelaID] = 1;
        Destroy(gameObject);
        // TODO: destroy effect upon close?
    }
    public string OpenPerkMenu_desc() {
        return "Choose Perk";
    }
}
