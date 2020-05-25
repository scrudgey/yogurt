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
        // Toolbox.Instance.AudioSpeaker(poofSfx, transform.position);
        UINew.Instance.ShowMenu(UINew.MenuType.perk);
        GameManager.Instance.data.collectedChelas[chelaID] = 1;
        Destroy(gameObject);
        GameObject.Instantiate(poofEffect, transform.position, Quaternion.identity);
    }
    public string OpenPerkMenu_desc() {
        return "Choose Perk";
    }
}
