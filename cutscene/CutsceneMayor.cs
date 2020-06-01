using UnityEngine;
using System.Collections.Generic;

public class CutsceneMayor : Cutscene {
    private GameObject spawnPoint;
    private GameObject mayor;
    // private Humanoid mayorControl;
    private DecisionMaker mayorAI;
    private Speech mayorSpeech;
    private bool inPosition;
    private bool walkingAway;
    Controller playerController;
    Controller mayorController;
    public override void Configure() {
        configured = true;
        spawnPoint = GameObject.Find("mayorSpawnpoint");
        foreach (Controllable controllable in GameObject.FindObjectsOfType<Controllable>()) {
            controllable.enabled = false;
        }
        mayor = GameObject.Instantiate(Resources.Load("prefabs/Mayor"), spawnPoint.transform.position, Quaternion.identity) as GameObject;
        mayorController = new Controller(mayor);

        mayorAI = mayor.GetComponent<DecisionMaker>();
        mayorSpeech = mayor.GetComponent<Speech>();
        mayorSpeech.defaultMonologue = "mayor";
        mayorAI.enabled = false;
        Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        playerControllable.enabled = true;
        playerController = new Controller(playerControllable);
        playerController.SetDirection(Vector2.down);
        UINew.Instance.RefreshUI();
        MusicController.Instance.EnqueueMusic(new MusicMayor());
    }
    public override void Update() {
        if (!inPosition) {
            mayorController.rightFlag = true;
        }
        if (!inPosition && Vector3.Distance(mayor.transform.position, GameManager.Instance.playerObject.transform.position) < 0.25) {
            inPosition = true;
            mayorController.ResetInput();
            DialogueMenu menu = mayorSpeech.SpeakWith();
            menu.menuClosed += MenuWasClosed;
        }
        if (walkingAway) {
            mayorController.leftFlag = true;
            if (mayor.transform.position.x < spawnPoint.transform.position.x) {
                UnityEngine.Object.Destroy(mayor);
                complete = true;
                InputController.Instance.state = InputController.ControlState.normal;
            }
        }
    }
    public override void CleanUp() {
        mayorController.Deregister();
        playerController.Deregister();
        UINew.Instance.RefreshUI(active: true);
        foreach (Controllable controllable in GameObject.FindObjectsOfType<Controllable>()) {
            controllable.enabled = true;
        }
        MusicController.Instance.End();
    }
    public void MenuWasClosed() {
        walkingAway = true;
        InputController.Instance.state = InputController.ControlState.cutscene;
        UINew.Instance.RefreshUI(active: false);
    }
}