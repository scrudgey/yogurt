using UnityEngine;
using System.Collections.Generic;
public class CutsceneImp : Cutscene {
    private enum State { start, first, describeFirst, second, describeSecond }
    private State state;
    GameObject analyzand;
    BuffType buffType;
    PotionData potionData;
    GameObject imp;
    Speech impSpeech;
    AnimateFrames impAnimate;
    PotionSeller impSeller;
    SpriteRenderer impRenderer;
    float timer = 0;
    public void Configure(GameObject analyzand) {
        configured = true;
        imp = GameObject.Find("imp");
        impSpeech = imp.GetComponent<Speech>();
        impAnimate = imp.GetComponent<AnimateFrames>();
        impSeller = imp.GetComponent<PotionSeller>();
        impRenderer = imp.GetComponent<SpriteRenderer>();
        impAnimate.enabled = false;
        impRenderer.sprite = impAnimate.frames[0];
        this.analyzand = analyzand;
        Controllable playerController = GameManager.Instance.playerObject.GetComponent<Controllable>();
        UINew.Instance.RefreshUI();
        if (GetIngredients()) {
            StartAnalysis();
        }
    }
    void SetDialogue(DialogueNode newNode) {
        DialogueMenu menu = UINew.Instance.ShowMenu(UINew.MenuType.dialogue).GetComponent<DialogueMenu>();
        menu.Configure(GameManager.Instance.playerObject.GetComponent<Speech>(), impSpeech);
        menu.node = null;
        menu.dialogueTree = new List<DialogueNode>();
        menu.monologue = new Monologue();
        menu.dialogue = new Stack<Monologue>();
        menu.cutsceneDialogue = true;

        menu.dialogueTree.Add(newNode);
        menu.ParseNode(newNode);
    }
    void StartAnalysis() {
        state = State.start;
        Dictionary<BuffType, PotionData> buffMap = PotionComponent.BuffToPotion();
        PotionData dat = buffMap[buffType];
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add("Gra ha ha ha... Show me your trinket....");
        newNode.text.Add("What a lovely " + Toolbox.Instance.GetName(analyzand) + ".");
        newNode.text.Add("Within it, I sense much " + dat.name + ".");
        newNode.text.Add("The first ingredient is...");
        newNode.text.Add("IMPCALLBACK1");
        SetDialogue(newNode);
    }
    public override void Update() {
        timer += Time.deltaTime;
        if (state == State.first) {
            if (timer > 1 && impRenderer.sprite == impSeller.leftWave[0]) {
                impRenderer.sprite = impSeller.leftWave[1];
                RevealFirstIngredient();
            }
            if (timer > 3) {
                DescribeFirstIngredient();
            }
        }
        if (state == State.second) {
            if (timer > 1 && impRenderer.sprite == impSeller.rightWave[0]) {
                impRenderer.sprite = impSeller.rightWave[1];
                RevealSecondIngredient();
            }
            if (timer > 3) {
                DescribeSecondIngredient();
            }
        }
    }
    public void RevealFirstIngredient() {
        impSeller.PlayIngredientSound();
        impSeller.leftPoint.SetActive(true);
        impSeller.leftPoint.GetComponent<SpriteRenderer>().sprite = potionData.ingredient1.icon;
    }
    public void RevealSecondIngredient() {
        impSeller.PlayIngredientSound();
        impSeller.rightPoint.SetActive(true);
        impSeller.rightPoint.GetComponent<SpriteRenderer>().sprite = potionData.ingredient2.icon;
    }
    public void FirstIngredient() {
        state = State.first;
        timer = 0;
        impRenderer.sprite = impSeller.leftWave[0];
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void DescribeFirstIngredient() {
        state = State.describeFirst;
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add(potionData.ingredient1.name + "!!!");
        newNode.text.Add("The second ingredient is...");
        newNode.text.Add("IMPCALLBACK2");
        SetDialogue(newNode);
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void SecondIngredient() {
        state = State.second;
        timer = 0;
        impRenderer.sprite = impSeller.rightWave[0];
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void DescribeSecondIngredient() {
        state = State.describeFirst;
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add(potionData.ingredient2.name + "!!!");
        newNode.text.Add("Together they make potion of " + potionData.name + "!");
        newNode.text.Add("IMPCALLBACK3");
        SetDialogue(newNode);
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void Finish() {
        complete = true;
        impAnimate.enabled = true;
        impSeller.leftPoint.SetActive(false);
        impSeller.rightPoint.SetActive(false);
    }

    public bool GetIngredients() {
        Intrinsics intrinsics = analyzand.GetComponent<Intrinsics>();
        Dictionary<BuffType, PotionData> buffMap = PotionComponent.BuffToPotion();
        if (intrinsics == null)
            return false;
        if (intrinsics.buffs.Count > 0) {
            buffType = intrinsics.buffs[0].type;
            potionData = buffMap[buffType];
            return true;
        } else if (intrinsics.liveBuffs.Count > 0) {
            buffType = intrinsics.liveBuffs[0].type;
            potionData = buffMap[buffType];
            return true;
        }
        return false;
    }
}