using UnityEngine;
using System.Collections.Generic;

public class CutsceneThroneroom : Cutscene {
    GameObject polestar;
    public override void Configure() {
        configured = true;

        polestar = GameObject.Instantiate(Resources.Load("prefabs/Polestar_Superswan"), Vector3.zero, Quaternion.identity) as GameObject;
        Speech speech = polestar.GetComponent<Speech>();
        speech.defaultMonologue = "polestar_warning";

        DialogueMenu menu = speech.SpeakWith();
        menu.menuClosed += MenuWasClosed;
    }
    public void MenuWasClosed() {
        complete = true;
        // UINew.Instance.RefreshUI(active: false);
        GameObject.Destroy(polestar);
    }
}