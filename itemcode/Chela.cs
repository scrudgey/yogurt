using UnityEngine;
using System.Collections;
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
        perkMenu.chela = this;
        // perkMenu.menuClosed += MenuWasClosed;
    }
    public string OpenPerkMenu_desc() {
        return "Choose Perk";
    }
    void Poof() {
        GameManager.Instance.data.collectedChelas[chelaID] = 1;
        Destroy(gameObject);
        GameObject.Instantiate(poofEffect, transform.position, Quaternion.identity);
    }
    public void Disappear() {
        StartCoroutine(StartPoof());
    }
    IEnumerator StartPoof() {
        yield return new WaitForSeconds(0.01f);
        Poof();
    }
}
