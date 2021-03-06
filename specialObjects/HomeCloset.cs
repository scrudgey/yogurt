﻿using UnityEngine;
public class HomeCloset : Interactive {
    public enum ClosetType { all, items, food, clothing }
    public static readonly string prefsKey_ClosetMenuType;
    public ClosetType type;
    AnimateUIBubble newBubbleAnimation;
    public void Start() {
        Interaction act = new Interaction(this, "Open", "OpenCloset");
        act.holdingOnOtherConsent = false;
        // act.otherOnSelfConsent = false;
        interactions.Add(act);

        Interaction stashAct = new Interaction(this, "Stash", "StashObject");
        stashAct.validationFunction = true;
        // act.holdingOnOtherConsent = false;
        // act.otherOnSelfConsent = false;
        interactions.Add(stashAct);

        Interaction stashHatAct = new Interaction(this, "Remove Hat", "StashHat");
        stashHatAct.validationFunction = true;
        interactions.Add(stashHatAct);

        newBubbleAnimation = GetComponent<AnimateUIBubble>();
        CheckBubble();
    }
    public void InitBubble() {
        newBubbleAnimation = GetComponent<AnimateUIBubble>();
    }
    public void CheckBubble() {
        bool activeBubble = false;
        GameManager.Instance.closetHasNew.TryGetValue(type, out activeBubble);
        if (!activeBubble)
            newBubbleAnimation.DisableFrames();
    }
    public void OpenCloset() {
        if (InputController.Instance.state != InputController.ControlState.cutscene &&
            InputController.Instance.state != InputController.ControlState.inMenu &&
            InputController.Instance.state != InputController.ControlState.waitForMenu
            ) {

            var menuType = PlayerPrefs.GetString(prefsKey_ClosetMenuType, "simple");
            UINew.Instance.RefreshUI(active: false);
            if (menuType == "simple" || type == HomeCloset.ClosetType.clothing || type == HomeCloset.ClosetType.food) {
                GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.closet);
                ClosetButtonHandler menu = menuObject.GetComponent<ClosetButtonHandler>();
                menu.PopulateItemList(type, this);
            } else if (menuType == "advanced") {
                GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.loadoutEditor);
                LoadoutEditor menu = menuObject.GetComponent<LoadoutEditor>();
                menu.Configure(this);
            }
            GameManager.Instance.DetermineClosetNews();
            CheckBubble();
        }
    }

    public string OpenCloset_desc() {
        switch (type) {
            case ClosetType.all:
                return "Browse items";
            case ClosetType.items:
                return "Browse closet";
            case ClosetType.food:
                return "Browse refrigerator";
            case ClosetType.clothing:
                return "Browse dresser";
            default:
                return "Browse closet";
        }
    }
    public void StashObject(Pickup pickup) {
        ClaimsManager.Instance.WasDestroyed(pickup.gameObject);
        // if (pickup.holder) {
        //     pickup.holder.SoftDropItem();
        // }
        Destroy(pickup.gameObject);
        string prefabName = Toolbox.Instance.CloneRemover(pickup.gameObject.name);
        GameManager.Instance.data.itemCheckedOut[prefabName] = false;
    }
    public string StashObject_desc(Pickup pickup) {
        string itemName = Toolbox.Instance.GetName(pickup.gameObject);
        switch (type) {
            case ClosetType.all:
                return $"Put {itemName} in closet";
            case ClosetType.items:
                return $"Put {itemName} in closet";
            case ClosetType.food:
                return $"Put {itemName} in refrigerator";
            case ClosetType.clothing:
                return $"Put {itemName} in dresser";
            default:
                return $"Put {itemName} away";
        }
    }
    public bool StashObject_Validation(Pickup pickup) {
        switch (type) {
            case ClosetType.all:
                return true;
            case ClosetType.items:
                return true;
            case ClosetType.food:
                Edible edible = pickup.GetComponent<Edible>();
                if (edible != null) {
                    return !edible.inedible;
                } else return false;
            case ClosetType.clothing:
                return pickup.GetComponent<Uniform>() != null;
            default:
                return true;
        }
    }


    public void StashHat(Head head) {
        GameObject hat = head.RemoveHat().gameObject;
        ClaimsManager.Instance.WasDestroyed(hat);
        string prefabName = Toolbox.Instance.CloneRemover(hat.name);
        GameManager.Instance.data.itemCheckedOut[prefabName] = false;
        Destroy(hat);
    }
    public string StashHat_desc(Head head) {
        if (head.hat == null)
            return "stash hat";
        string itemName = Toolbox.Instance.GetName(head.hat.gameObject);
        switch (type) {
            case ClosetType.all:
                return $"Put {itemName} in closet";
            case ClosetType.items:
                return $"Put {itemName} in closet";
            case ClosetType.food:
                return $"Put {itemName} in refrigerator";
            case ClosetType.clothing:
                return $"Put {itemName} in dresser";
            default:
                return $"Put {itemName} away";
        }
    }
    public bool StashHat_Validation(Head head) {
        if (head.hat == null)
            return false;
        switch (type) {
            case ClosetType.all:
                return true;
            case ClosetType.items:
                return true;
            case ClosetType.food:
                return false;
            case ClosetType.clothing:
                return true;
            default:
                return true;
        }
    }
}
