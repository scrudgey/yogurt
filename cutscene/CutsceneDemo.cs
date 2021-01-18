using UnityEngine;
using System.Collections.Generic;

public class CutsceneDemo : Cutscene {
    GameObject polestar;
    public override void Configure() {
        configured = true;

        polestar = GameObject.Instantiate(Resources.Load("prefabs/Polestar_Superswan"), 10f * Vector3.one, Quaternion.identity) as GameObject;
        Speech speech = polestar.GetComponent<Speech>();
        speech.defaultMonologue = "polestar_demo";

        DialogueMenu menu = speech.SpeakWith();
        menu.menuClosed += MenuWasClosed;
    }
    public void MenuWasClosed() {
        complete = true;
        GameObject.Destroy(polestar);
        GameManager.Instance.NewDay();
    }
}