using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
        foreach (GameObject prefab in perkPrefabs) {
            PerkComponent component = prefab.GetComponent<PerkComponent>();
            if (component) {
                GameObject buttonObject = Instantiate(Resources.Load("UI/PerkButton")) as GameObject;
                Text buttonText = buttonObject.transform.Find("Text").GetComponent<Text>();
                buttonObject.transform.SetParent(buttonList, false);
                PerkButton perkScript = buttonObject.GetComponent<PerkButton>();
                perkScript.menu = this;
                perkScript.perk = new Perk(component.perk);
                buttonText.text = component.perk.title;
                if (GameManager.Instance.data.perks[component.perk.name]) {
                    Button perkButton = buttonObject.GetComponent<Button>();
                    perkButton.interactable = false;
                }
            }
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
        UINew.Instance.RefreshUI(active:true);
        UINew.Instance.PlayUISound("sounds/8-bit/BOUNCE3");
    }
    public void DoneButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
}
