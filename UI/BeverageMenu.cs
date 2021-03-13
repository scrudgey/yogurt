using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class BeverageMenu : MonoBehaviour {
    public Image icon;
    public Text nameText;
    public Text ingredientsText;
    public Text buffsText;
    public GameObject buffBox;
    public Transform listContent;
    public UIButtonEffects effects;
    public Button closeButton;
    void Awake() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    private LiquidEntryScript spawnEntry() {
        GameObject newObject = Instantiate(Resources.Load("UI/LiquidEntry")) as GameObject;
        LiquidEntryScript script = newObject.GetComponent<LiquidEntryScript>();
        script.menu = this;
        return script;
    }
    public void PopulateItemList() {
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (Transform childObject in listContent) {
            Destroy(childObject.gameObject);
        }
        List<Liquid> itemList = GameManager.Instance.data.collectedLiquids;
        itemList = itemList.OrderBy(i => Liquid.GetName(i)).ToList();
        bool mousedover = false;
        foreach (Liquid liquid in itemList) {
            LiquidEntryScript script = spawnEntry();
            script.Configure(liquid);
            script.transform.SetParent(listContent, false);
            Button entryButton = script.gameObject.GetComponentInChildren<Button>();
            effects.buttons.Add(entryButton);
            if (!mousedover) {
                mousedover = true;
                MouseOver(script);
            }
        }
        effects.Configure();
        GameManager.Instance.data.newCollectedLiquids = new List<Liquid>();
    }
    public void CloseButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
    public void ItemClick(LiquidEntryScript itemScript) {
        GameObject playerObject = GameManager.Instance.playerObject;

        GameObject item = Instantiate(Resources.Load("prefabs/slushie"), playerObject.transform.position, Quaternion.identity) as GameObject;
        Instantiate(Resources.Load("particles/poof"), playerObject.transform.position, Quaternion.identity);
        GameManager.Instance.publicAudio.PlayOneShot(Resources.Load("sounds/pop", typeof(AudioClip)) as AudioClip);

        LiquidContainer liquidContainer = item.GetComponent<LiquidContainer>();
        // liquidContainer.
        liquidContainer.FillWithLiquid(itemScript.liquid);

        Inventory playerInventory = playerObject.GetComponent<Inventory>();
        Pickup itemPickup = item.GetComponent<Pickup>();
        if (playerInventory != null && itemPickup != null) {
            playerInventory.GetItem(itemPickup);
        }

        UINew.Instance.CloseActiveMenu();
    }
    public void MouseOver(LiquidEntryScript script) {
        nameText.text = Liquid.GetName(script.liquid);
        var ingredients = from liquid in script.liquid.atomicLiquids select liquid.name;
        // var buffs = from buff in script.liquid.buffs select Buff.buffNames[buff.type];

        ingredientsText.text = "";
        buffsText.text = "";
        buffBox.SetActive(true);


        ingredientsText.text = string.Join("\n", ingredients.ToList());
        // buffsText.text = string.Join("\n", buffs.ToList());
        icon.color = script.liquid.color;

        List<string> buffstrings = new List<string>();
        foreach (Buff buff in script.liquid.buffs) {
            string buffName = Buff.buffNames[buff.type];
            if (buff.lifetime == 0) {
                buffName = "permanent " + buffName;
            }
            // buffsText.text = buffsText.text + buffName + "\n";
            buffstrings.Add(buffName);
        }
        buffsText.text = string.Join("\n", buffstrings);

        if (ingredientsText.text == "") {
            ingredientsText.text = script.liquid.name;
        }
        if (buffsText.text == "") {
            buffBox.SetActive(false);
        }
    }
}
