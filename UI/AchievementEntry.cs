using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AchievementEntry : MonoBehaviour, IPointerDownHandler {
    public Achievement achievement;
    public AchievementBrowser browser;
    public void OnPointerDown(PointerEventData eventData) {
        browser.AchievementEntryCallback(achievement);
    }
}
