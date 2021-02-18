using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutsceneTrigger : MonoBehaviour {
    public void OnDie() {
        if (GameManager.Instance.data.state == GameState.postCredits) {
            CutsceneManager.Instance.InitializeCutscene<CutscenePostCreditsDeath>();
        }
    }
}
