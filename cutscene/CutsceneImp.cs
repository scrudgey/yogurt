using UnityEngine;
using System.Collections.Generic;
using Nimrod;
using System;

public class CutsceneImp : Cutscene {
    static public List<BuffType> buffPriority = new List<BuffType>(){
        BuffType.telepathy,
        BuffType.poison,
        BuffType.coughing,
        BuffType.undead,
        BuffType.bonusHealth,
        BuffType.armor,
        BuffType.fireproof,
        BuffType.clearHeaded,
        BuffType.enraged,
        BuffType.noPhysicalDamage,
        BuffType.strength,
        BuffType.speed,
        BuffType.ethereal,
        BuffType.invulnerable,
        BuffType.death,
    };

    private enum CutsceneState { none, describePotion, describeIngredient }
    private enum PotionState { start, first, describeFirst, second, describeSecond }
    private CutsceneState cutsceneState;
    private PotionState state;
    GameObject analyzand;
    BuffType buffType;
    PotionData potionData;
    GameObject imp;
    Speech impSpeech;
    AnimateFrames impAnimate;
    PotionSeller impSeller;
    SpriteRenderer impRenderer;
    Dictionary<BuffType, PotionData> buffMap = PotionComponent.BuffToPotion();
    Grammar grammar = new Grammar();
    public string ingredient;
    float timer = 0;
    public void Configure(GameObject analyzand) {
        grammar.Load("imp");
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
            cutsceneState = CutsceneState.describePotion;
            StartAnalysis();
        } else if (GetPotion()) {
            cutsceneState = CutsceneState.describeIngredient;
            StartIngredientAnalysis();
        } else {
            RefuseAnalysis();
        }
        // TODO: else if special
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
    void StartIngredientAnalysis() {

        // message.phrase = grammar.Parse(message.phrase);

        DialogueNode newNode = new DialogueNode();
        newNode.text.Add("Gra ha ha ha... Yes, show me your trinket....");
        newNode.text.Add(ingredient);
        newNode.text.Add(grammar.Parse("{ingredientDesc}"));
        newNode.text.Add("It can be used in potion of " + potionData.name + "!");
        newNode.text.Add("IMPCALLBACK3");
        SetDialogue(newNode);
    }
    void StartAnalysis() {
        state = PotionState.start;
        PotionData dat = buffMap[buffType];
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add("Gra ha ha ha... Yes, show me your trinket....");
        newNode.text.Add("What a lovely " + Toolbox.Instance.GetName(analyzand) + ".");
        newNode.text.Add("Within it, I sense much " + dat.name + ".");
        newNode.text.Add("The first ingredient is...");
        newNode.text.Add("IMPCALLBACK1");
        SetDialogue(newNode);
    }
    void RefuseAnalysis() {
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add("Gra ha ha ha... Yes, show me your trinket....");
        newNode.text.Add("What a lovely " + Toolbox.Instance.GetName(analyzand) + ".");
        newNode.text.Add("But it contains no magic.");
        newNode.text.Add("Show me something else...");
        newNode.text.Add("IMPCALLBACK3");
        SetDialogue(newNode);
    }
    public override void Update() {
        timer += Time.deltaTime;
        if (cutsceneState == CutsceneState.describePotion) {
            if (state == PotionState.first) {
                if (timer > 1 && impRenderer.sprite == impSeller.leftWave[0]) {
                    impRenderer.sprite = impSeller.leftWave[1];
                    RevealFirstIngredient();
                }
                if (timer > 3) {
                    DescribeFirstIngredient();
                }
            }
            if (state == PotionState.second) {
                if (timer > 1 && impRenderer.sprite == impSeller.rightWave[0]) {
                    impRenderer.sprite = impSeller.rightWave[1];
                    RevealSecondIngredient();
                }
                if (timer > 3) {
                    DescribeSecondIngredient();
                }
            }
        } else if (cutsceneState == CutsceneState.describeIngredient) {

        }
    }
    public void RevealFirstIngredient() {
        impSeller.PlayIngredientSound();
        impSeller.leftPoint.SetActive(true);
        SpriteRenderer leftPointSprite = impSeller.leftPoint.GetComponent<SpriteRenderer>();
        leftPointSprite.sprite = potionData.ingredient1.icon;
        leftPointSprite.color = potionData.ingredient1.spriteColor;
    }
    public void RevealSecondIngredient() {
        impSeller.PlayIngredientSound();
        impSeller.rightPoint.SetActive(true);
        SpriteRenderer rightPointSprite = impSeller.rightPoint.GetComponent<SpriteRenderer>();
        rightPointSprite.sprite = potionData.ingredient2.icon;
        rightPointSprite.color = potionData.ingredient2.spriteColor;
    }
    public void FirstIngredient() {
        state = PotionState.first;
        timer = 0;
        impRenderer.sprite = impSeller.leftWave[0];
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void DescribeFirstIngredient() {
        state = PotionState.describeFirst;
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add("TEXTSIZE:LARGE");
        newNode.text.Add(Toolbox.UppercaseFirst(potionData.ingredient1.name) + "!!!");
        newNode.text.Add("TEXTSIZE:NORMAL");
        newNode.text.Add("The second ingredient is...");
        newNode.text.Add("IMPCALLBACK2");
        SetDialogue(newNode);
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void SecondIngredient() {
        state = PotionState.second;
        timer = 0;
        impRenderer.sprite = impSeller.rightWave[0];
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void DescribeSecondIngredient() {
        state = PotionState.describeFirst;
        DialogueNode newNode = new DialogueNode();
        newNode.text.Add("TEXTSIZE:LARGE");
        newNode.text.Add(Toolbox.UppercaseFirst(potionData.ingredient2.name) + "!!!");
        newNode.text.Add("TEXTSIZE:NORMAL");
        newNode.text.Add("Together they make potion of " + potionData.name + "!");
        newNode.text.Add("IMPCALLBACK3");
        SetDialogue(newNode);
        Controller.Instance.state = Controller.ControlState.cutscene;
    }
    public void Finish() {
        complete = true;
        impAnimate.enabled = true;
        Toolbox.Instance.deactivateEventually(impSeller.leftPoint);
        Toolbox.Instance.deactivateEventually(impSeller.rightPoint);
    }

    public bool GetIngredients() {
        Intrinsics intrinsics = analyzand.GetComponent<Intrinsics>();

        Comparison<Buff> comparison = (Buff x, Buff y) => {
            // Less than 0	x is less than y.
            // 0	x equals y.
            // Greater than 0	x is greater than y.
            if (buffPriority.IndexOf(x.type) < buffPriority.IndexOf(y.type)) {
                return 1;
            } else if (buffPriority.IndexOf(x.type) == buffPriority.IndexOf(y.type)) {
                return 0;
            } else if (buffPriority.IndexOf(x.type) > buffPriority.IndexOf(y.type)) {
                return -1;
            }
            return 0;
        };
        if (intrinsics == null)
            return false;
        intrinsics.buffs.Sort(comparison);
        if (intrinsics.buffs.Count > 0) {
            buffType = intrinsics.buffs[0].type; // TODO: change
            potionData = buffMap[buffType];
            return true;
        } else if (intrinsics.liveBuffs.Count > 0) {
            buffType = intrinsics.liveBuffs[0].type; // TODO: change
            potionData = buffMap[buffType];
            return true;
        }
        return false;
    }
    public bool GetPotion() {
        LiquidContainer liquidContainer = analyzand.GetComponent<LiquidContainer>();
        foreach (KeyValuePair<BuffType, PotionData> kvp in buffMap) {
            if (kvp.Value.ingredient1.prefabName == analyzand.name || kvp.Value.ingredient2.prefabName == analyzand.name) {
                potionData = kvp.Value;
                ingredient = "What a lovely " + Toolbox.Instance.GetName(analyzand) + ".";
                return true;
            }
            if (liquidContainer != null && (kvp.Value.ingredient1.prefabName == liquidContainer.liquid.name || kvp.Value.ingredient2.prefabName == liquidContainer.liquid.name)) {
                potionData = kvp.Value;
                ingredient = "I see you have brought me " + liquidContainer.liquid.name + ".";
                return true;
            }
        }
        return false;
    }
    public override void CleanUp() {
        UINew.Instance.RefreshUI(active: true);
    }
}