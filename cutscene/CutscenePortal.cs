using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CutscenePortal : Cutscene {
    public enum Destination { none, venus, hell, magic, ceo };
    public Destination destination;
    private float timer;
    GameObject player;
    Transform playerTransform;
    LorentzAttractor attractor;
    MessageSpeech message;
    Controller playerController;
    public override void Configure() {
        player = GameManager.Instance.playerObject;
        playerTransform = player.transform;
        attractor = new LorentzAttractor();
        configured = true;

        // Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        // if (playerControllable != null) {
        //     playerControllable.enabled = false;
        // }
        playerController = new Controller(player);
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
            player.transform.position = Vector3.zero;
        }
        if (Random.Range(0, 75) < 1f) {
            message = new MessageSpeech("{disturbreact}");
            message.nimrod = true;
            message.interrupt = true;
            Toolbox.Instance.SendMessage(player, CutsceneManager.Instance, message);
        }
        timer += Time.deltaTime;
        if (destination == Destination.ceo && timer > 5f && GameManager.Instance.data.prefabName != "CEO") {
            playerController.Dispose();
            GameObject origPlayer = GameManager.Instance.playerObject;
            GameObject ceo = GameObject.Instantiate(Resources.Load("prefabs/CEO"), GameManager.Instance.playerObject.transform.position, Quaternion.identity) as GameObject;
            GameManager.Instance.SetFocus(ceo);
            GameObject.Destroy(origPlayer);
            GameManager.Instance.data.prefabName = "CEO";
            MySaver.Save();
            UINew.Instance.RefreshUI(active: false);

            playerTransform = ceo.transform;
            attractor = new LorentzAttractor();
            playerController = new Controller(ceo);
        }
        if (timer > 10.0f) {
            playerController.Dispose();
            complete = true;
            if (destination == Destination.none) {
                SceneManager.LoadScene("hells_landing");
                GameManager.Instance.data.entryID = 420;
            } else if (destination == Destination.venus) {
                SceneManager.LoadScene("venus1");
                GameManager.Instance.data.entryID = 420;
            } else if (destination == Destination.hell) {
                SceneManager.LoadScene("hells_landing");
                GameManager.Instance.data.entryID = 420;
            } else if (destination == Destination.magic) {
                SceneManager.LoadScene("hallucination");
                GameManager.Instance.data.entryID = 420;
            } else if (destination == Destination.ceo) {
                SceneManager.LoadScene("apartment");
                GameManager.Instance.data.entryID = -99;
            }
        }
        Vector3 lorentz = attractor.next(Time.deltaTime);
        playerTransform.position = lorentz * 0.01f;

        float xScale = 0.08f * Mathf.Sin(timer);
        float yScale = 0.08f * Mathf.Cos(timer);

        playerTransform.localScale = new Vector3(lorentz.z * xScale, lorentz.z * yScale, 1);
    }
}