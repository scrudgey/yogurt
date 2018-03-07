using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NewDayDiaryMenu : MonoBehaviour {
    public Text diaryText;
    public Text itemNum;
    public Text clothingNum;
    public Text foodNum;
    public GameObject commercialsPanel;
    void Start() {
        Time.timeScale = 0f;
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        // diaryText = transform.Find("main/diaryPanel/diaryText").GetComponent<Text>();
        itemNum = transform.Find("main/bottom/itemPanel/item/value").GetComponent<Text>();
        clothingNum = transform.Find("main/bottom/itemPanel/clothing/value").GetComponent<Text>();
        foodNum = transform.Find("main/bottom/itemPanel/food/value").GetComponent<Text>();
        commercialsPanel = transform.Find("main/bottom/rightPanel/panel").gameObject;

        // itemNum.text = GameManager.Instance.data.newCollectedItems.Count.ToString();
        // foodNum.text = GameManager.Instance.data.newCollectedFood.Count.ToString();
        // clothingNum.text = GameManager.Instance.data.newCollectedClothes.Count.ToString();
        itemNum.text = GameManager.Instance.data.itemsCollectedToday.ToString();
        foodNum.text = GameManager.Instance.data.foodCollectedToday.ToString();
        clothingNum.text = GameManager.Instance.data.clothesCollectedToday.ToString();
        GameManager.Instance.data.itemsCollectedToday = 0;
        GameManager.Instance.data.foodCollectedToday = 0;
        GameManager.Instance.data.clothesCollectedToday = 0;
        foreach (Commercial commercial in GameManager.Instance.data.newUnlockedCommercials) {
            GameObject newPanel = GameObject.Instantiate(Resources.Load("UI/panelText")) as GameObject;
            newPanel.GetComponent<Text>().text = commercial.name;
            newPanel.transform.SetParent(commercialsPanel.transform);
            RectTransform rectTransform = newPanel.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
        }
        GameManager.Instance.data.newUnlockedCommercials = new HashSet<Commercial>();
    }

    public void OkayButtonCallback() {
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}
