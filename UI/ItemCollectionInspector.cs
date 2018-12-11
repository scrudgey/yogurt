using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionInspector : MonoBehaviour {
    public Transform collectionList;
    public Image sprite;
    public Text descriptionText;
    public Text itemName;
    public StartMenu startMenu;
    public void Initialize(StartMenu menu, GameData data){
        this.startMenu = menu;
        foreach(Transform oldButton in collectionList){
            Destroy(oldButton.gameObject);
        }
        List<string> objects = data.collectedObjects;
        objects.Sort();
        bool initializedText = false;
        foreach (string itemName in objects){
            // create itemCollectionButton
            GameObject entry = GameObject.Instantiate(Resources.Load("UI/ItemCollectionEntry")) as GameObject;
            ItemCollectionButton script = entry.GetComponent<ItemCollectionButton>();
            // set itemCollectionButton inspector value
            script.Configure(this, itemName);
            script.transform.SetParent(collectionList, false);
            if (!initializedText){
                EntryClickedCallback(script);
            }
        }
    }
    public void EntryClickedCallback(ItemCollectionButton script){
        // update icon
        sprite.sprite = script.sprite;
        // update description
        descriptionText.text = script.description;
        itemName.text = script.itemName;
    }
    public void CloseButtonCallback(){
        startMenu.CloseItemCollectionInspector();
    }
}
