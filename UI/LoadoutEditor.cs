using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
public class LoadoutEditor : MonoBehaviour {
    public Transform itemCollection;
    public Transform loadoutCollection;
    public Transform buffs;

    public Text clothesText;
    public GameObject clothesEvictButton;
    public Text hatText;
    public GameObject hatEvictButton;

    public Button inventoryButton;
    public Button clothesButton;
    public Button hatButton;
    public UIButtonEffects effects;

    public Image icon;
    public Text nameText;
    public Text descriptionText;

    private ItemEntryScript lastClicked;
    public HashSet<ItemEntryScript> loadoutItems = new HashSet<ItemEntryScript>();
    public ItemEntryScript loadoutClothes;
    public ItemEntryScript loadoutHat;

    private Dictionary<ItemEntryScript, GameObject> itemScripts = new Dictionary<ItemEntryScript, GameObject>();
    private Dictionary<LoadoutEntryScript, GameObject> loadoutScripts = new Dictionary<LoadoutEntryScript, GameObject>();

    public static readonly string loadoutFileName = "loadout.xml";
    public static readonly string keyItems = "items";
    public static readonly string keyHat = "hat";
    public static readonly string keyClothes = "clothes";
    public HomeCloset closet;
    public static string LoadoutSavePath() {
        return Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName, loadoutFileName);
    }
    private ItemEntryScript spawnEntry(string name) {
        GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
        ItemEntryScript script = newObject.GetComponent<ItemEntryScript>();
        script.Configure(name, HomeCloset.ClosetType.all);
        script.transform.SetParent(itemCollection, false);
        itemScripts[script] = newObject;
        script.enableItem = !GameManager.Instance.data.itemCheckedOut[name];
        // Debug.Log($"{name} {script.enableItem}");
        return script;
    }
    private LoadoutEntryScript spawnLoadoutScript(ItemEntryScript itemScript) {
        GameObject newObject = Instantiate(Resources.Load("UI/LoadoutItem")) as GameObject;
        LoadoutEntryScript script = newObject.GetComponent<LoadoutEntryScript>();
        script.Configure(itemScript);
        script.transform.SetParent(loadoutCollection, false);
        script.loadoutEditor = this;
        loadoutScripts[script] = newObject;
        loadoutItems.Add(itemScript);
        script.enableItem = !GameManager.Instance.data.itemCheckedOut[itemScript.prefabName];
        // Debug.Log($"{name} {script.enableItem}");
        return script;
    }

    public void Configure(HomeCloset closet) {
        this.closet = closet;
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

        clothesText.text = "";
        clothesEvictButton.SetActive(false);
        hatText.text = "";
        hatEvictButton.SetActive(false);

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
            Button entryButton = script.gameObject.GetComponentInChildren<Button>();
            effects.buttons.Add(entryButton);
            if (!mousedOver) {
                mousedOver = true;
                SetDetailView(script);
            }
        }
        effects.Configure();
        LoadLoadout();
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


    public void CloseButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
    public void SimpleButtonCallback() {
        PlayerPrefs.SetString(HomeCloset.prefsKey_ClosetMenuType, "simple");

        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.closet);
        ClosetButtonHandler menu = menuObject.GetComponent<ClosetButtonHandler>();
        menu.PopulateItemList(HomeCloset.ClosetType.all, closet);
        GameManager.Instance.DetermineClosetNews();
    }
    public enum LoadoutButtonType { none, inventory, clothes, hat }
    public void SetButtons(LoadoutButtonType buttonType) {
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
        SaveLoadout();
    }
    public void ClothesButtonCallback() {
        // evict existing clothes
        EvictClothes();

        // set new clothes
        SetClothes(lastClicked);
        SetButtons(LoadoutButtonType.none);
        SaveLoadout();
    }
    public void HatButtonCallback() {
        // evict existing hat
        EvictHat();

        // set new hat
        SetHat(lastClicked);
        SetButtons(LoadoutButtonType.none);
        SaveLoadout();
    }
    public void ClothesFieldCallback() {
        if (loadoutClothes != null) {
            SetDetailView(loadoutClothes);
            SetButtons(LoadoutButtonType.none);
        }
    }
    public void HatFieldCallback() {
        if (loadoutHat != null) {
            SetDetailView(loadoutHat);
            SetButtons(LoadoutButtonType.none);
        }
    }
    void SetHat(ItemEntryScript script) {
        script.enableItem = false;
        loadoutHat = script;
        hatText.text = script.itemName;
        hatEvictButton.SetActive(true);
    }
    void SetClothes(ItemEntryScript script) {
        script.enableItem = false;
        loadoutClothes = script;
        clothesText.text = script.itemName;
        clothesEvictButton.SetActive(true);
    }

    public void EvictHat() {
        // add hat back to the item list
        if (loadoutHat != null) {
            loadoutHat.enableItem = !GameManager.Instance.data.itemCheckedOut[loadoutHat.prefabName];
        }
        loadoutHat = null;
        hatText.text = "";
        hatEvictButton.SetActive(false);
    }
    public void EvictClothes() {
        // add clothes back to the item list
        if (loadoutClothes != null) {
            loadoutClothes.enableItem = !GameManager.Instance.data.itemCheckedOut[loadoutClothes.prefabName];
        }
        loadoutClothes = null;
        clothesText.text = "";
        clothesEvictButton.SetActive(false);
    }
    public void EvictItem(LoadoutEntryScript script) {

        loadoutItems.Remove(script.itemEntryScript);
        // reenable item script
        script.itemEntryScript.enableItem = !GameManager.Instance.data.itemCheckedOut[script.prefabName];

        // delete button
        Destroy(script.gameObject);
        SaveLoadout();
    }
    public GameObject SpawnPrefab(string prefabName) {
        GameObject item = Instantiate(Resources.Load("prefabs/" + prefabName), GameManager.Instance.playerObject.transform.position, Quaternion.identity) as GameObject;
        GameManager.Instance.data.itemCheckedOut[prefabName] = true;
        PhysicalBootstrapper phys = item.GetComponent<PhysicalBootstrapper>();
        if (phys)
            phys.DestroyPhysical();
        return item;
    }
    public void LoadButtonCallback() {
        // load the loadout
        GameObject playerObject = GameManager.Instance.playerObject;
        Inventory playerInventory = playerObject.GetComponent<Inventory>();
        Outfit playerOutfit = playerObject.GetComponent<Outfit>();

        foreach (ItemEntryScript script in loadoutItems) {
            if (GameManager.Instance.data.itemCheckedOut[script.prefabName])
                continue;
            GameObject item = SpawnPrefab(script.prefabName);
            Pickup itemPickup = item.GetComponent<Pickup>();
            if (playerInventory != null && itemPickup != null) {
                playerInventory.StashItem(item);
            }
        }
        if (loadoutClothes != null && !GameManager.Instance.data.itemCheckedOut[loadoutClothes.prefabName]) {
            GameObject item = SpawnPrefab(loadoutClothes.prefabName);
            Uniform itemUniform = item.GetComponent<Uniform>();
            if (playerOutfit != null && itemUniform != null) {
                GameObject removedUniform = playerOutfit.DonUniform(itemUniform);
                if (removedUniform != null) {
                    closet.StashObject(removedUniform.GetComponent<Pickup>());
                }
            }
        }
        if (loadoutHat != null && !GameManager.Instance.data.itemCheckedOut[loadoutHat.prefabName]) {
            GameObject item = SpawnPrefab(loadoutHat.prefabName);
            Head playerHead = playerObject.GetComponentInChildren<Head>();
            Hat itemHat = item.GetComponent<Hat>();
            if (playerHead != null && itemHat != null) {
                GameObject removedHat = playerHead.DonHat(itemHat);
                if (removedHat != null) {
                    closet.StashObject(removedHat.GetComponent<Pickup>());
                }
            }
        }
        GameManager.Instance.publicAudio.PlayOneShot(Resources.Load("sounds/pop", typeof(AudioClip)) as AudioClip);
        Instantiate(Resources.Load("particles/poof"), playerObject.transform.position, Quaternion.identity);
        UINew.Instance.CloseActiveMenu();
    }

    public void LoadLoadout() {
        string path = LoadoutSavePath();

        if (!System.IO.File.Exists(path))
            return;


        var dictSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
        SerializableDictionary<string, string> loadout = new SerializableDictionary<string, string>();

        if (File.Exists(path)) {
            using (var loadoutStream = new FileStream(path, FileMode.Open)) {
                loadout = dictSerializer.Deserialize(loadoutStream) as SerializableDictionary<string, string>;
            }
        }
        List<ItemEntryScript> itemScriptList = itemScripts.Keys.ToList();
        if (loadout.ContainsKey(keyItems)) {
            foreach (string prefabName in loadout[keyItems].Split(';')) {
                ItemEntryScript itemScript = itemScriptList.Find(x => x.prefabName == prefabName);
                spawnLoadoutScript(itemScript);
                itemScript.enableItem = false;
            }
        }
        if (loadout.ContainsKey(keyHat)) {
            ItemEntryScript itemScript = itemScriptList.Find(x => x.prefabName == loadout[keyHat]);
            SetHat(itemScript);
        }
        if (loadout.ContainsKey(keyClothes)) {
            ItemEntryScript itemScript = itemScriptList.Find(x => x.prefabName == loadout[keyClothes]);
            SetClothes(itemScript);
        }
    }

    public void SaveLoadout() {
        var loadout = new SerializableDictionary<string, string>();

        if (loadoutItems.Count > 0) {
            loadout[keyItems] = String.Join(";", loadoutItems.Select(x => x.prefabName));
        }
        if (loadoutHat != null) {
            loadout[keyHat] = loadoutHat.prefabName;
        }
        if (loadoutClothes != null) {
            loadout[keyClothes] = loadoutClothes.prefabName;
        }

        var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
        string path = LoadoutSavePath();
        using (FileStream objectStream = File.Create(path)) {
            persistentSerializer.Serialize(objectStream, loadout);
        }
    }
}
