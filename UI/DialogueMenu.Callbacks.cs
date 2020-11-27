using System.Collections;
using UnityEngine;

public partial class DialogueMenu : MonoBehaviour {
    public void CheckForCommands(string text) {
        if (text == "IMPCALLBACK1") {
            UINew.Instance.CloseActiveMenu();
            CutsceneImp cutscene = (CutsceneImp)CutsceneManager.Instance.cutscene;
            cutscene.FirstIngredient();
        }
        if (text == "IMPCALLBACK2") {
            UINew.Instance.CloseActiveMenu();
            CutsceneImp cutscene = (CutsceneImp)CutsceneManager.Instance.cutscene;
            cutscene.SecondIngredient();
        }
        if (text == "IMPCALLBACK3") {
            UINew.Instance.CloseActiveMenu();
            CutsceneImp cutscene = (CutsceneImp)CutsceneManager.Instance.cutscene;
            cutscene.Finish();
        }

        // while (nextLine) {
        bool nextLine = false;
        if (text == "END") {
            if (UINew.Instance.activeMenuType == UINew.MenuType.dialogue)
                UINew.Instance.CloseActiveMenu();
        }
        if (text == "POLESTARCALLBACK") {
            PoleStarCallback();
            nextLine = true;
        }
        if (text == "VAMPIRETRAP") {
            doTrapDoor = true;
            nextLine = true;
        }
        if (text == "VAMPIREATTACK") {
            doVampireAttack = true;
            nextLine = true;
        }
        if (text == "TEXTSIZE:NORMAL") {
            textSize = TextSize.normal;
            nextLine = true;
        }
        if (text == "TEXTSIZE:LARGE") {
            textSize = TextSize.large;
            nextLine = true;
        }
        if (text == "MAYORAWARDCALLBACK") {
            MayorAward();
            nextLine = true;
        }
        if (text == "GODBLESS") {
            target.GetComponent<Godhead>().Bless();
            UINew.Instance.CloseActiveMenu();
        }
        if (text == "GODDESTROY") {
            target.GetComponent<Godhead>().Destroy();
            UINew.Instance.CloseActiveMenu();
        }
        if (text == "TRADERCALLBACK") {
            nextLine = true;
            TraderCallback();
        }
        if (text == "SMOOTHIE1") {
            SmoothieCallback(1);
            nextLine = true;
        }
        if (text == "SMOOTHIE2") {
            nextLine = true;
            SmoothieCallback(2);
        }
        if (text == "SMOOTHIE3") {
            nextLine = true;
            SmoothieCallback(3);
        }
        if (text == "INQUIRE") {
            nextLine = true;
            Debug.Log("inquire");
            Inquire();
        }
        if (text == "CHARONCALLBACK") {
            nextLine = true;
            UINew.Instance.CloseActiveMenu();
            BoxCollider2D loadingZone = GameObject.Find("charonStartZone").GetComponent<BoxCollider2D>();
            if (loadingZone.bounds.Contains(GameManager.Instance.playerObject.transform.position)) {
                CutsceneManager.Instance.InitializeCutscene<CutsceneCharon>();
            }
        }
        if (nextLine)
            NextLine();
    }
    public void SmoothieCallback(int idn) {
        GameObject smoothieSeller = null;
        foreach (DecisionMaker ai in GameObject.FindObjectsOfType<DecisionMaker>()) {
            if (ai.defaultPriorityType == DecisionMaker.PriorityType.SellSmoothies) {
                smoothieSeller = ai.gameObject;
            }
        }
        if (smoothieSeller == null)
            return;
        MessageSmoothieOrder message = new MessageSmoothieOrder(idn);
        Toolbox.Instance.SendMessage(smoothieSeller, instigator, message);
    }
    public void PoleStarCallback() {
        target.defaultMonologue = "polestar";
        GameManager.Instance.data.teleporterUnlocked = true;
        GameManager.Instance.data.cosmicName = GameManager.Instance.CosmicName();
    }
    public void VampireTrap() {
        TrapDoor trapdoor = GameObject.Find("trapdoor").GetComponent<TrapDoor>();
        trapdoor.Activate();
    }
    public void VampireAttack() {
        GameObject vampire = target.gameObject;
        MessageInsult message = new MessageInsult();
        Toolbox.Instance.SendMessage(vampire, instigator, message);
        Toolbox.Instance.SendMessage(vampire, instigator, message);
    }
    public void TraderCallback() {
        Trader trader = target.GetComponent<Trader>();
        Inventory playerInventory = instigator.GetComponent<Inventory>();
        if (playerInventory != null && trader != null) {
            trader.Trade(playerInventory);
        }
    }
    public void MayorAward() {
        GameObject mayor = GameObject.Find("Mayor");
        if (mayor != null) {
            Inventory mayorInventory = mayor.GetComponent<Inventory>();
            Controllable mayorControl = mayor.GetComponent<Controllable>();
            Speech mayorSpeech = mayor.GetComponent<Speech>();
            GameObject key = GameObject.Instantiate(Resources.Load("prefabs/key_to_city"), mayor.transform.position, Quaternion.identity) as GameObject;
            Pickup keyPickup = key.GetComponent<Pickup>();
            mayorInventory.GetItem(keyPickup);
            mayorSpeech.defaultMonologue = "mayor_normal";
            GameManager.Instance.data.mayorAwardToday = true;
            GameManager.Instance.StartCoroutine(AwardRoutine(mayorControl, mayorInventory));
        }
    }
    IEnumerator AwardRoutine(Controllable controllable, Inventory inv) {
        using (Controller control = new Controller(controllable)) {
            control.LookAtPoint(target.transform.position);

            yield return new WaitForSeconds(0.1f);
            control.ResetInput();
            control.LookAtPoint(GameManager.Instance.playerObject.transform.position);
            controllable.disabled = true;
            yield return new WaitForSeconds(1.0f);
            control.LookAtPoint(GameManager.Instance.playerObject.transform.position);
            // AudioClip congratsClip = Resources.Load("music/Short CONGRATS YC3") as AudioClip;
            MusicController.Instance.EnqueueMusic(new MusicCongrats());
            GameObject confetti = Resources.Load("particles/confetti explosion") as GameObject;
            // Toolbox.Instance.AudioSpeaker(congratsClip, controllable.transform.position);
            GameObject.Instantiate(confetti, controllable.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(3f);
            control.LookAtPoint(GameManager.Instance.playerObject.transform.position);
            inv.DropItem();
            yield return new WaitForSeconds(0.5f);
        }
    }
}