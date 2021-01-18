using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlushieMachine : Item {
    public AnimateUIBubble newBubble;
    void Start() {
        if (GameManager.Instance.data != null && !GameManager.Instance.data.perks["beverage"]) {
            gameObject.SetActive(false);
        }

        Interaction power = new Interaction(this, "Browse drinks...", "Power");
        power.descString = "Browse drinks";
        interactions.Add(power);

        if (newBubble == null)
            newBubble = GetComponent<AnimateUIBubble>();
        newBubble.DisableFrames();
        CheckBubble();
    }
    public void CheckBubble() {
        bool activeBubble = false;

        if (GameManager.Instance.data == null)
            return;
        if (GameManager.Instance.data.newCollectedLiquids.Count > 0)
            activeBubble = true;

        if (activeBubble) {
            newBubble.EnableFrames();
        } else {
            newBubble.DisableFrames();
        }
    }
    public void Power() {
        UINew.Instance.RefreshUI(active: false);
        GameObject menu = UINew.Instance.ShowMenu(UINew.MenuType.beverageMenu);
        BeverageMenu bevMenu = menu.GetComponent<BeverageMenu>();
        bevMenu.PopulateItemList();
        CheckBubble();
    }
}
