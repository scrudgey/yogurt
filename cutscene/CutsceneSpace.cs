using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CutsceneSpace : Cutscene {
    private float timer;
    GameObject player;
    public override void Configure() {
        if (configured)
            return;
        player = GameManager.Instance.playerObject;
        configured = true;
        player.transform.localScale = new Vector3(-1f, 1f, 1f);
        player.transform.rotation = Quaternion.identity;
        player.transform.RotateAround(player.transform.position, new Vector3(0f, 0f, 1f), 90f);

        Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        if (playerControllable != null) {
            playerControllable.enabled = false;
        }
        Speech playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        if (playerSpeech != null) {
            playerSpeech.enabled = false;
        }
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
            player.transform.position = Vector3.zero;
        }
        timer += Time.deltaTime;
        if (timer > 5.0f) {
            complete = true;
            SceneManager.LoadScene("moon1");
            GameManager.Instance.data.entryID = 420;
        }
    }
}