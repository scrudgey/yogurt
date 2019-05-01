using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AchievementEntry : MonoBehaviour, IPointerDownHandler, ISelectHandler, IDeselectHandler {
    public Achievement achievement;
    public AchievementBrowser browser;
    public Image entryImage;
    public Text entryText;
    public Button entryButton;
    public bool complete;
    public void OnPointerDown(PointerEventData eventData) {
        browser.AchievementEntryCallback(achievement);
    }
    public void Initialize(Achievement achievement, AchievementBrowser browser) {
        this.browser = browser;
        entryButton = GetComponent<Button>();
        entryImage = transform.Find("Image/Image").GetComponent<Image>();
        entryText = transform.Find("Image/Text").GetComponent<Text>();

        entryText.text = achievement.title;
        Sprite icon = Resources.Load<Sprite>("achievements/icons/" + achievement.icon) as Sprite;
        entryImage.sprite = icon;
    }
    public void Complete() {
        ColorBlock cb = entryButton.colors;
        cb.normalColor = new Color(114f / 255f, 185f / 255f, 255f / 255f, 1f);
        entryButton.colors = cb;

        entryText.color = Color.black;
        complete = true;
    }
    public void OnSelect(BaseEventData data) {
        if (complete)
            entryText.color = Color.white;
    }
    public void OnDeselect(BaseEventData data) {
        if (complete)
            entryText.color = Color.black;
    }
}
