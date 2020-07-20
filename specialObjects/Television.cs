using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Television : Item {
    public AnimateUIBubble newBubble;
    void Start() {
        Interaction power = new Interaction(this, "Watch TV...", "Power");
        power.descString = "Watch TV";
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
        if (GameManager.Instance.data.newTelevisionShows.Count > 0)
            activeBubble = true;

        if (activeBubble) {
            newBubble.EnableFrames();
        } else {
            newBubble.DisableFrames();
        }
    }
    public void Power() {
        GameObject menu = UINew.Instance.ShowMenu(UINew.MenuType.tv);
        menu.GetComponent<TVMenu>().television = this;
    }
}
