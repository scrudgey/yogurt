using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// using UnityEngine.UI

public class NewDayDiaryMenu : MonoBehaviour {
    public Text itemNum;
    public Text clothingNum;
    public Text foodNum;
    public GameObject commercialsPanel;
    public GameObject topItemPanel;
    public Image topItemIcon;
    public Text topItemText;
    void Start() {
        Time.timeScale = 0f;
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        itemNum.text = GameManager.Instance.data.itemsCollectedToday.ToString();
        foodNum.text = GameManager.Instance.data.foodCollectedToday.ToString();
        clothingNum.text = GameManager.Instance.data.clothesCollectedToday.ToString();

        // set up top item
        if (GameManager.Instance.data.itemsCollectedToday > 0) {
            ConfigureTopItem();
        } else {
            topItemPanel.SetActive(false);
        }

        GameManager.Instance.data.itemsCollectedToday = 0;
        GameManager.Instance.data.foodCollectedToday = 0;
        GameManager.Instance.data.clothesCollectedToday = 0;
        foreach (Commercial commercial in GameManager.Instance.data.newUnlockedCommercials) {
            GameObject newPanel = GameObject.Instantiate(Resources.Load("UI/panelText")) as GameObject;
            newPanel.GetComponent<Text>().text = commercial.name;
            newPanel.transform.SetParent(commercialsPanel.transform);
            RectTransform rectTransform = newPanel.GetComponent<RectTransform>();
            // rectTransform.localScale = Vector3.one;
            Vector3 pos = rectTransform.localPosition;
            pos.z = 0;
            rectTransform.localPosition = pos;
        }
        GameManager.Instance.data.newUnlockedCommercials = new HashSet<Commercial>();

    }

    void ConfigureTopItem() {
        string itemName = GameManager.Instance.data.newCollectedItems[Random.Range(0, GameManager.Instance.data.newCollectedItems.Count)];

        GameObject item = Resources.Load("prefabs/" + itemName) as GameObject;

        Item itemComponent = item.GetComponent<Item>();
        Pickup itemPickup = item.GetComponent<Pickup>();
        SpriteRenderer itemRenderer = item.GetComponent<SpriteRenderer>();
        if (itemComponent != null) {
            itemName = itemComponent.itemName;
        } else itemName = Toolbox.Instance.GetName(item);
        topItemText.text = itemName;

        if (itemPickup != null && itemPickup.icon != null) {
            topItemIcon.sprite = itemPickup.icon;
            // TODO: someday, use c# 6 null-conditional or monads
            Transform balloon = item.transform.Find("balloon");
            if (balloon != null) {
                SpriteRenderer balloonRenderer = balloon.GetComponent<SpriteRenderer>();
                if (balloonRenderer != null) {
                    topItemIcon.color = balloonRenderer.color;
                }
            }
        } else topItemIcon.sprite = itemRenderer.sprite;

        // Destroy(item);
    }

    public void OkayButtonCallback() {
        Time.timeScale = 1f;
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
}
