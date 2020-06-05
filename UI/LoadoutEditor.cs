using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LoadoutEditor : MonoBehaviour {
    public Transform itemCollection;
    public Transform loadoutCollection;
    public Transform buffs;

    public Text clothesText;
    public Text hatText;

    public Button inventoryButton;
    public Button clothesButton;
    public Button hatButton;
    public UIButtonEffects effects;

    public Image icon;
    public Text nameText;
    public Text descriptionText;
    public HomeCloset.ClosetType closetType;

    private ItemEntryScript lastClicked;
    public List<ItemEntryScript> loadoutItems;
    public ItemEntryScript loadoutClothes;
    public ItemEntryScript loadoutHat;

    private Dictionary<ItemEntryScript, GameObject> itemScripts = new Dictionary<ItemEntryScript, GameObject>();
    private Dictionary<LoadoutEntryScript, GameObject> loadoutScripts = new Dictionary<LoadoutEntryScript, GameObject>();

    private ItemEntryScript spawnEntry(string name) {
        GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
        ItemEntryScript script = newObject.GetComponent<ItemEntryScript>();
        script.Configure(name, HomeCloset.ClosetType.all);
        script.transform.SetParent(itemCollection, false);
        itemScripts[script] = newObject;
        return script;
    }
    private LoadoutEntryScript spawnLoadoutScript(ItemEntryScript itemScript) {
        GameObject newObject = Instantiate(Resources.Load("UI/LoadoutItem")) as GameObject;
        LoadoutEntryScript script = newObject.GetComponent<LoadoutEntryScript>();
        script.Configure(itemScript);
        script.transform.SetParent(loadoutCollection, false);
        script.loadoutEditor = this;
        loadoutScripts[script] = newObject;
        return script;
    }

    public void Configure(HomeCloset.ClosetType type) {
        closetType = type;

        effects = GetComponent<UIButtonEffects>();

        foreach (Transform child in itemCollection) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in loadoutCollection) {
            Destroy(child.gameObject);
        }
        inventoryButton.interactable = false;
        clothesButton.interactable = false;
        hatButton.interactable = false;

        ClearBuffs();

        List<string> itemList = GameManager.Instance.data.collectedObjects;

        Dictionary<string, string> names = new Dictionary<string, string>();
        foreach (string name in itemList) {
            GameObject tempObject = Instantiate(Resources.Load("prefabs/" + name)) as GameObject;
            Item tempItem = tempObject.GetComponent<Item>();
            if (tempItem) {
                names[name] = tempItem.itemName;
            } else names[name] = name;
            Destroy(tempObject);
        }
        itemList = itemList.OrderBy(i => names[i]).ToList();
        bool mousedOver = false;
        foreach (string name in itemList) {
            ItemEntryScript script = spawnEntry(name);
            // effects.buttons.Add(script.GetComponentInChildren<Button>());
            Button entryButton = script.gameObject.GetComponentInChildren<Button>();
            effects.buttons.Add(entryButton);
            if (!mousedOver) {
                mousedOver = true;
                SetDetailView(script);
            }
        }
        effects.Configure();
    }
    public void InstantiateBuff(Buff buff) {
        GameObject icon = Instantiate(Resources.Load("UI/StatusIcon")) as GameObject;
        UIStatusIcon statusIcon = icon.GetComponent<UIStatusIcon>();

        Outline outline = icon.GetComponent<Outline>();
        OutlineFader fader = icon.GetComponent<OutlineFader>();
        ParticleSystem fx = icon.GetComponentInChildren<ParticleSystem>();

        if (outline != null)
            Destroy(outline);
        if (fader != null)
            Destroy(fader);
        if (fx != null)
            Destroy(fx);

        statusIcon.Initialize(buff.type, buff);
        statusIcon.transform.SetParent(buffs, false);
    }
    public void ClearBuffs() {
        foreach (Transform childObject in buffs) {
            Destroy(childObject.gameObject);
        }
    }

    public void SetDetailView(ItemEntryScript itemScript) {
        ClearBuffs();
        nameText.text = itemScript.itemName;
        descriptionText.text = itemScript.description;
        icon.sprite = itemScript.sprite;
        icon.color = new Color(1f, 1f, 1f, 1f);
        foreach (Buff buff in itemScript.buffs) {
            InstantiateBuff(buff);
        }
    }
    // set lastclicked
    // check enabled
    // update buttons
    public void ItemButtonCallback(ItemEntryScript itemScript) {
        SetDetailView(itemScript);
        lastClicked = itemScript;
        if (itemScript.enableItem) {
            GameObject tempObject = Instantiate(Resources.Load("prefabs/" + itemScript.prefabName)) as GameObject;
            Uniform clothes = tempObject.GetComponent<Uniform>();
            Hat hat = tempObject.GetComponent<Hat>();
            Destroy(tempObject);
            if (clothes != null) {
                SetButtons(LoadoutButtonType.clothes);
            } else if (hat != null) {
                SetButtons(LoadoutButtonType.hat);
            } else {
                SetButtons(LoadoutButtonType.inventory);
            }
        } else {
            SetButtons(LoadoutButtonType.none);
        }
    }


    public void LoadButtonCallback() {
        // load the loadout
        Debug.Log("load the loadout");
    }
    public void CloseButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
    public void SimpleButtonCallback() {
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.closet);
        ClosetButtonHandler menu = menuObject.GetComponent<ClosetButtonHandler>();
        menu.PopulateItemList(closetType);
        GameManager.Instance.DetermineClosetNews();
    }
    enum LoadoutButtonType { none, inventory, clothes, hat }
    void SetButtons(LoadoutButtonType buttonType) {
        switch (buttonType) {
            case LoadoutButtonType.none:
                inventoryButton.interactable = false;
                clothesButton.interactable = false;
                hatButton.interactable = false;
                break;
            case LoadoutButtonType.inventory:
                inventoryButton.interactable = true;
                clothesButton.interactable = false;
                hatButton.interactable = false;
                break;
            case LoadoutButtonType.clothes:
                inventoryButton.interactable = true;
                clothesButton.interactable = true;
                hatButton.interactable = false;
                break;
            case LoadoutButtonType.hat:
                inventoryButton.interactable = true;
                clothesButton.interactable = false;
                hatButton.interactable = true;
                break;
        }
    }

    public void InventoryButtonCallback() {
        // add to loadout
        spawnLoadoutScript(lastClicked);

        // remove from list
        lastClicked.enableItem = false;

        SetButtons(LoadoutButtonType.none);
    }
    public void ClothesButtonCallback() {
        // evict existing clothes
        EvictClothes();

        // set new clothes
        SetClothes(lastClicked);
        SetButtons(LoadoutButtonType.none);
    }
    public void HatButtonCallback() {
        Debug.Log("hat arrow");

        // evict existing hat
        EvictHat();

        // set new hat
        SetHat(lastClicked);
        SetButtons(LoadoutButtonType.none);
    }
    void SetHat(ItemEntryScript script) {
        script.enableItem = false;
        loadoutHat = script;
        hatText.text = script.itemName;
    }
    void SetClothes(ItemEntryScript script) {
        script.enableItem = false;
        loadoutClothes = script;
        clothesText.text = script.itemName;
    }

    void EvictHat() {
        // add hat back to the item list
        if (loadoutHat != null) {
            loadoutHat.enableItem = true;
        }
    }
    void EvictClothes() {
        // add clothes back to the item list
        if (loadoutClothes != null) {
            loadoutClothes.enableItem = true;
        }
    }
    public void EvictItem(LoadoutEntryScript script) {
        // add item back to the item list

        // delete button
        Destroy(script.gameObject);

        // reenable item script
        script.itemEntryScript.enableItem = true;
    }
}
