public class Chela : Item {
    public void Start() {
        Interaction perkAction = new Interaction(this, "Choose Perk...", "OpenPerkMenu");
        interactions.Add(perkAction);
    }
    public void OpenPerkMenu() {
        UINew.Instance.ShowMenu(UINew.MenuType.perk);
    }
    public string OpenPerkMenu_desc() {
        return "Choose Perk";
    }
}
