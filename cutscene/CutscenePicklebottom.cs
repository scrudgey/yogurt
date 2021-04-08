using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CutscenePickleBottom : Cutscene {
    CameraControl camControl;
    GameObject peter;
    PeterPicklebottom peterAI;
    GameObject nightShade;
    public override void Configure() {
        Doorway doorway = null;
        foreach (Doorway door in GameObject.FindObjectsOfType<Doorway>()) {
            if (door.entryID == 0 && !door.spawnPoint) {
                doorway = door;
            }
        }
        peter = GameObject.Instantiate(Resources.Load("prefabs/peter_picklebottom")) as GameObject;
        doorway.Enter(peter);
        camControl = GameObject.FindObjectOfType<CameraControl>();
        camControl.focus = peter;
        UINew.Instance.RefreshUI();
        nightShade = GameObject.Instantiate(Resources.Load("UI/nightShade")) as GameObject;
        nightShade.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        FadeInOut fader = nightShade.GetComponent<FadeInOut>();
        fader.enabled = false;
        peterAI = peter.GetComponent<PeterPicklebottom>();
        peterAI.door = doorway;
        peterAI.PlayThemeSong();
        peterAI.SetTargets(); // unnecessary?
        UINew.Instance.SetActionText("You have been visited by Peter Picklebottom");
        Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
        configured = true;
    }
    public override void Update() {
        camControl.focus = peter;
        if (peterAI.targets.Count == 0 && peterAI.target.val == null) {
            complete = true;
        }
    }
    public override void CleanUp() {
        camControl.focus = GameManager.Instance.playerObject;
        GameObject.Destroy(nightShade);
        peterAI.state = PeterPicklebottom.AIState.leave;
        UINew.Instance.SetActionText("");
    }
}