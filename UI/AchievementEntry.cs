using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementEntry : MonoBehaviour {
    public Achievement achievement;
    public AchievementBrowser browser;
    public void ButtonClickedCallback(){
        browser.AchievementEntryCallback(achievement);
    }
}
