using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PerkBrowser : MonoBehaviour {
    public Transform buttonList;
    public Image perkImage;
    public Text perkTitleText;
    public Text perkDescText;
    public Text requiredText;
    public Button doneButton;
    public PerkButton selectedPerk;
    int numberCollected = 0;
    public Color unlockedPerkColor;
    public Color lockedPerkColor;
    public Color unlockedTextColor;
    public Color lockedTextColor;
    public UIButtonEffects effects;
    public Text activeText;
    void Start() {
        effects = GetComponent<UIButtonEffects>();
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        perkImage = transform.Find("menu/body/infoPanel/imagePanel/Image").GetComponent<Image>();

        perkDescText = transform.Find("menu/body/infoPanel/textPanel/desc").GetComponent<Text>();
        perkTitleText = transform.Find("menu/body/infoPanel/textPanel/title").GetComponent<Text>();
        requiredText = transform.Find("menu/body/infoPanel/textPanel/req").GetComponent<Text>();
        activeText = transform.Find("menu/body/infoPanel/activeText").GetComponent<Text>();

        doneButton = transform.Find("menu/body/infoPanel/Done").GetComponent<Button>();

        buttonList = transform.Find("menu/body/list");

        numberCollected = 1;
        foreach (KeyValuePair<string, bool> kvp in GameManager.Instance.data.perks) {
            if (kvp.Value) {
                numberCollected += 1;
            }
        }
        PopulatePerkList();
    }
    void PopulatePerkList() {
        effects.buttons = new List<Button>();
        GameObject[] perkPrefabs = Resources.LoadAll("perks/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        requiredText.text = "";
        foreach (Transform child in buttonList) {
            Destroy(child.gameObject);
        }

        Dictionary<GameObject, PerkComponent> perkComponents = new Dictionary<GameObject, PerkComponent>();
        foreach (GameObject prefab in perkPrefabs) {
            PerkComponent component = prefab.GetComponent<PerkComponent>();
            if (component)
                perkComponents[prefab] = component;
        }
        perkPrefabs = perkPrefabs.OrderBy(p => perkComponents[p].perk.requiredPerks).ToArray();
        bool initialButton = false;
        foreach (GameObject prefab in perkPrefabs) {
            PerkComponent component = perkComponents[prefab];

            GameObject buttonObject = Instantiate(Resources.Load("UI/PerkButton")) as GameObject;
            Text buttonText = buttonObject.transform.Find("Text").GetComponent<Text>();
            buttonObject.transform.SetParent(buttonList, false);
            PerkButton perkScript = buttonObject.GetComponent<PerkButton>();
            perkScript.browser = this;
            perkScript.perk = new Perk(component.perk);
            buttonText.text = component.perk.title;
            Button perkButton = buttonObject.GetComponent<Button>();
            effects.buttons.Add(perkButton);
            ColorBlock colors = perkButton.colors;
            if (GameManager.Instance.data.perks[perkScript.perk.name]) {
                // colors.highlightedColor = unlockedPerkColor;
                colors.normalColor = unlockedPerkColor;
            } else {
                // colors.highlightedColor = lockedPerkColor;
                colors.normalColor = lockedPerkColor;
            }
            perkButton.colors = colors;
            if (!initialButton) {
                initialButton = true;
                PerkButtonClicked(perkScript);
            }
        }
        effects.Configure();
    }
    public void PerkButtonClicked(PerkButton buttonScript) {
        selectedPerk = buttonScript;
        perkDescText.text = buttonScript.perk.desc;
        perkTitleText.text = buttonScript.perk.title;
        perkImage.sprite = buttonScript.perk.perkImage;
        Button perkButton = buttonScript.GetComponent<Button>();
        int reqLevel = buttonScript.perk.requiredPerks + 1;
        requiredText.text = "required: level " + reqLevel.ToString();
        if (GameManager.Instance.data.perks[buttonScript.perk.name]) {
            // colors.highlightedColor = unlockedPerkColor;
            // colors.normalColor = unlockedPerkColor;
            // activeText.color = unlockedPerkColor;
            activeText.color = unlockedTextColor;
            activeText.text = "This perk is active";
        } else {
            // colors.highlightedColor = lockedPerkColor;
            // colors.normalColor = lockedPerkColor;
            // activeText.color = lockedPerkColor;
            // activeText.color = lockedPerkColor;
            activeText.color = lockedTextColor;
            activeText.text = "You haven't unlocked this perk yet";
        }
    }
    public void DoneButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }

}
