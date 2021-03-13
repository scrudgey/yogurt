using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class ScriptSelectionMenu : MonoBehaviour {
    Text descriptionText;
    public TextMeshProUGUI objectivesText;
    public Text commercialTitleText;
    GameObject scrollContent;
    ScriptListEntry lastClicked;
    public List<Button> builtInButtons;
    public UIButtonEffects effects;
    public Image headImage;
    public Image bodyImage;
    public List<Sprite> headSprites;
    public List<Sprite> sabotageHeadSprites;
    public Sprite sabotageBodySprite;
    public Text headshotText;
    float timer = 0;
    float pauseCountDown = 0f;
    int headIndex;
    public bool sabotage;
    void Start() {
        effects = GetComponent<UIButtonEffects>();
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        descriptionText = transform.Find("Panel/Body/sidebar/DescriptionPanel/DescriptionBox/Description").GetComponent<Text>();
        scrollContent = transform.Find("Panel/Body/Left/ScriptList/Viewport/Content").gameObject;

        // Toolbox.ApplySkinToneToSpriteSheet()
        Sprite[] undeadHead = Toolbox.MemoizedSkinTone("nude_demon_head", SkinColor.undead);
        sabotageHeadSprites = new List<Sprite> { undeadHead[2], undeadHead[3] };

        if (sabotage) {
            bodyImage.sprite = sabotageBodySprite;
            headImage.sprite = sabotageHeadSprites[0];
            headshotText.text = "Shady character";
        }

        effects.buttons = new List<Button>(builtInButtons);
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "gravy_studio") {
            LoadCommercials(gravy: true);
        } else if (sabotage) {
            LoadCommercials(sabotage: true);

        } else LoadCommercials();

        effects.Configure();
    }
    void Update() {
        if (pauseCountDown > 0f) {
            pauseCountDown -= Time.unscaledDeltaTime;
            return;
        }
        timer += Time.unscaledDeltaTime;
        if (timer > 0.1f) {
            timer = 0f;
            headIndex += 1;
            if (headIndex > 1) headIndex = 0;
            if (sabotage) {
                bodyImage.sprite = sabotageBodySprite;
                headImage.sprite = sabotageHeadSprites[headIndex];
            } else {
                headImage.sprite = headSprites[headIndex];
            }
            if (UnityEngine.Random.Range(0, 100f) < 3f) {
                pauseCountDown = Random.Range(1f, 2f);
                if (sabotage) {
                    headImage.sprite = sabotageHeadSprites[0];
                } else headImage.sprite = headSprites[0];
            }
        }
    }
    public void LoadCommercials(bool gravy = false, bool sabotage = false) {
        GameObject firstEntry = null;

        HashSet<Commercial> completeCommercials = new HashSet<Commercial>();
        HashSet<Commercial> incompleteCommercials = new HashSet<Commercial>();

        foreach (Commercial commercial in GameManager.Instance.data.unlockedCommercials) {
            if (gravy == commercial.gravy && sabotage == commercial.sabotage) {
                incompleteCommercials.Add(commercial);
            }
        }

        // yeah this is terrible. so what
        foreach (Commercial commercial in incompleteCommercials) {
            foreach (Commercial completed in GameManager.Instance.data.completeCommercials) {
                if (completed.name == commercial.name) {
                    completeCommercials.Add(commercial);
                }
            }
        }
        foreach (Commercial commercial in completeCommercials) {
            incompleteCommercials.Remove(commercial);
        }



        foreach (Commercial script in completeCommercials) {
            GameObject newEntry = CreateScriptButton(script);
            firstEntry = newEntry;
        }
        foreach (Commercial script in incompleteCommercials) {
            GameObject newEntry = CreateScriptButton(script);
            firstEntry = newEntry;
        }

        EventSystem.current.SetSelectedGameObject(firstEntry);
        ClickedScript(firstEntry.GetComponent<ScriptListEntry>());
        GameManager.Instance.data.setupSabotage = false;
    }
    GameObject CreateScriptButton(Commercial script) {
        GameObject newEntry = Instantiate(Resources.Load("UI/ScriptListEntry")) as GameObject;
        effects.buttons.Add(newEntry.GetComponent<Button>());
        newEntry.transform.SetParent(scrollContent.transform, false);
        newEntry.transform.SetAsFirstSibling();
        ScriptListEntry scriptEntry = newEntry.GetComponent<ScriptListEntry>();
        scriptEntry.Configure(script, this);
        return newEntry;
    }
    public void ClickedOkay() {
        if (lastClicked) {
            GameManager.Instance.StartCommercial(new Commercial(lastClicked.commercial));
        }
        UINew.Instance.CloseActiveMenu();
    }
    public void ClickedCancel() {
        UINew.Instance.CloseActiveMenu();
    }
    public void ClickedScript(ScriptListEntry entry) {
        if (lastClicked) {
            lastClicked.highlight = false;
            lastClicked.ResetColors();
        }
        descriptionText.text = entry.commercial.description;
        lastClicked = entry;
        lastClicked.highlight = true;
        commercialTitleText.text = entry.commercial.name;
        objectivesText.text = "";
        foreach (Objective objective in entry.commercial.objectives) {
            objectivesText.text += $"• {objective.desc}\n";
        }
    }
}
