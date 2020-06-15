using UnityEngine;
public class Chela : Item {
    public int chelaID;
    public ParticleSystem poofEffect;
    public AudioClip poofSfx;
    public void Start() {
        if (GameManager.Instance.data != null && GameManager.Instance.data.collectedChelas[chelaID] == 1) {
            Destroy(gameObject);
        }
        Interaction perkAction = new Interaction(this, "Choose Perk...", "OpenPerkMenu");
        interactions.Add(perkAction);
    }
    public void OpenPerkMenu() {
        GameObject menu = UINew.Instance.ShowMenu(UINew.MenuType.perk);
        PerkMenu perkMenu = menu.GetComponent<PerkMenu>();
        perkMenu.menuClosed += MenuWasClosed;
    }
    public string OpenPerkMenu_desc() {
        return "Choose Perk";
    }
    public void MenuWasClosed() {
        GameManager.Instance.data.collectedChelas[chelaID] = 1;
        Destroy(gameObject);
        GameObject.Instantiate(poofEffect, transform.position, Quaternion.identity);
    }
}
