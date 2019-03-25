using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PerkMenu : MonoBehaviour {
    public Transform buttonList;
    public Image perkImage;
    public Text perkTitleText;
    public Text perkDescText;
    public Button doneButton;
    public Button acceptButton;
    public bool perkChosen;
    public PerkButton selectedPerk;
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        perkImage = transform.Find("menu/body/infoPanel/imagePanel/Image").GetComponent<Image>();

        perkDescText = transform.Find("menu/body/infoPanel/textPanel/desc").GetComponent<Text>();
        perkTitleText = transform.Find("menu/body/infoPanel/textPanel/title").GetComponent<Text>();
        doneButton = transform.Find("menu/body/infoPanel/Done").GetComponent<Button>();
        acceptButton = transform.Find("menu/body/infoPanel/Accept").GetComponent<Button>();

        buttonList = transform.Find("menu/body/list");
        doneButton.interactable = false;
        acceptButton.interactable = false;

        PopulatePerkList();
    }
    void PopulatePerkList() {
        GameObject[] perkPrefabs = Resources.LoadAll("perks/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        int numberCollected = 0;
        foreach (KeyValuePair<string, bool> kvp in GameManager.Instance.data.perks) {
            if (kvp.Value) {
                numberCollected += 1;
            }
        }
        Dictionary<GameObject, PerkComponent> perkComponents = new Dictionary<GameObject, PerkComponent>();
        foreach (GameObject prefab in perkPrefabs) {
            PerkComponent component = prefab.GetComponent<PerkComponent>();
            if (component)
                perkComponents[prefab] = component;
        }
        perkPrefabs = perkPrefabs.OrderBy(p => perkComponents[p].perk.requiredPerks).ToArray();
        foreach (GameObject prefab in perkPrefabs) {
            PerkComponent component = perkComponents[prefab];

            GameObject buttonObject = Instantiate(Resources.Load("UI/PerkButton")) as GameObject;
            Text buttonText = buttonObject.transform.Find("Text").GetComponent<Text>();
            buttonObject.transform.SetParent(buttonList, false);
            PerkButton perkScript = buttonObject.GetComponent<PerkButton>();
            perkScript.menu = this;
            perkScript.perk = new Perk(component.perk);
            buttonText.text = component.perk.title;
            Button perkButton = buttonObject.GetComponent<Button>();
            if (numberCollected < component.perk.requiredPerks) {
                perkButton.interactable = false;
            }
            if (GameManager.Instance.data.perks[component.perk.name]) {
                perkButton.interactable = false;
            }
        }
        if (numberCollected == perkPrefabs.Count()) {
            doneButton.interactable = true;
        }
    }
    public void PerkButtonClicked(PerkButton buttonScript) {
        selectedPerk = buttonScript;
        perkDescText.text = buttonScript.perk.desc;
        perkTitleText.text = buttonScript.perk.title;
        perkImage.sprite = buttonScript.perk.perkImage;

        if (!doneButton.interactable)
            acceptButton.interactable = true;
    }
    public void AcceptButtonClicked() {
        if (selectedPerk == null)
            return;
        acceptButton.interactable = false;
        doneButton.interactable = true;
        GameManager.Instance.data.perks[selectedPerk.perk.name] = true;
        selectedPerk.GetComponent<Button>().interactable = false;
        UINew.Instance.RefreshUI(active: true);
        UINew.Instance.PlayUISound("sounds/8-bit/BOUNCE3");
    }
    public void DoneButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
}
