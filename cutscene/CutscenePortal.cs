using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CutscenePortal : Cutscene {
    public enum Destination { none, venus, hell };
    public Destination destination;
    private float timer;
    GameObject player;
    Transform playerTransform;
    LorentzAttractor attractor;
    public override void Configure() {
        player = GameManager.Instance.playerObject;
        playerTransform = player.transform;
        attractor = new LorentzAttractor();
        configured = true;
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
            player.transform.position = Vector3.zero;
        }
        timer += Time.deltaTime;
        if (timer > 10.0f) {
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
            }
        }
        Vector3 lorentz = attractor.next(Time.deltaTime);
        playerTransform.position = lorentz * 0.01f;

        float xScale = 0.08f * Mathf.Sin(timer);
        float yScale = 0.08f * Mathf.Cos(timer);

        playerTransform.localScale = new Vector3(lorentz.z * xScale, lorentz.z * yScale, 1);
    }
}