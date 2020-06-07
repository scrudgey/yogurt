using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class ClosetButtonHandler : MonoBehaviour {
    public Image icon;
    public Text titleText;
    public Text descriptionText;
    public Text nameText;
    public Transform listContent;
    public Transform buffs;
    public UIButtonEffects effects;
    public Button closeButton;
    public HomeCloset.ClosetType closetType;
    public HomeCloset closet;
    void Awake() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        icon.sprite = null;
        icon.color = new Color(1f, 1f, 1f, 0f);
    }
    private ItemEntryScript spawnEntry() {
        GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
        return newObject.GetComponent<ItemEntryScript>();
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
    public void PopulateItemList(HomeCloset.ClosetType type, HomeCloset closet) {
        this.closet = closet;
        closetType = type;

        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (Transform childObject in listContent) {
            Destroy(childObject.gameObject);
        }
        ClearBuffs();
        List<string> itemList = GameManager.Instance.data.collectedObjects;

        if (type == HomeCloset.ClosetType.clothing) {
            itemList = GameManager.Instance.data.collectedClothes;
            titleText.text = "Collected Clothing";
        } else if (type == HomeCloset.ClosetType.food) {
            itemList = GameManager.Instance.data.collectedFood;
            titleText.text = "Collected Food";
        } else if (type == HomeCloset.ClosetType.items) {
            itemList = GameManager.Instance.data.collectedItems;
            titleText.text = "Collected Items";
        }
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
            ItemEntryScript script = spawnEntry();
            // effects.buttons.Add(script.GetComponentInChildren<Button>());
            script.Configure(name, type);
            script.transform.SetParent(listContent, false);

            Button entryButton = script.gameObject.GetComponentInChildren<Button>();
            effects.buttons.Add(entryButton);
            if (!mousedOver) {
                mousedOver = true;
                ItemMouseover(script);
            }
        }
        effects.Configure();
    }
    public void CloseButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
    public void AdvancedButtonClick() {
        PlayerPrefs.SetString(HomeCloset.prefsKey_ClosetMenuType, "advanced");
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.loadoutEditor);
        LoadoutEditor menu = menuObject.GetComponent<LoadoutEditor>();
        menu.Configure(closet);
        GameManager.Instance.DetermineClosetNews();
    }
    public void ItemClick(ItemEntryScript itemScript) {
        GameObject playerObject = GameManager.Instance.playerObject;

        if (GameManager.Instance.data.itemCheckedOut[itemScript.prefabName])
            return;
        GameObject item = Instantiate(Resources.Load("prefabs/" + itemScript.prefabName), playerObject.transform.position, Quaternion.identity) as GameObject;
        Instantiate(Resources.Load("particles/poof"), playerObject.transform.position, Quaternion.identity);
        GameManager.Instance.publicAudio.PlayOneShot(Resources.Load("sounds/pop", typeof(AudioClip)) as AudioClip);
        GameManager.Instance.data.itemCheckedOut[itemScript.prefabName] = true;
        if (closetType == HomeCloset.ClosetType.clothing) {
            Outfit playerOutfit = playerObject.GetComponent<Outfit>();
            Uniform itemUniform = item.GetComponent<Uniform>();
            Head playerHead = playerObject.GetComponentInChildren<Head>();
            Hat itemHat = item.GetComponent<Hat>();

            if (playerOutfit != null && itemUniform != null) {
                GameObject removedUniform = playerOutfit.DonUniform(itemUniform);
                if (removedUniform != null) {
                    closet.StashObject(removedUniform.GetComponent<Pickup>());
                }
            }
            if (playerHead != null && itemHat != null) {
                GameObject removedHat = playerHead.DonHat(itemHat);
                if (removedHat != null) {
                    closet.StashObject(removedHat.GetComponent<Pickup>());
                }
            }
        } else {
            Inventory playerInventory = playerObject.GetComponent<Inventory>();
            Pickup itemPickup = item.GetComponent<Pickup>();
            if (playerInventory != null && itemPickup != null) {
                playerInventory.GetItem(itemPickup);
            }
        }


        UINew.Instance.CloseActiveMenu();
    }
    public void ItemMouseover(ItemEntryScript itemScript) {
        ClearBuffs();
        nameText.text = itemScript.itemName;
        descriptionText.text = itemScript.description;
        icon.sprite = itemScript.sprite;
        icon.color = new Color(1f, 1f, 1f, 1f);
        foreach (Buff buff in itemScript.buffs) {
            InstantiateBuff(buff);
        }
    }
}
