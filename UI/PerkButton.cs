using UnityEngine;

public class PerkButton : MonoBehaviour {
    public PerkMenu menu;
    public PerkBrowser browser;
    public Perk perk;
    public void Clicked() {
        if (menu)
            menu.PerkButtonClicked(this);
        if (browser)
            browser.PerkButtonClicked(this);
    }
}
