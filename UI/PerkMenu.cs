using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PerkMenu : MonoBehaviour {
    public Transform buttonList;
    public Image perkImage;
    public Text perkTitleText;
    public Text perkDescText;
    public Text requiredText;
    public Text levelText;
    public Text buttonText;
    public Button doneButton;
    public Button acceptButton;
    public bool perkChosen;
    public PerkButton selectedPerk;
    int numberCollected = 0;
    public Color unlockedPerkColor;
    public Color lockedPerkColor;
    public List<Button> builtInButtons;
    public UIButtonEffects effects;
    public AspectRatioFitter aspectRatioFitter;
    public delegate void MyDelegate();
    // public MyDelegate menuClosed;
    public Chela chela;

    void Start() {
        effects = GetComponent<UIButtonEffects>();
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        perkImage = transform.Find("menu/body/infoPanel/imagePanel/Image").GetComponent<Image>();
        aspectRatioFitter = perkImage.GetComponent<AspectRatioFitter>();

        perkDescText = transform.Find("menu/body/infoPanel/textPanel/desc").GetComponent<Text>();
        perkTitleText = transform.Find("menu/body/infoPanel/textPanel/title").GetComponent<Text>();
        requiredText = transform.Find("menu/body/infoPanel/textPanel/req").GetComponent<Text>();
        levelText = transform.Find("menu/body/infoPanel/levelText").GetComponent<Text>();

        doneButton = transform.Find("menu/body/infoPanel/Done").GetComponent<Button>();
        acceptButton = transform.Find("menu/body/infoPanel/Accept").GetComponent<Button>();

        buttonList = transform.Find("menu/body/list");
        doneButton.interactable = false;
        acceptButton.interactable = false;

        numberCollected = 1;
        foreach (KeyValuePair<string, bool> kvp in GameManager.Instance.data.perks) {
            if (kvp.Value) {
                numberCollected += 1;
            }
        }
        levelText.text = "";

        SetLevelText();
        PopulatePerkList();

        // MusicController.Instance.EnqueueMusic(new MusicChela());
        MusicController.Instance.EnqueueMusic(new Music(new Track(TrackName.chela)));
    }
    void SetLevelText() {
        string text = $"Level {numberCollected} ";
        switch (numberCollected) {
            default:
            case 1:
                text += "Yogurt Neophyte";
                break;

            case 2:
                text += "Yogurt Adept";
                break;

            case 3:
                text += "Yogurtmancer";
                break;
            case 4:
                text += "Yogurtmancer";
                break;

            case 5:
                text += "Yogurt Master";
                break;
            case 6:
                text += "Yogurt Master";
                break;
            case 7:
                text += "Yogurt Master";
                break;

            case 8:
                text += "Yogurt Champion";
                break;
        }
        levelText.text = text;
    }
    void PopulatePerkList() {
        effects.buttons = new List<Button>(builtInButtons);
        List<GameObject> perkPrefabs = Resources.LoadAll("data/perks/", typeof(GameObject))
    .Cast<GameObject>()
    .ToList();
        requiredText.text = "";
        foreach (Transform child in buttonList) {
            Destroy(child.gameObject);
        }

        Dictionary<GameObject, PerkComponent> perkComponents = new Dictionary<GameObject, PerkComponent>();
        List<GameObject> removeThese = new List<GameObject>();
        foreach (GameObject prefab in perkPrefabs) {
            PerkComponent component = prefab.GetComponent<PerkComponent>();
            if (component && !component.disablePerk)
                perkComponents[prefab] = component;
            if (component && component.disablePerk)
                removeThese.Add(prefab);
        }
        foreach (GameObject removeMe in removeThese) {
            perkPrefabs.Remove(removeMe);
        }
        perkPrefabs = perkPrefabs.OrderBy(p => perkComponents[p].perk.requiredPerks).ToList();
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
            effects.buttons.Add(perkButton);
            ColorBlock colors = perkButton.colors;
            if (numberCollected > component.perk.requiredPerks) {
                colors.highlightedColor = unlockedPerkColor;
            } else {
                colors.highlightedColor = lockedPerkColor;
            }
            perkButton.colors = colors;
            if (GameManager.Instance.data.perks[component.perk.name]) {
                perkButton.interactable = false;
            }
        }
        if (numberCollected == perkPrefabs.Count() + 1) {
            doneButton.interactable = true;
        }
        effects.Configure();
    }
    public void PerkButtonClicked(PerkButton buttonScript) {
        aspectRatioFitter.aspectRatio = 1;
        selectedPerk = buttonScript;
        perkDescText.text = buttonScript.perk.desc;
        perkTitleText.text = buttonScript.perk.title;
        perkImage.sprite = buttonScript.perk.perkImage;
        Button perkButton = buttonScript.GetComponent<Button>();
        int reqLevel = buttonScript.perk.requiredPerks + 1;
        requiredText.text = "required: level " + reqLevel.ToString();
        if (numberCollected > buttonScript.perk.requiredPerks) {
            requiredText.color = unlockedPerkColor;
        } else {
            requiredText.color = lockedPerkColor;
        }
        if (perkChosen) {
            acceptButton.interactable = false;
        } else if (numberCollected <= buttonScript.perk.requiredPerks) {
            acceptButton.interactable = false;
        } else if (GameManager.Instance.data.perks[buttonScript.perk.name]) {
            acceptButton.interactable = false;
        } else {
            acceptButton.interactable = true;
        }
    }
    public void AcceptButtonClicked() {
        if (selectedPerk == null)
            return;
        buttonText.text = "CHOOSE PERK (0)";
        perkChosen = true;
        acceptButton.interactable = false;
        doneButton.interactable = true;
        GameManager.Instance.data.perks[selectedPerk.perk.name] = true;
        if (GameManager.Instance.data.perks["vomit"]) {
            GameManager.Instance.UnlockTVShow("vampire2");
        }

        // TODO: check if we've unlocked camcorder, too
        if (GameManager.Instance.data.perks["vomit"] &&
        GameManager.Instance.data.perks["eat_all"] &&
        GameManager.Instance.data.collectedObjects.Contains("camcorder")
        ) {
            GameManager.Instance.UnlockCommercial("dungeon");
            GameManager.Instance.ReceiveEmail("dungeon_start");
        }
        selectedPerk.GetComponent<Button>().interactable = false;
        UINew.Instance.RefreshUI(active: true);
        GameManager.Instance.PlayPublicSound(Resources.Load("sounds/8-bit/BOUNCE3") as AudioClip);

        numberCollected += 1;
        chela.Disappear();
        SetLevelText();
    }
    public void DoneButtonClick() {
        UINew.Instance.CloseActiveMenu();

        MusicController.Instance.End();
    }
    void OnDestroy() {
        // if (menuClosed != null)
        //     menuClosed();
    }

}
