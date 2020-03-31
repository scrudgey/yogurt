using UnityEngine;
public class HomeCloset : Interactive {
    public enum ClosetType { all, items, food, clothing }
    public ClosetType type;
    AnimateUIBubble newBubbleAnimation;
    public void Start() {
        interactions.Add(new Interaction(this, "Open", "OpenCloset"));
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
        if (Controller.Instance.state != Controller.ControlState.cutscene &&
            Controller.Instance.state != Controller.ControlState.inMenu &&
            Controller.Instance.state != Controller.ControlState.waitForMenu
            ) {
            GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.closet);
            ClosetButtonHandler menu = menuObject.GetComponent<ClosetButtonHandler>();
            menu.PopulateItemList(type);
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
}
