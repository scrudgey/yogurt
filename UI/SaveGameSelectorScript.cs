using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class SaveGameSelectorScript : MonoBehaviour {
    public Text nameText;
    public Text dateText;
    public Text completionText;
    public Image icon;
    public string saveName;
    public StartMenu startMenu;
    public GameData data;

    void Awake() {
        InitValues();
    }
    public void InitValues() {
        GameObject temp = transform.Find("HeadShot/Icon").gameObject;
        icon = temp.GetComponent<Image>();
        temp = transform.Find("TextPanel/Name").gameObject;
        nameText = temp.GetComponent<Text>();
        temp = transform.Find("TextPanel/Last").gameObject;
        dateText = temp.GetComponent<Text>();
        temp = transform.Find("TextPanel/Time").gameObject;
        completionText = temp.GetComponent<Text>();
    }
    public void Configure(StartMenu startMenu, DirectoryInfo dir, GameData data) {
        if (data == null)
            return;
        this.data = data;
        this.startMenu = startMenu;
        // nameText.text = dir.Name;
        nameText.text = $"{dir.Name} - Day {data.days}";
        saveName = dir.Name;

        TimeSpan t = TimeSpan.FromSeconds(0f);
        if (data != null) {
            t = TimeSpan.FromSeconds(data.secondsPlayed);
            dateText.text = data.saveDate;
        }
        completionText.text = "Completion: " + SaveInspector.Completion(data).ToString("0") + "%";
        if (data.headSpriteSheet != null) {
            Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/" + data.headSpriteSheet);
            icon.sprite = Toolbox.ApplySkinToneToSprite(sprites[0], data.headSkinColor);
        }
    }
    public void Clicked() {
        startMenu.InspectSaveGame(this);
    }

    public void DeleteClicked() {
        // Debug.Log($"detele {saveName}");
        startMenu.DeleteSaveGamePrompt(this);
    }
}
