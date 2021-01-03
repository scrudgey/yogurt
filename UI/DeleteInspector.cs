using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DeleteInspector : MonoBehaviour {
    public Text gameNameText;
    public Text lastPlayedText;
    public Text totalTimeText;
    public Text completionText;
    public Image headShot;
    public StartMenu startMenu;
    public SaveGameSelectorScript data;
    public Text promptText;
    public Text buttonText;
    public void Initialize(StartMenu menu, SaveGameSelectorScript save) {
        this.startMenu = menu;
        this.data = save;
        gameNameText.text = save.saveName;

        TimeSpan t = TimeSpan.FromSeconds(0f);
        if (data != null) {
            t = TimeSpan.FromSeconds(save.data.secondsPlayed);
            lastPlayedText.text = "Last played: " + save.data.saveDate.ToString();
        }
        totalTimeText.text = "Total time: " + string.Format("{0:D2}:{1:D2}:{2:D2}s",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds).ToString();

        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/" + save.data.headSpriteSheet);
        headShot.sprite = Toolbox.ApplySkinToneToSprite(sprites[0], save.data.headSkinColor);
        completionText.text = "Completion: " + SaveInspector.Completion(save.data).ToString("0") + "%";

        promptText.text = $"Really delete {save.saveName}?";
        buttonText.text = $"Yes, delete {save.saveName}";
    }
    public void CancelCallback() {
        startMenu.LoadButton();
    }
    public void DeleteCallback() {
        startMenu.DeleteGame(data);
    }
}
