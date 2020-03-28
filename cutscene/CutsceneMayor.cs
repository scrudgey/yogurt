using UnityEngine;
using System.Collections.Generic;

public class CutsceneMayor : Cutscene {
    private GameObject spawnPoint;
    private GameObject mayor;
    private Humanoid mayorControl;
    private DecisionMaker mayorAI;
    private Speech mayorSpeech;
    private bool inPosition;
    private bool walkingAway;
    private Dictionary<Controllable, Controllable.ControlType> initialState = new Dictionary<Controllable, Controllable.ControlType>();
    public override void Configure() {
        configured = true;
        spawnPoint = GameObject.Find("mayorSpawnpoint");
        foreach (Controllable controllable in GameObject.FindObjectsOfType<Controllable>()) {
            initialState[controllable] = controllable.control;
            controllable.control = Controllable.ControlType.none;
        }
        mayor = GameObject.Instantiate(Resources.Load("prefabs/Mayor"), spawnPoint.transform.position, Quaternion.identity) as GameObject;
        mayorControl = mayor.GetComponent<Humanoid>();
        mayorAI = mayor.GetComponent<DecisionMaker>();
        mayorSpeech = mayor.GetComponent<Speech>();
        mayorSpeech.defaultMonologue = "mayor";
        mayorAI.enabled = false;
        Controllable playerController = GameManager.Instance.playerObject.GetComponent<Controllable>();
        playerController.SetDirection(Vector2.down);
        UINew.Instance.RefreshUI();
        MusicController.Instance.EnqueueMusic(new MusicMayor());
    }
    public override void Update() {
        if (!inPosition) {
            mayorControl.rightFlag = true;
        }
        if (!inPosition && Vector3.Distance(mayor.transform.position, GameManager.Instance.playerObject.transform.position) < 0.25) {
            inPosition = true;
            mayorControl.ResetInput();
            DialogueMenu menu = mayorSpeech.SpeakWith();
            menu.menuClosed += MenuWasClosed;
        }
        if (walkingAway) {
            mayorControl.leftFlag = true;
            if (mayor.transform.position.x < spawnPoint.transform.position.x) {
                UnityEngine.Object.Destroy(mayor);
                complete = true;
                Controller.Instance.state = Controller.ControlState.normal;
            }
        }
    }
    public override void CleanUp() {
        UINew.Instance.RefreshUI(active: true);
        foreach (KeyValuePair<Controllable, Controllable.ControlType> kvp in initialState) {
            kvp.Key.control = kvp.Value;
        }
        MusicController.Instance.End();
    }
    public void MenuWasClosed() {
        walkingAway = true;
        Controller.Instance.state = Controller.ControlState.cutscene;
        UINew.Instance.RefreshUI(active: false);
    }
}