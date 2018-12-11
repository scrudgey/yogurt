using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class SaveGameSelectorScript : MonoBehaviour {
    public Text nameText;
    public Text dateText;
    public Text timeText;
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
        timeText = temp.GetComponent<Text>();
    }
    public void Configure(StartMenu startMenu, DirectoryInfo dir, GameData data){
        this.data = data;
        this.startMenu = startMenu;
        nameText.text = dir.Name;
        saveName = dir.Name;
        
        TimeSpan t = TimeSpan.FromSeconds(0f);
        if (data != null) {
            t = TimeSpan.FromSeconds(data.secondsPlayed);
            dateText.text = data.saveDate;
        }
        timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}s",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds).ToString();
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/" + data.headSpriteSheet);
        icon.sprite = sprites[0];
    }
    public void Clicked() {
        startMenu.InspectSaveGame(this);
    }
}
